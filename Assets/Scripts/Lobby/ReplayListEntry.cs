using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class ReplayListEntry : MonoBehaviour
{
    public TMP_Text replayNameText;
    public Button viewReplayButton;

    private string replayFilePath;

    public void Start()
    {
        viewReplayButton.onClick.AddListener(() =>
        {

        });
    }

    public void Initialize(string name, string filePath)
    {
        replayNameText.text = name;
        replayFilePath = filePath;
    }
}