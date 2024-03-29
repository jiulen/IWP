using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class ShooterGameManager : MonoBehaviourPunCallbacks
{
    public static ShooterGameManager Instance = null;

    public TMP_Text InfoText;

    [SerializeField] PlayerController player1, player2;
    PlayerController localPlayerController, otherPlayerController;

    [SerializeField] ControllerUI localControllerUI;

    [SerializeField] List<SpellFrameBehaviour> spellsPool = new();
    [SerializeField] Transform spellsPoolTransform;
    public Transform spellsSpawnPoint;
    const int amtToPool = 5;

    public bool gameStarted = false;
    public bool gamePaused = true;
    public bool gameOver = false;
    public int currentFrame;

    //Room properties
    const string GAME_STARTED = "GameStarted";
    const string GAME_PAUSED = "GamePaused";
    //replay
    const string REPLAY_FILE = "ReplayFile";
    const string REPLAY_NAME = "ReplayName";

    //Game over stuff
    [SerializeField] Animator gameOverSlide, gameOverFade;
    [SerializeField] TMP_Text winnerText, replaySaveProg;

    public bool isReplay = false;

    //Replay stuff
    [SerializeField] Slider replaySlider;
    [SerializeField] TMP_Text replayFrame;
    [SerializeField] Toggle playPauseToggle;

    bool oldGamePaused = false;

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
        if (!isReplay)
        {
            //Notify ready
            Hashtable props = new()
            {
                { ShooterGameInfo.PLAYER_LOADED_LEVEL, true },
                { ShooterGameInfo.PLAYER_DEAD, false }
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
        else
        {
            localPlayerController = player1;
            otherPlayerController = player2;

            gamePaused = false;
            replaySlider.maxValue = ReplayManager.Instance.replay.lastFrame;

            //Advance frames after resetting scene
            if (ReplayManager.Instance.replayTurn >= 0)
            {
                replaySlider.value = ReplayManager.Instance.replayTurn;
                gamePaused = ReplayManager.Instance.replayPaused;

                int turnsToAdvance = ReplayManager.Instance.replayTurn - currentFrame;
                for (int i = 0; i < turnsToAdvance; ++i)
                {
                    NextReplayTurn();
                }
            }

            playPauseToggle.isOn = !gamePaused;
        }
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
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LobbyScene");
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
        if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_DEAD, out object playerDied))
        {
            if ((bool)playerDied)
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

                //start recording replay
                ReplayManager.Instance.StartRecording(localPlayerController.playerName, localPlayerController.playerSkinID, 
                                                      otherPlayerController.playerName, otherPlayerController.playerSkinID);
            }
        }

        if (gamePaused)
        {
            //check when to continue
            if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_SELECTED_ACTION))
            {
                PlayerController currentController;
                bool stopPause = true;

                //assign selected action
                PlayerController assigningController;
                int assigningPlayerNum = 0;

                if (targetPlayer.IsLocal)
                {
                    assigningController = localPlayerController;
                    assigningPlayerNum = 1;
                }
                else
                {
                    assigningController = otherPlayerController;
                    assigningPlayerNum = 2;
                }

                changedProps.TryGetValue(ShooterGameInfo.PLAYER_SELECTED_ACTION, out object assigningAction);
                PlayerController.PlayerActions assigningAction2 = (PlayerController.PlayerActions)(int)assigningAction;

                if (assigningAction2 != PlayerController.PlayerActions.NONE)
                {
                    if (assigningAction2 != PlayerController.PlayerActions.CONTINUE_BLOCK && assigningAction2 != PlayerController.PlayerActions.SKIP)
                    {
                        assigningController.playerCurrentAction = assigningAction2;
                    }

                    bool assigningPlayerFlip = false;
                    if (targetPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_FLIP, out object playerFlip))
                    {
                        assigningPlayerFlip = (bool)playerFlip;
                    }

                    //add replay turn
                    ReplayManager.Instance.replay.AddTurn(currentFrame, (int)assigningAction2, assigningPlayerFlip, assigningPlayerNum);
                }

                //check selected move
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
                            if (selectedAction != PlayerController.PlayerActions.CONTINUE_BLOCK && selectedAction != PlayerController.PlayerActions.SKIP)
                            {
                                currentController.currentFrameNum = 0;
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
        //Sync replay file for client only
        if (propertiesThatChanged.TryGetValue(REPLAY_FILE, out object replayFile))
        {
            //receive replay

            string replayName;

            if (propertiesThatChanged.TryGetValue(REPLAY_NAME, out object replayNameObj))
            {
                replayName = (string)replayNameObj;
            }
            else
            {
                DateTime timeNow = DateTime.Now;
                replayName = timeNow.Year + "-" + timeNow.Month + "-" + timeNow.Day + "-" +
                             timeNow.Hour + "-" + timeNow.Minute + "-" + timeNow.Second;
            }

            ReplayManager.Instance.AddReplay(replayName, (string)replayFile);
            replaySaveProg.text = "Replay saved!";
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

    private void CheckEndOfGame()
    {
        bool playerDead = false;
        string winner = ""; //if draw then winner will be ""

        if (!isReplay)
        {
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
        }
        else
        {
            if (!localPlayerController.isDead)
            {
                winner = localPlayerController.playerName;
            }
            else
            {
                playerDead = true;
            }
            if (!otherPlayerController.isDead)
            {
                winner = otherPlayerController.playerName;
            }
            else
            {
                playerDead = true;
            }
        }

        if (playerDead)
        {
            if (!isReplay)
            {
                localPlayerController.ShowControls(false);
            }

            gameOver = true;

            if (winner != "")
            {
                winnerText.text = winner + " WINS";
            }
            else
            {
                winnerText.text = "DRAW?";
            }

            if (PhotonNetwork.IsMasterClient && !isReplay)
            {
                if (winner == ReplayManager.Instance.replay.p1.playerName)
                    ReplayManager.Instance.replay.winner = 1;
                else if (winner == ReplayManager.Instance.replay.p2.playerName)
                    ReplayManager.Instance.replay.winner = 2;
                else if (winner == "")
                    ReplayManager.Instance.replay.winner = 0; //draw
                else
                    Debug.Log("HUH? wrong winner name : " + winner);

                ReplayManager.Instance.replay.lastFrame = currentFrame;
                ReplayManager.Instance.replay.WrapTurns();

                //save replay

                string replayName = localPlayerController.playerName + "_v_" + otherPlayerController.playerName;

                string replayContents = ReplayManager.Instance.ReplayToJson();

                ReplayManager.Instance.AddReplay(replayName, replayContents);

                replaySaveProg.text = "Replay saved!";

                //send replay
                Hashtable roomProps = new Hashtable() { { REPLAY_FILE, replayContents }, { REPLAY_NAME, replayName} };
                PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
            }

            StartCoroutine(EndOfGame());
        }
    }

    //Game parts

    private void FixedUpdate()
    {
        if (!isReplay)
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

                localPlayerController.AddMeter(1f / 360);
                otherPlayerController.AddMeter(1f / 360);

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
        else
        {
            localPlayerController.UpdateInfoUIAuto();
            otherPlayerController.UpdateInfoUIAuto();

            if (!gamePaused)
            {
                NextReplayTurn();
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

        bool localIdle, otherIdle;
        localIdle = localPlayerController.IsIdle();
        otherIdle = otherPlayerController.IsIdle();

        if (localIdle || 
            (localPlayerController.IsInterruptable() && otherIdle) || //move can be interrupted during opponents turn
            (localPlayerController.IsStunned() && localPlayerController.CanBurst() && otherIdle)) //can burst while stunned during opponennts turn
        {
            localPlayerController.allowMove = true;
        }
        else
        {
            localPlayerController.allowMove = false;
        }

        if (otherIdle || 
            (otherPlayerController.IsInterruptable() && localIdle) ||
            (otherPlayerController.IsStunned() && otherPlayerController.CanBurst() && localIdle)) //move can be interrupted during opponents turn
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
            GameObject newSpellObj;

            if (!isReplay)
            {
                newSpellObj = PhotonNetwork.InstantiateRoomObject(spellName, spellsSpawnPoint.position, Quaternion.identity);
            }
            else
            {
                newSpellObj = Instantiate(Resources.Load<GameObject>(spellName), spellsSpawnPoint.position, Quaternion.identity);
            }
            
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

    public void InstantiateParticleEffect(string effectName, Vector3 effectPos)
    {
        if (!isReplay)
            PhotonNetwork.InstantiateRoomObject(effectName, effectPos, Quaternion.identity);
        else
            Instantiate(Resources.Load<GameObject>(effectName), effectPos, Quaternion.identity);
    }

    //Replay stuff

    public void NextReplayTurn()
    {
        if (currentFrame == ReplayManager.Instance.replay.lastFrame)
            ReplayEndOfGame();

        CheckInput();

        //normal game logic

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

        localPlayerController.AddMeter(1f / 360);
        otherPlayerController.AddMeter(1f / 360);

        Physics2D.Simulate(Time.fixedDeltaTime);

        localPlayerController.CheckIfGrounded();
        otherPlayerController.CheckIfGrounded();

        ++localPlayerController.currentFrameNum;
        ++otherPlayerController.currentFrameNum;

        ++currentFrame;

        //check end
        if (currentFrame > ReplayManager.Instance.replay.lastFrame)
            return;

        replaySlider.value = currentFrame;
        replayFrame.text = replaySlider.value.ToString();
    }

    public void CheckInput()
    {
        var turnList = ReplayManager.Instance.replay.turns;
        var inputsThisTurn = turnList.Where(turn => turn.frameNum == currentFrame);

        foreach (var input in inputsThisTurn)
        {
            PlayerController inputtingController = null;

            switch (input.playerNum)
            {
                case 1:
                    inputtingController = localPlayerController;
                    break;
                case 2:
                    inputtingController = otherPlayerController;
                    break;
            }

            if (inputtingController != null)
            {
                PlayerController.PlayerActions inputtingAction = (PlayerController.PlayerActions)input.playerAction;

                if (inputtingAction != PlayerController.PlayerActions.CONTINUE_BLOCK && inputtingAction != PlayerController.PlayerActions.SKIP)
                {
                    inputtingController.playerCurrentAction = inputtingAction;
                    inputtingController.currentFrameNum = 0;
                }

                inputtingController.toFlip = input.playerFlip;
            }
            else
            {
                Debug.Log("Null inputting controller");
            }
        }
    }

    public void PlayReplay(bool play)
    {
        gamePaused = !play;
    }

    public void ClickDownReplaySlider()
    {
        DragReplaySlider();

        oldGamePaused = gamePaused;
        gamePaused = true;
    }

    public void DragReplaySlider()
    {
        replayFrame.text = replaySlider.value.ToString();
    }

    public void SkipToTurn()
    {
        DragReplaySlider();
        gamePaused = oldGamePaused;

        //check if before turn, current turn, or after turn
        int newTurnNum = (int)replaySlider.value;
        replayFrame.text = replaySlider.value.ToString();
        if (newTurnNum < currentFrame)
        {
            //Less than current frame, need reset scene before advancing to target frame
            ReplayManager.Instance.replayTurn = newTurnNum;
            ReplayManager.Instance.replayPaused = gamePaused;

            gamePaused = true;

            ResetReplay();
        }
        else
        {
            //More than current frame, advance to target frame
            int turnsToAdvance = newTurnNum - currentFrame;
            for (int i = 0; i < turnsToAdvance; ++i)
            {
                NextReplayTurn();
            }
        }
    }

    public void ReplayEndOfGame()
    {
        replaySaveProg.text = "";

        switch (ReplayManager.Instance.replay.winner)
        {
            case 0: //draw
                winnerText.text = "DRAW?";
                break;
            case 1: //host wins
                winnerText.text = ReplayManager.Instance.replay.p1.playerName + " WINS";
                break;
            case 2: //client wins
                winnerText.text = ReplayManager.Instance.replay.p2.playerName + " WINS";
                break;
            default: //undefined winner
                winnerText.text = "UNDEFINED WINNER";
                break;
        }

        StartCoroutine(EndOfGame());
    }

    public void ResetReplay()
    {
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        if (!isReplay)
            PhotonNetwork.LeaveRoom(false);
        else
            PhotonNetwork.LoadLevel("LobbyScene");
    }
}
