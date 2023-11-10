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

    [SerializeField] ControllerUI localControllerUI;

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

        localPlayerController.controllerUI = localControllerUI;
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
        //check when to show controls
        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_SHOW_CONTROLS))
        {
            if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_SHOW_CONTROLS, out object showControls))
            {
                if ((bool)showControls)
                {
                    //show this player's controls
                    localPlayerController.ShowControls(true);

                    Hashtable props = new Hashtable() { { ShooterGameInfo.PLAYER_SHOW_CONTROLS, false } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                }
            }
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
                localPlayerController.playerCurrentAction = PlayerController.PlayerActions.NONE;
                otherPlayerController.playerCurrentAction = PlayerController.PlayerActions.NONE;

                Hashtable playerActionProps = new Hashtable() { { ShooterGameInfo.PLAYER_SELECTED_ACTION, (int)PlayerController.PlayerActions.NONE },
                                                                { ShooterGameInfo.PLAYER_FLIP, false },
                                                                { ShooterGameInfo.PLAYER_SHOW_CONTROLS, true } };
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    p.SetCustomProperties(playerActionProps);
                }

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

        localPlayerController.UpdateInfoUIAuto();
        otherPlayerController.UpdateInfoUIAuto();

        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!gamePaused)
        {
            Physics2D.Simulate(Time.fixedDeltaTime);

            ++currentFrame;

            if (localPlayerController.IsIdle() || otherPlayerController.IsIdle())
            {
                OnPauseGame();
            }
        }
        else
        {
            //check if available players made their move
            if (localPlayerController.IsIdle() && localPlayerController.allowMove)
                return;
            if (otherPlayerController.IsIdle() && otherPlayerController.allowMove)
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
        if (localPlayerController.IsIdle())
        {
            //get list of unavailable moves
            localPlayerController.CheckMoves();

            localPlayerController.allowMove = true;
        }
        else
        {
            if (localPlayerController.CanBurst())
            {
                localPlayerController.allowMove = true;
            }
            else
            {
                localPlayerController.allowMove = false;
            }
        }

        //do otherplayer
        if (otherPlayerController.IsIdle())
        {
            //get list of unavailable moves
            otherPlayerController.CheckMoves();

            otherPlayerController.allowMove = true;
        }
        else
        {
            if (otherPlayerController.CanBurst())
            {
                otherPlayerController.allowMove = true;
            }
            else
            {
                otherPlayerController.allowMove = false;
            }
        }

    }
}
