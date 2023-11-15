using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class ShooterLobbyMainPanel : MonoBehaviourPunCallbacks
{
    [Header("Login Panel")]
    public GameObject LoginPanel;

    public Button playButton;
    public GameObject playButtonDim;
    public TMP_InputField PlayerNameInput;

    [Header("Selection Panel")]
    public GameObject SelectionPanel;
    public TMP_Text errorMsg;
    public TMP_InputField roomNameInputField;
    public Button joinRoomButton;
    public GameObject joinRoomDim;

    [Header("Room List Panel")]
    public GameObject RoomListPanel;

    public GameObject RoomListContent;
    public GameObject RoomListEntryPrefab;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;
    public Transform transformP_1;
    public Transform transformP_2;
    public TMP_Text roomCodeText;
    public Toggle roomPublicToggle;

    public Button StartGameButton;
    public GameObject StartGameDim;
    public GameObject PlayerListEntryPrefab;

    bool roomPublic = false;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;

    private byte maxPlayers = 2;
    const int ROOM_CODE_LENGTH = 4;

    const string ROOM_PUBLIC = "RoomPublic";

    #region UNITY

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PlayerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }
        else
        {
            PlayerNameInput.text = "Guest " + Random.Range(1000, 10000);
        }
    }

    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        SetActivePanel(SelectionPanel.name);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    public override void OnJoinedLobby()
    {
        // whenever this joins a new lobby, clear any previous room lists
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    // note: when a client joins / creates a room, OnLeftLobby does not get called, even if the client was in a lobby before
    public override void OnLeftLobby()
    {
        cachedRoomList.Clear();
        ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
        errorMsg.text = "Error :\n" + message;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(SelectionPanel.name);
        errorMsg.text = "Error :\n" + message;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        int roomNum = Random.Range(0, 65536);
        string roomName = roomNum.ToString("X4");

        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000, IsVisible = false };
        roomPublicToggle.isOn = false;
        roomPublic = false;

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        // joining (or entering) a room invalidates any cached lobby room list (even if LeaveLobby was not called due to just joining a room)
        cachedRoomList.Clear();

        SetActivePanel(InsideRoomPanel.name);

        roomCodeText.text = "Room Code : " + PhotonNetwork.CurrentRoom.Name;

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        int thisPlayerNumber = 1;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p == PhotonNetwork.LocalPlayer)
                continue;

            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
            {
                switch ((int)playerNumber)
                {
                    case 1:
                        thisPlayerNumber = 2;
                        break;
                    case 2:
                        thisPlayerNumber = 1;
                        break;
                    default:
                        Debug.Log("Wrong player number");
                        break;
                }
            }
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab);

            if (p == PhotonNetwork.LocalPlayer)
            {
                switch (thisPlayerNumber)
                {
                    case 1:
                        entry.transform.SetParent(transformP_1);
                        break;
                    case 2:
                        entry.transform.SetParent(transformP_2);
                        entry.GetComponent<ShooterPlayerListEntry>().PlayerImage.transform.localScale = new Vector3(-1, 1, 1);
                        break;
                    default:
                        break;
                }

                entry.GetComponent<ShooterPlayerListEntry>().Initialize(p.ActorNumber, p.NickName, thisPlayerNumber);

                Hashtable playerNumProps = new Hashtable() { { ShooterGameInfo.PLAYER_NUMBER, thisPlayerNumber } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerNumProps);
            }
            else
            {
                if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
                {
                    switch ((int)playerNumber)
                    {
                        case 1:
                            entry.transform.SetParent(transformP_1);
                            break;
                        case 2:
                            entry.transform.SetParent(transformP_2);
                            entry.GetComponent<ShooterPlayerListEntry>().PlayerImage.transform.localScale = new Vector3(-1, 1, 1);
                            break;
                        default:
                            break;
                    }

                    entry.GetComponent<ShooterPlayerListEntry>().Initialize(p.ActorNumber, p.NickName, (int)playerNumber);

                    p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_SKIN, out object playerSkinID);
                    entry.GetComponent<ShooterPlayerListEntry>().SetPlayerSkin((int)playerSkinID);
                }
                else
                {
                    Debug.Log("No player number found for opponent");
                }
            }

            entry.transform.localScale = Vector3.one;
            entry.transform.localPosition = Vector3.zero;

            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_READY, out object isPlayerReady))
            {
                entry.GetComponent<ShooterPlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }

        StartGameButton.interactable = CheckPlayersReady();
        StartGameDim.SetActive(!CheckPlayersReady());

        if (!PhotonNetwork.IsMasterClient) roomPublicToggle.interactable = false;
        else roomPublicToggle.interactable = true;

        Hashtable props = new Hashtable
            {
                {ShooterGameInfo.PLAYER_LOADED_LEVEL, false}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(ROOM_PUBLIC, out object isRoomPublic))
        {
            roomPublic = (bool)isRoomPublic;
            ToggleRoomPublic();
        }
    }

    public override void OnLeftRoom()
    {
        SetActivePanel(SelectionPanel.name);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);

        int newPlayerNumber = 1;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
        {
            switch ((int)playerNumber)
            {
                case 1:
                    newPlayerNumber = 2;

                    entry.transform.SetParent(transformP_2);
                    entry.GetComponent<ShooterPlayerListEntry>().PlayerImage.transform.localScale = new Vector3(-1, 1, 1);
                    break;
                case 2:
                    newPlayerNumber = 1;

                    entry.transform.SetParent(transformP_1);
                    break;
                default:
                    Debug.Log("Wrong player number");
                    break;
            }
        }

        entry.GetComponent<ShooterPlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName, newPlayerNumber);
        entry.transform.localScale = Vector3.one;
        entry.transform.localPosition = Vector3.zero;

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        bool playersReady = CheckPlayersReady();

        StartGameButton.interactable = playersReady;
        StartGameDim.SetActive(!playersReady);

        if (!PhotonNetwork.IsMasterClient) roomPublicToggle.interactable = false;
        else roomPublicToggle.interactable = true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (playerListEntries.ContainsKey(otherPlayer.ActorNumber))
        {
            Destroy(playerListEntries[otherPlayer.ActorNumber]);
            playerListEntries.Remove(otherPlayer.ActorNumber);
        }
        else
        {
            Debug.Log("Actor number not found as key. Did the player quit the whole app?"); //Found out leaving play mode in unity triggers this once, then timeout triggers another
        }

        bool playersReady = CheckPlayersReady();

        StartGameButton.interactable = playersReady;
        StartGameDim.SetActive(!playersReady);

        if (!PhotonNetwork.IsMasterClient) roomPublicToggle.interactable = false;
        else roomPublicToggle.interactable = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            bool playersReady = CheckPlayersReady();

            StartGameButton.interactable = playersReady;
            StartGameDim.SetActive(!playersReady);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_READY, out object isPlayerReady))
            {
                entry.GetComponent<ShooterPlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            if (changedProps.TryGetValue(ShooterGameInfo.PLAYER_SKIN, out object playerSkinID))
            {
                entry.GetComponent<ShooterPlayerListEntry>().SetPlayerSkin((int)playerSkinID);
            }
        }

        bool playersReady = CheckPlayersReady();

        StartGameButton.interactable = playersReady;
        StartGameDim.SetActive(!playersReady);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        //Sync room publlic for client only
        if (propertiesThatChanged.TryGetValue(ROOM_PUBLIC, out object isRoomPublic))
        {
            roomPublic = (bool)isRoomPublic;
            ToggleRoomPublic();
        }
    }

    #endregion

    #region UI CALLBACKS

    public void OnRoomPublicToggled()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        roomPublic = roomPublicToggle.isOn;
        PhotonNetwork.CurrentRoom.IsVisible = roomPublic;

        Hashtable roomProps = new Hashtable() { { ROOM_PUBLIC, roomPublic } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    void ToggleRoomPublic()
    {
        roomPublicToggle.isOn = roomPublic;
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        SetActivePanel(SelectionPanel.name);
    }

    public void OnCreateRoomButtonClicked()
    {
        errorMsg.text = "";

        int roomNum = Random.Range(0, 65536);
        string roomName = roomNum.ToString("X4");

        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers, PlayerTtl = 10000, IsVisible = false };
        roomPublicToggle.isOn = false;
        roomPublic = false;

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public void OnRoomCodeInputChanged()
    {
        string roomName = roomNameInputField.text;
        if (roomName != roomName.ToUpper())
        {
            roomNameInputField.text = roomName.ToUpper();
        }

        if (roomNameInputField.text.Length == ROOM_CODE_LENGTH)
        {
            joinRoomButton.interactable = true;
            joinRoomDim.SetActive(false);
        }
        else
        {
            joinRoomButton.interactable = false;
            joinRoomDim.SetActive(true);
        }
    }

    public void OnJoinRoomButtonClicked()
    {
        errorMsg.text = "";
        PhotonNetwork.JoinRoom(roomNameInputField.text);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom(false);
    }

    public void OnPlayerNameInputFieldChanged()
    {
        if (PlayerNameInput.text != "")
        {
            playButton.interactable = true;
            playButtonDim.SetActive(false);
        }
        else
        {
            playButton.interactable = false;
            playButtonDim.SetActive(true);
        }
    }

    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;

        PhotonNetwork.LocalPlayer.NickName = playerName;
        PhotonNetwork.ConnectUsingSettings();

        PlayerPrefs.SetString("PlayerName", playerName);
    }

    public void OnRoomListButtonClicked()
    {
        errorMsg.text = "";

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        SetActivePanel(RoomListPanel.name);
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("GameScene");
    }

    #endregion

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        if (PhotonNetwork.PlayerList.Length < maxPlayers)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_READY, out object isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.interactable = CheckPlayersReady();
        StartGameDim.SetActive(!CheckPlayersReady());
    }

    private void SetActivePanel(string activePanel)
    {
        LoginPanel.SetActive(activePanel.Equals(LoginPanel.name));
        SelectionPanel.SetActive(activePanel.Equals(SelectionPanel.name));
        RoomListPanel.SetActive(activePanel.Equals(RoomListPanel.name));    // UI should call OnRoomListButtonClicked() to activate this
        InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));

        errorMsg.text = "";
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            // Remove room from cached room list if it got closed, became invisible or was marked as removed
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }
            // Add new room info to cache
            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab);
            entry.transform.SetParent(RoomListContent.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<ShooterRoomListEntry>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }
}