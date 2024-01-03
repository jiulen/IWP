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

    [SerializeField] List<SpellFrameBehaviour> spellsPool = new();
    [SerializeField] Transform spellsPoolTransform;
    [SerializeField] Transform spellsSpawnPoint;
    const int amtToPool = 5;

    public bool gameStarted = false;
    public bool gamePaused = true;
    public bool gameOver = false;
    public int currentFrame;

    //Room properties
    const string GAME_STARTED = "GameStarted";
    const string GAME_PAUSED = "GamePaused";

    //Game over stuff
    [SerializeField] Animator gameOverSlide, gameOverFade;
    [SerializeField] TMP_Text winnerText;

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
            {ShooterGameInfo.PLAYER_LOADED_LEVEL, true},
            {ShooterGameInfo.PLAYER_DEAD, false}
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

    private IEnumerator EndOfGame()
    {
        gameOverSlide.Play("GameOverSlide");

        yield return new WaitForSeconds(0.5f);

        gameOverFade.Play("GameOverFade");

        float timer = 3.5f;

        while (timer > 0.0f)
        {
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (gameOver)
            return;

        CheckEndOfGame();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (gameOver)
            return;

        //check if any player dead (out of stage)
        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_DEAD))
        {
            CheckEndOfGame();
        }

        //check when to show controls
        if (targetPlayer.IsLocal)
        {
            if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_SHOW_CONTROLS))
            {
                if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_SHOW_CONTROLS, out object showControls))
                {
                    if ((bool)showControls)
                    {
                        //set player current action first
                        if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_CURRENT_ACTION, out object playerCurrAction))
                        {
                            localPlayerController.playerCurrentAction = (PlayerController.PlayerActions)(int)playerCurrAction;
                        }

                        //set grounded last
                        if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_GROUNDED, out object grounded))
                        {
                            localPlayerController.ForceSetGrounded((bool)grounded);

                            //show this player's controls
                            localPlayerController.ShowControls(true);

                            Hashtable props = new Hashtable() { { ShooterGameInfo.PLAYER_SHOW_CONTROLS, false } };
                            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                        }
                    }
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

                Hashtable playerInitProps = new Hashtable() { { ShooterGameInfo.PLAYER_SELECTED_ACTION, (int)PlayerController.PlayerActions.NONE },
                                                              { ShooterGameInfo.PLAYER_FLIP, false },
                                                              { ShooterGameInfo.PLAYER_SHOW_CONTROLS, true },
                                                              { ShooterGameInfo.PLAYER_GROUNDED, true } };
                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    p.SetCustomProperties(playerInitProps);
                }

                gameStarted = true;
                gamePaused = true;

                Hashtable roomProps = new Hashtable() { { GAME_STARTED, true }, { GAME_PAUSED, true } };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                localPlayerController.allowMove = true;
                otherPlayerController.allowMove = true;
            }
        }

        if (gamePaused)
        {
            //check when to continue
            if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_SELECTED_ACTION))
            {
                PlayerController currentController;
                bool stopPause = true;

                foreach (Player p in PhotonNetwork.PlayerList)
                {
                    if (p.IsLocal)
                    {
                        currentController = localPlayerController;
                    }
                    else
                    {
                        currentController = otherPlayerController;
                    }

                    if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_SELECTED_ACTION, out object playerAction))
                    {
                        PlayerController.PlayerActions selectedAction = (PlayerController.PlayerActions)(int)playerAction;

                        if (selectedAction == PlayerController.PlayerActions.NONE && currentController.allowMove)
                        {
                            stopPause = false;
                        }
                        else if (currentController.allowMove)
                        {
                            if (selectedAction != PlayerController.PlayerActions.CONTINUE_BLOCK)
                            {
                                currentController.playerCurrentAction = selectedAction;
                                if (selectedAction != PlayerController.PlayerActions.SKIP) currentController.currentFrameNum = 0;
                            }

                            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_FLIP, out object playerFlip))
                            {
                                currentController.toFlip = (bool)playerFlip;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Selected action not set");
                        stopPause = false;
                    }
                }

                if (stopPause)
                {
                    Debug.Log("Game continue");

                    gamePaused = false;

                    Hashtable roomProps = new Hashtable() { { GAME_PAUSED, false } };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

                    localPlayerController.allowMove = false;
                    otherPlayerController.allowMove = false;
                }
            }
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        //Sync game started for client only
        if (propertiesThatChanged.TryGetValue(GAME_STARTED, out object isGameStarted))
        {
            gameStarted = (bool)isGameStarted;
        }
        //Sync game paused for client only
        if (propertiesThatChanged.TryGetValue(GAME_PAUSED, out object isGamePaused))
        {
            gamePaused = (bool)isGamePaused;
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
        string winner = ""; //if draw then winner will be ""

        if (PhotonNetwork.PlayerList.Length == 1)
        {
            winner = PhotonNetwork.PlayerList[0].NickName;
            playerDead = true;
        }
        else
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_DEAD, out object isDead))
                {
                    if (!(bool)isDead)
                    {
                        winner = p.NickName;
                    }
                    else
                    {
                        playerDead = true;
                    }
                }
            }
        }

        if (playerDead)
        {
            gameOver = true;

            if (winner != "")
            {
                winnerText.text = winner + " WINS";
            }
            else
            {
                winnerText.text = "DRAW?";
            }

            StartCoroutine(EndOfGame());
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
            //spells
            foreach (SpellFrameBehaviour spell in spellsPool)
            {
                if (spell.activeSpell)
                {
                    ++spell.frameNum; //add frame first since start is -1

                    spell.GoToFrame();
                    if (spell.IsAnimationDone())
                    {
                        ReturnPooledObject(spell);
                    }
                }
            }

            //player
            localPlayerController.RefillAirOptions();
            otherPlayerController.RefillAirOptions();

            localPlayerController.ApplyResistances();
            otherPlayerController.ApplyResistances();

            localPlayerController.RunFrameBehaviour();
            otherPlayerController.RunFrameBehaviour();

            localPlayerController.AddMeter(0.0005f);
            otherPlayerController.AddMeter(0.0005f);

            Physics2D.Simulate(Time.fixedDeltaTime);

            localPlayerController.CheckIfGrounded();
            otherPlayerController.CheckIfGrounded();

            ++localPlayerController.currentFrameNum;
            ++otherPlayerController.currentFrameNum;

            ++currentFrame;

            if (localPlayerController.IsIdle() || otherPlayerController.IsIdle())
            {
                if (!gameOver)
                {
                    OnPauseGame();
                }
            }
        }
    }

    private void OnPauseGame()
    {
        Debug.Log("Game paused");

        //pause game
        gamePaused = true;

        Hashtable roomProps = new Hashtable() { { GAME_PAUSED, true } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);

        //check can move

        bool localCanMove, otherCanMove = false;

        if (localPlayerController.IsIdle() || (localPlayerController.IsStunned() && localPlayerController.CanBurst()))
        {
            localCanMove = true;
        }
        else
        {
            localCanMove = false;
        }
        if (otherPlayerController.IsIdle() || (otherPlayerController.IsStunned() && otherPlayerController.CanBurst()))
        {
            otherCanMove = true;
        }
        else
        {
            otherCanMove = false;
        }

        if (localCanMove || (localPlayerController.IsInterruptable() && otherCanMove)) //move can be interrupted during opponents turn
        {
            localPlayerController.allowMove = true;
        }
        else
        {
            localPlayerController.allowMove = false;
        }

        if (otherCanMove || (otherPlayerController.IsInterruptable() && localCanMove)) //move can be interrupted during opponents turn
        {
            otherPlayerController.allowMove = true;
        }
        else
        {
            otherPlayerController.allowMove = false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsLocal)
            {
                if (localPlayerController.allowMove)
                {
                    bool playerIsGrounded = localPlayerController.CheckIfGrounded();

                    Hashtable playerPauseProps = new Hashtable() { { ShooterGameInfo.PLAYER_SELECTED_ACTION, (int)PlayerController.PlayerActions.NONE },
                                                                   { ShooterGameInfo.PLAYER_SHOW_CONTROLS, true },
                                                                   { ShooterGameInfo.PLAYER_GROUNDED, playerIsGrounded },
                                                                   { ShooterGameInfo.PLAYER_CURRENT_ACTION, (int)localPlayerController.playerCurrentAction } };

                    p.SetCustomProperties(playerPauseProps);
                }
            }
            else
            {
                if (otherPlayerController.allowMove)
                {
                    bool playerIsGrounded = otherPlayerController.CheckIfGrounded();

                    Hashtable playerPauseProps = new Hashtable() { { ShooterGameInfo.PLAYER_SELECTED_ACTION, (int)PlayerController.PlayerActions.NONE },
                                                                   { ShooterGameInfo.PLAYER_SHOW_CONTROLS, true },
                                                                   { ShooterGameInfo.PLAYER_GROUNDED, playerIsGrounded },
                                                                   { ShooterGameInfo.PLAYER_CURRENT_ACTION, (int)otherPlayerController.playerCurrentAction } };

                    p.SetCustomProperties(playerPauseProps);
                }
            }
        }
    }

    //Object pooling

    public GameObject GetPooledSpell(string spellName) //spell name should match name of prefab
    {
        //find target spell from pool
        foreach (SpellFrameBehaviour spell in spellsPool)
        {
            if (!spell.activeSpell && spell.spellName == spellName)
            {
                ActivatePooledObject(spell);

                return spell.gameObject;
            }
        }

        GameObject returnSpellObj = null;
        SpellFrameBehaviour returnSpell = null;

        for (int i = 0; i < amtToPool; ++i)
        {
            GameObject newSpellObj = PhotonNetwork.InstantiateRoomObject(spellName, spellsSpawnPoint.position, Quaternion.identity);
            newSpellObj.transform.SetParent(spellsPoolTransform);

            SpellFrameBehaviour newSpell = newSpellObj.GetComponent<SpellFrameBehaviour>();
            spellsPool.Add(newSpell);

            ReturnPooledObject(newSpell);

            if (i == 0)
            {
                returnSpellObj = newSpellObj;
                returnSpell = newSpell;
            }
        }

        ActivatePooledObject(returnSpell);

        return returnSpellObj;
    }

    public void ReturnPooledObject(SpellFrameBehaviour spell)
    {
        spell.activeSpell = false;
        spell.enabledBehaviour = false;
        spell.lastFrame = true;
        spell.frameNum = -1;
        spell.phase = 0;

        spell.transform.position = spellsSpawnPoint.position;
    }

    void ActivatePooledObject(SpellFrameBehaviour spell)
    {
        spell.activeSpell = true;
        spell.enabledBehaviour = true;
        spell.lastFrame = false;
        spell.frameNum = -1;
    }
}
