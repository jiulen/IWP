using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FrameBehaviour : MonoBehaviour, IPunObservable
{
    protected SpriteRenderer sr;
    protected Rigidbody2D rb;
    protected Animator animator;

    public int frameNum = 0;
    protected float timeInSeconds = 0;
    protected string currentAnimName = "";
    protected string oldAnimName = "";

    public int maxCooldown = 1;
    public int currentCooldown = 1;

    public float srSizeY = 1;

    public bool lastFrame;

    public bool enabledBehaviour = true;

    const float animationFPS = 60;

    #region IPunObservable implementation

    //For syncing data via IPunObservable
    const byte ENABLED_FLAG = 1 << 0;
    const byte FRAME_NUM_FLAG = 1 << 1;
    const byte CURRENT_ANIM_FLAG = 1 << 2;
    const byte CURRENT_COOLDOWN_FLAG = 1 << 3;
    const byte SR_SIZE_Y = 1 << 4;

    bool syncedEnabledBehaviour = true;
    int syncedFrameNum = 0;
    string syncedAnimName = "";
    int syncedCooldown = 0;
    float syncedSrSizeY = 1;

    byte syncDataFlags;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            syncDataFlags = 0;

            //Check which variables have changed
            if (syncedEnabledBehaviour != enabledBehaviour)
            {
                syncDataFlags |= ENABLED_FLAG;
            }
            if (syncedFrameNum != frameNum)
            {
                syncDataFlags |= FRAME_NUM_FLAG;
            }
            if (syncedAnimName != currentAnimName)
            {
                syncDataFlags |= CURRENT_ANIM_FLAG;
            }
            if (syncedCooldown != currentCooldown)
            {
                syncDataFlags |= CURRENT_COOLDOWN_FLAG;
            }
            if (syncedSrSizeY != srSizeY)
            {
                syncDataFlags |= SR_SIZE_Y;
            }

            //Send data flags
            stream.SendNext(syncDataFlags);

            //Send variables that changed
            if ((syncDataFlags & ENABLED_FLAG) != 0)
            {
                stream.SendNext(enabledBehaviour);
                syncedEnabledBehaviour = enabledBehaviour;
            }

            if ((syncDataFlags & CURRENT_COOLDOWN_FLAG) != 0)
            {
                stream.SendNext(currentCooldown);
                syncedCooldown = currentCooldown;
            }

            if (enabledBehaviour)
            {
                if ((syncDataFlags & FRAME_NUM_FLAG) != 0)
                {
                    stream.SendNext(frameNum);
                    syncedFrameNum = frameNum;
                }
                if ((syncDataFlags & CURRENT_ANIM_FLAG) != 0)
                {
                    stream.SendNext(currentAnimName);
                    syncedAnimName = currentAnimName;
                }
                if ((syncDataFlags & SR_SIZE_Y) != 0)
                {
                    stream.SendNext(srSizeY);
                    syncedSrSizeY = srSizeY;
                }
            }
        }
        else
        {
            //Receive data flags
            syncDataFlags = (byte)stream.ReceiveNext();

            //Receive and update variables based on data flags
            if ((syncDataFlags & ENABLED_FLAG) != 0)
            {
                enabledBehaviour = (bool)stream.ReceiveNext();
            }

            if ((syncDataFlags & CURRENT_COOLDOWN_FLAG) != 0)
            {
                currentCooldown = (int)stream.ReceiveNext();
            }

            if (enabledBehaviour)
            {
                if ((syncDataFlags & FRAME_NUM_FLAG) != 0)
                {
                    frameNum = (int)stream.ReceiveNext();
                }
                if ((syncDataFlags & CURRENT_ANIM_FLAG) != 0)
                {
                    currentAnimName = (string)stream.ReceiveNext();
                }
                if ((syncDataFlags & SR_SIZE_Y) != 0)
                {
                    srSizeY = (float)stream.ReceiveNext();
                    if (sr != null)
                    {
                        sr.size = new Vector2(1, srSizeY);
                    }
                }

                if (currentAnimName != "")
                {
                    AnimatorChangeAnimation(currentAnimName);
                    AnimatorSetFrame();
                }
            }
        }
    }

    #endregion

    protected virtual void Awake()
    {
        syncedEnabledBehaviour = enabledBehaviour;

        currentCooldown = -1;
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void FixedUpdate()
    {
        if ((ShooterGameManager.Instance.gameStarted && ShooterGameManager.Instance.gamePaused) || ShooterGameManager.Instance.isReplay)
        {
            if (enabledBehaviour)
            {
                if (currentAnimName != "")
                {
                    AnimatorChangeAnimation(currentAnimName);
                    AnimatorSetFrame();
                }
            }
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

    public void AnimatorSetFrame()
    {
        if (frameNum < 0)
        {
            timeInSeconds = 0;
        }
        else
        {
            timeInSeconds = frameNum / animationFPS;
        }

        animator.PlayInFixedTime(currentAnimName, sr.sortingLayerID, timeInSeconds);
    }

    public bool IsAnimationDone()
    {
        return lastFrame;
    }

    public virtual void EndAnimation()
    {
        lastFrame = true;
        enabledBehaviour = false;
    }  
}