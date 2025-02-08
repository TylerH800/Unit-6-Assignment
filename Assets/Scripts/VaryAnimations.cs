using UnityEngine;

public class VaryAnimations : MonoBehaviour
{
    public float min, max;
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator.speed = Random.Range(min, max);
    }

}
