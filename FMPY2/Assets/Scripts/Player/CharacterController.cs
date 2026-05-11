using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class DeadframePlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    [Header("Run")]
    [SerializeField] private float maxRunSpeed = 12f;
    [SerializeField] private float runAcceleration = 90f;
    [SerializeField] private float runDeceleration = 110f;
    [SerializeField] private float airAccelerationMultiplier = 0.65f; // Reduces control while in the air

    [Header("Jump")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float coyoteTime = 0.08f; // Grace period to jump after falling off a ledge
    [SerializeField] private float jumpBufferTime = 0.10f; // Stores jump input if pressed slightly before hitting ground
    [SerializeField] private float jumpCutMultiplier = 0.5f; // How much velocity is kept when releasing jump early

    [Header("Gravity")]
    [SerializeField] private float gravityScale = 5f;
    [SerializeField] private float fallGravityMultiplier = 1.15f; // Makes falling feel faster/weightier
    [SerializeField] private float lowJumpGravityMultiplier = 1.25f; // Applied when jump button is released early
    [SerializeField] private float maxFallSpeed = 30f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 24f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.08f;
    [SerializeField] private bool allowGroundDash = false; // If false, dash only works in mid-air

    [Header("Checks")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.52f, 0.08f);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.08f, 0.72f);

    [Header("Anti Wall Stick")]
    [SerializeField] private bool antiWallStick = true;
    [SerializeField] private float wallUnstickMinFallSpeed = 6f;

    // Public properties for external scripts (like Animators or VFX)
    public bool IsDashing => _isDashing;
    public bool DashAvailable => _dashAvailable;
    public float LastDashStartedTime { get; private set; }

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isTouchingWall;
    private bool _isDashing;

    private bool _dashPressed;
    private bool _dashAvailable = true;
    private bool _jumpHeld;
    private bool _jumpCutRequested;

    private Vector2 _dashDirection = Vector2.right;
    private float _lastFacingDirection = 1f;

    private float _coyoteCounter;
    private float _jumpBufferCounter;
    private float _dashTimeRemaining;
    private float _dashCooldownRemaining;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = gravityScale;
        // Interpolate smooths out movement for high-speed physics
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        TickTimers();
        EvaluateGrounded();
        EvaluateWallTouch();
        HandleJumpBuffer();
        HandleDash();
        Reset(); // Quick scene reload for debugging/testing
    }

    private void FixedUpdate()
    {
        // If dashing, bypass normal movement/gravity logic
        if (_isDashing)
        {
            _rb.linearVelocity = _dashDirection * dashSpeed;
            return;
        }

        ApplyHorizontalMovement();
        ApplyBetterGravity();
        ApplyJumpCut();
        ApplyAntiWallStick();
    }

    private void TickTimers()
    {
        // Count down all game-feel and ability timers
        _coyoteCounter -= Time.deltaTime;
        _jumpBufferCounter -= Time.deltaTime;
        _dashCooldownRemaining -= Time.deltaTime;

        if (_isDashing)
        {
            _dashTimeRemaining -= Time.deltaTime;
            if (_dashTimeRemaining <= 0f) _isDashing = false;
        }
    }

    private void EvaluateGrounded()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = groundCheck != null &&
                      Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (_isGrounded)
        {
            _coyoteCounter = coyoteTime; // Reset coyote time while on ground

            if (!_wasGrounded)
            {
                _dashAvailable = true; // Refresh dash ability upon landing
            }
        }
    }

    private void EvaluateWallTouch()
    {
        _isTouchingWall = wallCheck != null &&
                          Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, groundLayer);
    }

    private void HandleJumpBuffer()
    {
        // If we have a jump stored in the buffer AND we are within coyote time (on or near ground)
        if (_jumpBufferCounter > 0f && _coyoteCounter > 0f)
        {
            _jumpBufferCounter = 0f;
            _coyoteCounter = 0f;

            Vector2 v = _rb.linearVelocity;
            v.y = jumpForce;
            _rb.linearVelocity = v;
        }
    }

    private void HandleDash()
    {
        if (!_dashPressed || _dashCooldownRemaining > 0f || !_dashAvailable || _isDashing)
            return;

        _dashPressed = false;

        // Check if dashing is allowed based on ground status
        bool canDash = allowGroundDash || !_isGrounded;
        if (!canDash) return;

        // If no direction is held, dash in the direction the player is facing
        Vector2 raw = _moveInput;
        if (raw.sqrMagnitude < 0.01f)
            raw = new Vector2(_lastFacingDirection, 0f);

        _dashDirection = raw.normalized;
        _isDashing = true;
        _dashAvailable = false;
        _dashTimeRemaining = dashDuration;
        _dashCooldownRemaining = dashCooldown;
        LastDashStartedTime = Time.time;

        _rb.linearVelocity = _dashDirection * dashSpeed;
    }

    private void ApplyHorizontalMovement()
    {
        float targetSpeed = _moveInput.x * maxRunSpeed;
        
        // Use acceleration if moving, deceleration if stopping
        float accel = Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration : runDeceleration;
        
        // Air control is usually less snappy than ground control
        if (!_isGrounded) accel *= airAccelerationMultiplier;

        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);

        // Store facing direction for future dashes
        if (Mathf.Abs(_moveInput.x) > 0.01f)
            _lastFacingDirection = Mathf.Sign(_moveInput.x);
    }

    private void ApplyBetterGravity()
    {
        float g = gravityScale;

        // Apply extra gravity when falling or doing a "short jump"
        if (_rb.linearVelocity.y < 0f)
            g *= fallGravityMultiplier;
        else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            g *= lowJumpGravityMultiplier;

        _rb.gravityScale = g;

        // Terminal velocity clamp
        if (_rb.linearVelocity.y < -maxFallSpeed)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -maxFallSpeed);
    }

    private void ApplyJumpCut()
    {
        // If the player let go of jump, instantly reduce upward velocity
        if (!_jumpCutRequested) return;

        if (_rb.linearVelocity.y > 0f)
        {
            _rb.linearVelocity = new Vector2(
                _rb.linearVelocity.x,
                _rb.linearVelocity.y * jumpCutMultiplier
            );
        }

        _jumpCutRequested = false;
    }

    private void ApplyAntiWallStick()
    {
        if (!antiWallStick || _isGrounded || !_isTouchingWall) return;
        
        // Prevent player from sticking to walls due to physics friction
        if (_rb.linearVelocity.y > -wallUnstickMinFallSpeed)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -wallUnstickMinFallSpeed);
        }
    }

    // --- Input Callbacks (PlayerInput Component) ---

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            _jumpBufferCounter = jumpBufferTime;
            _jumpHeld = true;
        }
        else
        {
            _jumpHeld = false;
            _jumpCutRequested = true;
        }
    }

    public void OnDash(InputValue value)
    {
        if (value.isPressed)
            _dashPressed = true;
    }

    public void RefreshDashFromPerfectLanding()
    {
        _dashAvailable = true;
    }

    private void Reset()
    {
        // Debug helper to quickly restart the level
        if(Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection boxes in the Scene view
        Gizmos.color = Color.green;
        if (groundCheck != null) Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.cyan;
        if (wallCheck != null) Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}
