using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerInfoUI playerInfoUI;

    PhotonView photonView;
    public int playerNum;

    [SerializeField]
    SpriteRenderer playerSr;

    enum PlayerActions
    {
        //Movement

        WAIT,
        WALK_LEFT,
        WALK_RIGHT,
        ROLL,
        JUMP,
        FALL,

        //Attack


        //Defense
        BLOCK,
        BURST,

        //Others
        STUNNED,
        NONE
    }

    PlayerActions playerCurrentAction = PlayerActions.NONE;

    //Player stats
    public int knockbackMultiplier = 0;
    public float burstMeterValue = 0;
    public int airOptionsAvail = 2;
    public int airOptionsMax = 2;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

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
}
