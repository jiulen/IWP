using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ReplayListEntry : MonoBehaviour
{
    public TMP_Text replayNameText;
    public Button viewReplayButton;

    private string replayFilePath;

    ShooterLobbyMainPanel mainPanel;

    public void Start()
    {
        viewReplayButton.onClick.AddListener(() =>
        {
            bool replaySuccess = ReplayManager.Instance.SetCurrentReplay(replayFilePath);
            mainPanel.OnReplaySelected(replaySuccess);
        });
    }

    public void Initialize(string name, string filePath, ShooterLobbyMainPanel _mainPanel)
    {
        replayNameText.text = name;
        replayFilePath = filePath;
        mainPanel = _mainPanel;
    }
}