using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PerfectPad2D : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private float maxLandingVerticalSpeed = -6f;
    [SerializeField] private float perfectWindowSeconds = 0.1f;
    [SerializeField] private bool requireRecentDash = true;

    [Header("Juice / Feedback")]
    [SerializeField] private Transform cameraTransform; // Assign Main Camera here
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.2f;

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
            
            // Trigger the screen shake feedback
            if (cameraTransform != null)
            {
                StartCoroutine(ShakeCamera());
            }
        }
    }

    private IEnumerator ShakeCamera()
    {
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            // Create a random offset for the camera
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraTransform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return camera to its original spot
        cameraTransform.localPosition = originalPos;
    }
}
