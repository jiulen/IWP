using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

using TMPro;

public class ShooterPlayerListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text PlayerNameText;
    public Image PlayerImage;
    public GameObject PlayerReadyOutline;
    public Button PlayerReadyButton;
    public GameObject PlayerReadyText;
    public GameObject colorToggleGrp;
    public Image[] colorToggleImgs;

    [Header ("Player Skins")]

    private int ownerId;
    private bool isPlayerReady;

    #region UNITY

    public void OnEnable()
    {
        PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
    }

    public void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
        {
            PlayerReadyOutline.SetActive(false);
            PlayerReadyButton.gameObject.SetActive(false);
            colorToggleGrp.SetActive(false);
        }
        else
        {
            Hashtable initialProps = new Hashtable() { { ShooterGameInfo.PLAYER_READY, isPlayerReady }, { ShooterGameInfo.PLAYER_LIVES, ShooterGameInfo.PLAYER_MAX_LIVES }};
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
            PhotonNetwork.LocalPlayer.SetScore(0);

            PlayerReadyButton.onClick.AddListener(() =>
            {
                isPlayerReady = !isPlayerReady;
                SetPlayerReady(isPlayerReady);

                Hashtable props = new Hashtable() { { ShooterGameInfo.PLAYER_READY, isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                if (PhotonNetwork.IsMasterClient)
                {
                    FindObjectOfType<ShooterLobbyMainPanel>().LocalPlayerPropertiesUpdated();
                }
            });
        }
    }

    public void OnDisable()
    {
        PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
    }

    #endregion

    public void Initialize(int playerId, string playerName, int playerNum)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;

        for (int i = 0; i < colorToggleImgs.Length; ++i)
        {
            colorToggleImgs[i].color = ShooterGameInfo.GetColor(i + (playerNum - 1) * 4);
        }
    }

    private void OnPlayerNumberingChanged()
    {
    }

    public void SetPlayerReady(bool playerReady)
    {
        PlayerReadyButton.GetComponentInChildren<TMP_Text>().text = playerReady ? "Ready!" : "Ready?";
        PlayerReadyText.SetActive(playerReady);
    }

    public void SetPlayerSkin(int playerSkinID)
    {

    }
}