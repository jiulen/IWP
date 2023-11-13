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
    protected string oldAnimName = "";

    [SerializeField] protected bool loopAnim = false;
    public int lastFrame;

    public bool enabledBehaviour = true;

    const float animationFPS = 60;

    #region IPunObservable implementation

    //For syncing data via IPunObservable
    const byte FRAME_NUM_FLAG = 1 << 0;
    const byte CUREENT_ANIM_FLAG = 1 << 1;

    int syncedFrameNum = 0;
    string syncedAnimName = "";

    byte syncDataFlags;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!enabledBehaviour) return;

        if (stream.IsWriting)
        {
            syncDataFlags = 0;

            //Check which variables have changed
            if (syncedFrameNum != frameNum)
            {
                syncDataFlags |= FRAME_NUM_FLAG;
            }
            if (syncedAnimName != currentAnimName)
            {
                syncDataFlags |= CUREENT_ANIM_FLAG;
            }

            //Send data flags
            stream.SendNext(syncDataFlags);

            //Send variables that changed
            if ((syncDataFlags & FRAME_NUM_FLAG) != 0)
            {
                stream.SendNext(frameNum);
                syncedFrameNum = frameNum;
            }
            if ((syncDataFlags & CUREENT_ANIM_FLAG) != 0)
            {
                stream.SendNext(currentAnimName);
                syncedAnimName = currentAnimName;
            }
        }
        else
        {
            //Receive data flags
            syncDataFlags = (byte)stream.ReceiveNext();

            //Receive and update variables based on data flags
            if ((syncDataFlags & FRAME_NUM_FLAG) != 0)
            {
                frameNum = (int)stream.ReceiveNext();
            }
            if ((syncDataFlags & CUREENT_ANIM_FLAG) != 0)
            {
                currentAnimName = (string)stream.ReceiveNext();
            }
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (!enabledBehaviour) return;

        if (currentAnimName != "")
        {
            AnimatorChangeAnimation(currentAnimName);
            AnimatorSetFrame();
        }
    }

    public virtual void GoToFrame() //Handles physics and switching animations + progressing through animations on host side
    {
        AnimatorSetFrame();
    }

    protected void AnimatorChangeAnimation(string animationName)
    {
        if (animationName != oldAnimName)
        {
            animator.PlayInFixedTime(animationName);
            oldAnimName = animationName;
        }
    }

    protected void AnimatorSetFrame()
    {
        timeInSeconds = frameNum / animationFPS;
        if (timeInSeconds > 1 && !loopAnim)
        {
            timeInSeconds = 1;
        }

        animator.PlayInFixedTime(currentAnimName, sr.sortingLayerID, timeInSeconds);
    }

    public bool IsAnimationDone()
    {
        return frameNum >= lastFrame;
    }
}