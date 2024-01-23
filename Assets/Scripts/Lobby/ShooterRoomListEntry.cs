using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ShooterRoomListEntry : MonoBehaviour
{
    public TMP_Text RoomNameText;
    public TMP_Text RoomPlayersText;
    public Button JoinRoomButton;

    private string roomName;

    public void Start()
    {
        JoinRoomButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinRoom(roomName);
        });
    }

    public void Initialize(string name, int currentPlayers, int maxPlayers)
    {
        roomName = name;

        RoomNameText.text = name;
        //RoomPlayersText.text = currentPlayers + " / " + maxPlayers; //hardcode to 1/2 everytime for now
    }
}