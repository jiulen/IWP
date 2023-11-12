using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] PlayerInfoUI playerInfoUI;
    public ControllerUI controllerUI;

    public int playerNum;

    [SerializeField]
    SpriteRenderer playerSr;

    bool isGrounded = true;

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
    public int knockbackMultiplier = 0;
    public float burstMeterValue = 0;
    public int airOptionsAvail = 2;
    public const int airOptionsMax = 2;

    public bool allowMove = false;

    //Actions
    FrameBehaviour currentFrameBehaviour;
    PlayerWalk playerWalk;

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //own this player, send others data
            stream.SendNext(burstMeterValue);
            stream.SendNext(knockbackMultiplier);
            stream.SendNext(airOptionsAvail);
        }
        else
        {
            //network player, receive data
            burstMeterValue = (float)stream.ReceiveNext();
            knockbackMultiplier = (int)stream.ReceiveNext();
            airOptionsAvail = (int)stream.ReceiveNext();
        }
    }

    #endregion

    private void Awake()
    {
        SetPlayerInfo();

        playerWalk = GetComponent<PlayerWalk>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
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
        if (currentFrameNum == 0)
        {
            switch (playerCurrentAction)
            {
                case PlayerActions.WALK_LEFT:
                    playerWalk.goLeft = true;
                    playerWalk.forwards = false;
                    currentFrameBehaviour = playerWalk;
                    break;
                case PlayerActions.WALK_RIGHT:
                    playerWalk.goLeft = false;
                    playerWalk.forwards = true;
                    currentFrameBehaviour = playerWalk;
                    break;
            }

            currentFrameBehaviour.SetAnimation();
        }

        //currentFrameBehaviour.frameNum = currentFrameNum;
        currentFrameBehaviour.AnimatorSetTime();
        currentFrameBehaviour.GoToFrame();
    }
}
