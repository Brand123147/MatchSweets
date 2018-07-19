using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearedSweet : MonoBehaviour {

    public AnimationClip clearAnimation;//获取外部动画组件
    public AudioClip clearSound;//声音


    //协程必须保证代码没有错误，要不然尽量少用，代码有问题协程可能导致unity直接崩掉
    private bool isClearing;

    public bool IsClearing
    {
        get
        {
            return isClearing;
        }

        set
        {
            isClearing = value;
        }
    }

    protected GameSweet sweet;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    public virtual void Clear()
    {
        isClearing = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null)//安全校验
        {
            animator.Play(clearAnimation.name);//播放消除动画
            //玩家得分+1，消一次+1，播放清除声音
            UIManager.gameScore++;
            AudioSource.PlayClipAtPoint(clearSound, transform.position); 
            yield return new WaitForSeconds(clearAnimation.length);//等待动画长度
            Destroy(gameObject);
        }
    }
}
