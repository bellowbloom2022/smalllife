using UnityEngine;

public class UIStarController : MonoBehaviour
{
    private Animator animator;
    public bool HasFilled { get; private set; } = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayFillAnimation()
    {
        if (animator == null) animator = GetComponent<Animator>();
        animator.Play("FillStar",0 , 0f); //播放一次FillStar动画
        HasFilled = true;

        Debug.Log($"播放 FillStar 动画：{gameObject.name}");
    }

    public void ResetState()
    {
        if (animator == null) return;

        animator.Play("default", 0, 0);
        HasFilled = false;
    }
    
    public void SetFilled(){
        if (animator == null) animator = GetComponent<Animator>();
        animator.Play("Filled");
    }

    public void SetEmpty(){
        if (animator == null) animator = GetComponent<Animator>();
        animator.Play("default");
    }
}
