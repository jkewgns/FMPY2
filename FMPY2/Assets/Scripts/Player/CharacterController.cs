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
    [SerializeField] private float airAccelerationMultiplier = 0.65f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 18f;
    [SerializeField] private float coyoteTime = 0.08f;
    [SerializeField] private float jumpBufferTime = 0.10f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Gravity")]
    [SerializeField] private float gravityScale = 5f;
    [SerializeField] private float fallGravityMultiplier = 1.15f;
    [SerializeField] private float lowJumpGravityMultiplier = 1.25f;
    [SerializeField] private float maxFallSpeed = 30f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 24f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private float dashCooldown = 0.08f;
    [SerializeField] private bool allowGroundDash = false;

    [Header("Checks")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.52f, 0.08f);
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.08f, 0.72f);

    [Header("Anti Wall Stick")]
    [SerializeField] private bool antiWallStick = true;
    [SerializeField] private float wallUnstickMinFallSpeed = 6f;

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
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        TickTimers();
        EvaluateGrounded();
        EvaluateWallTouch();
        HandleJumpBuffer();
        HandleDash();
        Reset();
    }

    private void FixedUpdate()
    {
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
        _coyoteCounter -= Time.deltaTime;
        _jumpBufferCounter -= Time.deltaTime;
        _dashCooldownRemaining -= Time.deltaTime;

        if (_isDashing)
        {
            _dashTimeRemaining -= Time.deltaTime;
            if (_dashTimeRemaining <= 0f)
            {
                _isDashing = false;
            }
        }
    }

    private void EvaluateGrounded()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = groundCheck != null &&
                      Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        if (_isGrounded)
        {
            _coyoteCounter = coyoteTime;

            if (!_wasGrounded)
            {
                // Normal land restores dash by default.
                _dashAvailable = true;
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

        bool canDash = allowGroundDash || !_isGrounded;
        if (!canDash) return;

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
        float accel = Mathf.Abs(targetSpeed) > 0.01f ? runAcceleration : runDeceleration;
        if (!_isGrounded) accel *= airAccelerationMultiplier;

        float newX = Mathf.MoveTowards(_rb.linearVelocity.x, targetSpeed, accel * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(newX, _rb.linearVelocity.y);

        if (Mathf.Abs(_moveInput.x) > 0.01f)
            _lastFacingDirection = Mathf.Sign(_moveInput.x);
    }

    private void ApplyBetterGravity()
    {
        float g = gravityScale;

        if (_rb.linearVelocity.y < 0f)
            g *= fallGravityMultiplier;
        else if (_rb.linearVelocity.y > 0f && !_jumpHeld)
            g *= lowJumpGravityMultiplier;

        _rb.gravityScale = g;

        if (_rb.linearVelocity.y < -maxFallSpeed)
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -maxFallSpeed);
    }

    private void ApplyJumpCut()
    {
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
        if (!antiWallStick) return;
        if (_isGrounded) return;
        if (!_isTouchingWall) return;
        if (_rb.linearVelocity.y <= -wallUnstickMinFallSpeed) return;

        // Keeps the player from "gluing" to walls due to collider friction/contact.
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, -wallUnstickMinFallSpeed);
    }

    // Send Messages-compatible signatures for PlayerInput
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
        if(Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (groundCheck != null) Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.cyan;
        if (wallCheck != null) Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}