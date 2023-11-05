using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
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
    public int burstMeterValue = 0;
    public int burstMeterMax = 100;
    public int airOptionsAvail = 2;
    public int airOptionsMax = 2;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();

        SetPlayerSkin();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetPlayerSkin()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
            {
                if ((int)playerNumber == playerNum)
                {
                    if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_SKIN, out object playerSkinID))
                    {
                        playerSr.material.SetColor("_PlayerColor", ShooterGameInfo.GetColor((int)playerSkinID));

                        return;
                    }
                }

            }
        }
    }
}
