using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FrameBehaviour : MonoBehaviour, IPunObservable
{
    [SerializeField] protected SpriteRenderer sr;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;

    public int frameNum = 0;
    protected float timeInSeconds = 0;
    protected string currentAnimName = "";

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //own this player, send others data
            stream.SendNext(frameNum);
            stream.SendNext(currentAnimName);
        }
        else
        {
            //network player, receive data
            frameNum = (int)stream.ReceiveNext();
            currentAnimName = (string)stream.ReceiveNext();

            if (currentAnimName != "")
            {
                AnimatorChangeAnimation(currentAnimName);
                AnimatorSetTime();
                AnimatorSetFrame();
            }
        }
    }

    #endregion

    private void Awake()
    {
        animator.speed = 0;
    }

    public virtual void SetAnimation()
    {

    }

    public virtual void GoToFrame()
    {
        AnimatorSetFrame();
    }

    protected void AnimatorChangeAnimation(string animationName)
    {
        animator.PlayInFixedTime(animationName);
    }

    public void AnimatorSetTime()
    {
        if (frameNum == 0)
        {
            timeInSeconds = 0;
        }
        else
        {
            timeInSeconds = frameNum / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
            if (timeInSeconds > 1)
            {
                timeInSeconds = 1;
            }
        }
    }

    protected void AnimatorSetFrame()
    {
        animator.PlayInFixedTime(currentAnimName, sr.sortingLayerID, timeInSeconds);
    }
}
