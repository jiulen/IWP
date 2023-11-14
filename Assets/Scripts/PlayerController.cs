using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour, IPunObservable
{
    Rigidbody2D rb;
    Transform spriteTransform;

    [SerializeField] float airResistance = 0;
    [SerializeField] float friction = 0;

    [SerializeField] PlayerInfoUI playerInfoUI;
    public ControllerUI controllerUI;

    public int playerNum;

    [SerializeField] SpriteRenderer playerSr;
    public FreezeAnimator freezeAnimator;

    bool isGrounded = true;
    [SerializeField] bool facingLeft = false;
    public bool toFlip = false;

    List<PlayerActions> unavailableActions = new();

    public enum PlayerActions
    {
        //Movement
        WAIT,
        WALK_LEFT,
        WALK_RIGHT,
        ROLL,
        JUMP,
        FALL,

        //Defense
        BLOCK,
        BURST,

        //Attack
        HYDRO_BALL,
        ICICLE,
        LINGERING_SPIRIT,
        EXPLOSION,
        LIGHTNING,
        SEISMIC_STRIKE,
        WHIRLWIND,

        //Others
        STUNNED,
        NONE
    }

    public PlayerActions playerCurrentAction = PlayerActions.NONE;
    public int currentFrameNum = 0;

    //Player stats
    public float burstMeterValue = 0;
    public int knockbackMultiplier = 0;
    public int airOptionsAvail = 2;
    public const int airOptionsMax = 2;

    public bool allowMove = false;

    //Actions
    FrameBehaviour currentFrameBehaviour;
    PlayerWalk playerWalk;

    #region IPunObservable implementation

    //For syncing data via IPunObservable
    const byte BURST_FLAG = 1 << 0;
    const byte KNOCKBACK_FLAG = 1 << 1;
    const byte AIR_FLAG = 1 << 2;

    float syncedBurstMeterValue = 0;
    int syncedKnockbackMultiplier = 0;
    int syncedairOptionsAvail = 0;

    byte syncDataFlags;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            syncDataFlags = 0;

            //Check which variables have changed
            if (syncedBurstMeterValue != burstMeterValue)
            {
                syncDataFlags |= BURST_FLAG;
            }
            if (syncedKnockbackMultiplier != knockbackMultiplier)
            {
                syncDataFlags |= KNOCKBACK_FLAG;
            }
            if (syncedairOptionsAvail != airOptionsAvail)
            {
                syncDataFlags |= AIR_FLAG;
            }

            //Send data flags
            stream.SendNext(syncDataFlags);

            //Send variables that changed
            if ((syncDataFlags & BURST_FLAG) != 0)
            {
                stream.SendNext(burstMeterValue);
                syncedBurstMeterValue = burstMeterValue;
            }
            if ((syncDataFlags & KNOCKBACK_FLAG) != 0)
            {
                stream.SendNext(knockbackMultiplier);
                syncedKnockbackMultiplier = knockbackMultiplier;
            }
            if ((syncDataFlags & AIR_FLAG) != 0)
            {
                stream.SendNext(airOptionsAvail);
                syncedairOptionsAvail = airOptionsAvail;
            }
        }
        else
        {
            //Receive data flags
            syncDataFlags = (byte)stream.ReceiveNext();

            //Receive and update variables based on data flags
            if ((syncDataFlags & BURST_FLAG) != 0)
            {
                burstMeterValue = (float)stream.ReceiveNext();
            }
            if ((syncDataFlags & KNOCKBACK_FLAG) != 0)
            {
                knockbackMultiplier = (int)stream.ReceiveNext();
            }
            if ((syncDataFlags & AIR_FLAG) != 0)
            {
                airOptionsAvail = (int)stream.ReceiveNext();
            }
        }
    }

    #endregion

    private void Awake()
    {
        SetPlayerInfo();

        rb = GetComponent<Rigidbody2D>();
        spriteTransform = GetComponent<Transform>();

        playerWalk = GetComponent<PlayerWalk>();
    }

    void SetPlayerInfo()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
            {
                if ((int)playerNumber == playerNum)
                {
                    playerInfoUI.SetPlayerName(p.NickName);

                    if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_SKIN, out object playerSkinID))
                    {
                        playerSr.material.SetColor("_PlayerColor", ShooterGameInfo.GetColor((int)playerSkinID));

                        playerInfoUI.SetUISkin((int)playerSkinID);
                    }

                    return;
                }

            }
        }
    }

    public void UpdateInfoUIManual(float playerBurstMeterVal, int playerKnockbackMulti, int playerAirOptions)
    {
        playerInfoUI.UpdatePlayerInfo(playerBurstMeterVal, playerKnockbackMulti, playerAirOptions);
    }

    public void UpdateInfoUIAuto()
    {
        playerInfoUI.UpdatePlayerInfo(burstMeterValue, knockbackMultiplier, airOptionsAvail);
    }

    public void CheckMoves()
    {
        unavailableActions.Clear();

        //Check movement
        if (!isGrounded)
        {
            unavailableActions.Add(PlayerActions.WALK_LEFT);
            unavailableActions.Add(PlayerActions.WALK_RIGHT);

            if (airOptionsAvail <= 0)
            {
                unavailableActions.Add(PlayerActions.ROLL);

                unavailableActions.Add(PlayerActions.JUMP);
            }
        }
        else
        {
            if (airOptionsAvail <= 0)
            {
                unavailableActions.Add(PlayerActions.FALL);
            }
        }

        //Check burst
        if (!CanBurst())
        {
            unavailableActions.Add(PlayerActions.BURST);
        }
    }

    public bool CanBurst()
    {
        return burstMeterValue >= 1;
    }

    public bool IsIdle()
    {
        return playerCurrentAction == PlayerActions.NONE;
    }

    public void ShowControls(bool active)
    {
        CheckMoves();

        if (active)
        {
            //Set ui first
            bool forceBurst = !IsIdle() && CanBurst();
            controllerUI.SetUI(unavailableActions, forceBurst);
        }

        controllerUI.gameObject.SetActive(active);
    }

    public void RunFrameBehaviour()
    {
        if (toFlip)
        {
            toFlip = false;
            FlipPlayer();
        }

        if (currentFrameNum == 0)
        {
            switch (playerCurrentAction)
            {
                case PlayerActions.WALK_LEFT:
                    playerWalk.goLeft = true;

                    if (facingLeft) playerWalk.forwards = true;
                    else playerWalk.forwards = false;

                    currentFrameBehaviour = playerWalk;
                    break;
                case PlayerActions.WALK_RIGHT:
                    playerWalk.goLeft = false;

                    if (facingLeft) playerWalk.forwards = false;
                    else playerWalk.forwards = true;

                    currentFrameBehaviour = playerWalk;
                    break;
            }

            if (currentFrameBehaviour != null)
            {
                currentFrameBehaviour.enabledBehaviour = true;
                currentFrameBehaviour.lastFrame = false;
            }
        }

        if (currentFrameBehaviour != null)
        {
            currentFrameBehaviour.frameNum = currentFrameNum;
            currentFrameBehaviour.GoToFrame();

            if (currentFrameBehaviour.IsAnimationDone())
            {
                playerCurrentAction = PlayerActions.NONE;

                currentFrameBehaviour.enabledBehaviour = false;
                currentFrameBehaviour = null;
            }
        }
    }

    void ApplyAirResistance()
    {
        Vector2 resistiveForce = -rb.velocity * airResistance;

        rb.AddForce(resistiveForce, ForceMode2D.Impulse);
    }

    void ApplyFriction()
    {
        Vector2 resistiveForce = -rb.velocity * friction;

        rb.AddForce(resistiveForce, ForceMode2D.Impulse);
    }

    public void ApplyResistances()
    {
        ApplyAirResistance();

        if (isGrounded) ApplyFriction();
    }

    public void FlipPlayer()
    {
        spriteTransform.localScale = new Vector3(spriteTransform.localScale.x * -1, 1, 1);

        facingLeft = !facingLeft;
    }
}
