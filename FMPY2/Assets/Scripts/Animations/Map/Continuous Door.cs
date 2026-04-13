using UnityEngine;

public class ContinuousDoor : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        animator.Play("Continuous Door");
    }
}