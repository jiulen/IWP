using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerInfoUI playerInfoUI;

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
        EXPLOSION_NEUTRAL,
        EXPLOSION_LEFT,
        EXPLOSION_RIGHT,
        EXPLOSION_UP,
        EXPLOSION_DOWN,

        GEYSER_LEFT,
        GEYSER_RIGHT,
        GEYSER_UP,
        GEYSER_DOWN,

        STONEFIST_LEFT,
        STONEFIST_RIGHT,
        STONEFIST_UP,
        STONEFIST_DOWN,

        WHIRLWIND_NEUTRAL,
        WHIRLWIND_LEFT,
        WHIRLWIND_RIGHT,
        WHIRLWIND_UP,
        WHIRLWIND_DOWN,

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

    public bool allowMove = false;

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
        if (burstMeterValue < 1)
        {
            unavailableActions.Add(PlayerActions.BURST);
        }
    }
}
