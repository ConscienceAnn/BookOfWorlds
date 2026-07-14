using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int IsCollectingParam = Animator.StringToHash("IsCollecting");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetIdle()
    {
        if (animator == null) return;
        animator.SetFloat(SpeedParam, 0f);
        animator.SetBool(IsMovingParam, false);
        animator.SetBool(IsCollectingParam, false);
    }

    public void SetWalk()
    {
        if (animator == null) return;
        animator.SetFloat(SpeedParam, 0.5f);
        animator.SetBool(IsMovingParam, true);
        animator.SetBool(IsCollectingParam, false);
    }

    public void SetRun()
    {
        if (animator == null) return;
        animator.SetFloat(SpeedParam, 1f);
        animator.SetBool(IsMovingParam, true);
        animator.SetBool(IsCollectingParam, false);
    }

    public void SetCollect()
    {
        if (animator == null) return;
        animator.SetFloat(SpeedParam, 0f);
        animator.SetBool(IsMovingParam, false);
        animator.SetBool(IsCollectingParam, true);
    }
}