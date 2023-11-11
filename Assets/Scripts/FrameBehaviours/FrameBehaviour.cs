using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FrameBehaviour : MonoBehaviour, IPunObservable
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Rigidbody rb;
    [SerializeField] Animator animator;

    protected uint frameNum = 0;

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //own this player, send others data
            stream.SendNext(frameNum);
        }
        else
        {
            //network player, receive data
            frameNum = (uint)stream.ReceiveNext();
        }
    }

    #endregion

    public virtual void GoToFrame()
    {
        AnimatorSetFrame();
    }

    protected void AnimatorChangeAnimation(string animationName)
    {
        animator.PlayInFixedTime(animationName);
    }

    protected void AnimatorSetFrame()
    {
        float timeInSeconds = frameNum / animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;

        animator.PlayInFixedTime(0, sr.sortingLayerID, timeInSeconds);
    }
}
