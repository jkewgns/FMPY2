using UnityEngine;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(Collider2D))]
public class PerfectPad2D : MonoBehaviour
{
    [SerializeField] private float maxLandingVerticalSpeed = -6f;
    [SerializeField] private float perfectWindowSeconds = 0.1f;
    [SerializeField] private bool requireRecentDash = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.TryGetComponent(out DeadframePlayerController2D player))
        {
            return;
        }

        bool descendingFastEnough = collision.relativeVelocity.y <= maxLandingVerticalSpeed;
        bool insidePerfectWindow = Time.time - player.LastDashStartedTime <= perfectWindowSeconds;

        if (descendingFastEnough && (!requireRecentDash || insidePerfectWindow))
        {
            player.RefreshDashFromPerfectLanding();
        }
    }
}