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

    bool gameStarting = false;

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
        Hashtable props = new Hashtable
            {
                {ShooterGameInfo.PLAYER_LOADED_LEVEL, true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    private IEnumerator EndOfGame(string winner, bool draw = false)
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

        PhotonNetwork.LeaveRoom();
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
        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_LIVES))
        {
            CheckEndOfGame();
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (gameStarting)
        {
            return;
        }

        if (changedProps.ContainsKey(ShooterGameInfo.PLAYER_LOADED_LEVEL))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                //Rpc
                StartGame();
            }
            else
            {
                // not all players loaded yet. wait:
                Debug.Log("setting text waiting for players! ", this.InfoText);
                InfoText.text = "Waiting for other player...";
            }
        }
    }

    private void StartGame()
    {
        Debug.Log("StartGame!");

        // on rejoin, we have to figure out if the spaceship exists or not
        // if this is a rejoin (the ship is already network instantiated and will be setup via event) we don't need to call PN.Instantiate

        float xPos;
        if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == 0)
        {
            xPos = -4;
        }
        else
        {
            xPos = 4;
        }
        Vector3 position = new Vector3(xPos, -2.125f, 0);

        // Spawn player
        GameObject playerObj = PhotonNetwork.Instantiate("Player", position, Quaternion.identity, 0); // avoid this call on rejoin (ship was network instantiated before)
        if (PhotonNetwork.LocalPlayer.GetPlayerNumber() == 1)
        {
            playerObj.transform.localScale = new Vector3(-1, 1, 1);
        }

        if (PhotonNetwork.IsMasterClient)
        {
        }
    }

    private bool CheckAllPlayerLoadedLevel()
    {
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
        string winner = "";
        bool draw = true;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object lives;
            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_LIVES, out lives))
            {
                if ((int)lives <= 0)
                {
                    playerDead = true;
                }
                else
                {
                    winner = p.NickName;
                    draw = false;
                }
            }
        }

        if (playerDead)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                StopAllCoroutines();
            }

            StartCoroutine(EndOfGame(winner, draw));
        }
    }

    private void FixedUpdate()
    {
        Physics2D.Simulate(Time.fixedDeltaTime);
        currentFrame++;
    }
}
