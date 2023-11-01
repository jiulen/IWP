using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class ConnectingPanel : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text ConnectionStatusText;
    public GameObject dotHolder;
    public GameObject connectingPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(PhotonNetwork.NetworkClientState)
        {
            case Photon.Realtime.ClientState.PeerCreated:
                dotHolder.SetActive(false);
                ConnectionStatusText.text = "Connected";
                connectingPanel.SetActive(false);
                break;
            case Photon.Realtime.ClientState.Authenticating:
                dotHolder.SetActive(true);
                ConnectionStatusText.text = "Connecting";
                break;
            case Photon.Realtime.ClientState.Disconnecting:
            case Photon.Realtime.ClientState.Disconnected:
                dotHolder.SetActive(false);
                ConnectionStatusText.text = "Disconnected";
                break;
        }

        Debug.Log("" + PhotonNetwork.NetworkClientState);
    }
}
