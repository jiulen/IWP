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

    //Player stats
    public int knockbackMultiplier = 0;
    public float burstMeterValue = 0;
    public int airOptionsAvail = 2;
    public const int airOptionsMax = 2;
    //Old player stats
    int oldKnockbackMultiplier = 0;
    float oldBurstMeterValue = 0;
    int oldAirOptionsAvail = 2;

    public bool allowMove = false;

    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //own this player, send others data
            if (burstMeterValue != oldBurstMeterValue)
            {
                stream.SendNext(true); //burst meter changed
                stream.SendNext(burstMeterValue);
                oldBurstMeterValue = burstMeterValue;
            }
            else
            {
                stream.SendNext(false); //burst meter no change
            }
            if (knockbackMultiplier != oldKnockbackMultiplier)
            {
                stream.SendNext(true); //knockback changed
                stream.SendNext(knockbackMultiplier);
                oldKnockbackMultiplier = knockbackMultiplier;
            }
            else
            {
                stream.SendNext(false); //knockback no change
            }
            if (airOptionsAvail != oldAirOptionsAvail)
            {
                stream.SendNext(true); //air options changed
                stream.SendNext(airOptionsAvail);
                oldAirOptionsAvail = airOptionsAvail;
            }
            else
            {
                stream.SendNext(false); //air options no change
            }
        }
        else
        {
            //network player, receive data
            if ((bool)stream.ReceiveNext())
            {
                burstMeterValue = (float)stream.ReceiveNext();
            }
            if ((bool)stream.ReceiveNext())
            {
                knockbackMultiplier = (int)stream.ReceiveNext();
            }
            if ((bool)stream.ReceiveNext())
            {
                airOptionsAvail = (int)stream.ReceiveNext();
            }
        }
    }

    #endregion

    private void Awake()
    {
        SetPlayerInfo();
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
}
