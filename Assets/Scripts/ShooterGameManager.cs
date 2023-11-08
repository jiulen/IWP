using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class ShooterGameManager : MonoBehaviourPunCallbacks
{
    public static ShooterGameManager Instance = null;

    public TMP_Text InfoText;

    [SerializeField] PlayerController player1, player2;
    PlayerController localPlayerController, otherPlayerController;

    bool gameStarted = false;
    bool gamePaused = true;
    int currentFrame;

    public void Awake()
    {
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public void Start()
    {
        //Notify ready
        Hashtable props = new()
        {
            {ShooterGameInfo.PLAYER_LOADED_LEVEL, true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        //Set local controller for easier access
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
        {
            switch (playerNumber)
            {
                case 1:
                    localPlayerController = player1;
                    otherPlayerController = player2;
                    break;
                case 2:
                    localPlayerController = player2;
                    otherPlayerController = player1;
                    break;
            }
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    private IEnumerator EndOfGame(string winner, bool draw = false) //TODO
    {
        float timer = 2.0f;

        while (timer > 0.0f)
        {
            if (!draw)
            {
                InfoText.text = string.Format("{0} wins", winner);
            }
            else
            {
                InfoText.text = "DRAW";
            }

            yield return new WaitForEndOfFrame();

            timer -= Time.deltaTime;
        }

        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {

        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckEndOfGame();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //update player ui (might move this to only run on non-host)
        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_BURST) || 
            changedProps.ContainsKey(ShooterGameInfo.PLAYER_KNOCKBACK) || 
            changedProps.ContainsKey(ShooterGameInfo.PLAYER_AIR))
        {
            //get stats
            targetPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_BURST, out object playerBurst);
            targetPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_KNOCKBACK, out object playerKnockback);
            targetPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_AIR, out object playerAir);

            //check which player
            PlayerController controllerToChange = null;

            if (targetPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
            {
                switch (playerNumber)
                {
                    case 1:
                        controllerToChange = player1;
                        break;
                    case 2:
                        controllerToChange = player2;
                        break;
                }
            }

            controllerToChange.UpdateInfoUIManual((float)playerBurst, (int)playerKnockback, (int)playerAir);
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        //check when to start
        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_LOADED_LEVEL))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                Hashtable actionProps = new Hashtable
                {
                    //init player stats
                    { ShooterGameInfo.PLAYER_BURST, 0f},
                    { ShooterGameInfo.PLAYER_KNOCKBACK, 0},
                    { ShooterGameInfo.PLAYER_AIR, 2}
                };
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    p.SetCustomProperties(actionProps);
                }

                localPlayerController.playerCurrentAction = PlayerController.PlayerActions.NONE;
                otherPlayerController.playerCurrentAction = PlayerController.PlayerActions.NONE;

                gameStarted = true;
                gamePaused = true;
            }
        }
    }

    private bool CheckAllPlayerLoadedLevel()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
            {
                if ((bool)playerLoadedLevel)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }

    private void CheckEndOfGame() //TODO
    {
        bool playerDead = false;
        string winner = "";
        bool draw = true;

        if (playerDead)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            StartCoroutine(EndOfGame(winner, draw));
        }
    }

    //Game parts

    private void FixedUpdate()
    {
        if (!gameStarted)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!gamePaused)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);

            //might have to update ui host and client side separately
            //localPlayerController.UpdateInfoUIAuto();
            //otherPlayerController.UpdateInfoUIAuto();

            ++currentFrame;

            if (localPlayerController.playerCurrentAction == PlayerController.PlayerActions.NONE || 
                otherPlayerController.playerCurrentAction == PlayerController.PlayerActions.NONE)
            {
                OnPauseGame();
            }
        }
        else
        {
            //check if available players made their move
            if (localPlayerController.playerCurrentAction == PlayerController.PlayerActions.NONE && localPlayerController.allowMove)
                return;
            if (otherPlayerController.playerCurrentAction == PlayerController.PlayerActions.NONE && otherPlayerController.allowMove)
                return;

            //prepare to continue game
            localPlayerController.allowMove = false;
            otherPlayerController.allowMove = false;

            gamePaused = false;
        }
    }

    private void OnPauseGame()
    {
        //pause game
        gamePaused = true;

        //do localplayer

        //check can move
        if (localPlayerController.playerCurrentAction == PlayerController.PlayerActions.NONE)
        {
            //get list of unavailable moves
            localPlayerController.CheckMoves();

            localPlayerController.allowMove = true;
        }
        else
        {
            localPlayerController.allowMove = false;
        }

        //do otherplayer

            
    }
}
