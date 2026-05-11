using UnityEngine;

public class ContinuousDoor : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to this door
        animator = GetComponent<Animator>();

        // Force the Animator to play the specific animation clip named "Continuous Door"
        // This is useful if you want to bypass the default state in the Animator Controller
        animator.Play("Continuous Door");
    }
}
