using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfoUI : MonoBehaviour
{
    [SerializeField] PlayerController playerController;

    [SerializeField] Sprite[] playerUISkins;

    [SerializeField] Image playerImage;
    [SerializeField] Image burstBgImage;
    [SerializeField] Image burstMeterImage;
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text knockbackText;
    [SerializeField] GameObject airOption1, airOption2;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        burstBgImage.fillAmount = 1 - playerController.burstMeterValue;
        burstMeterImage.fillAmount = playerController.burstMeterValue;

        knockbackText.text = playerController.knockbackMultiplier + "%";

        if (playerController.airOptionsAvail <= 0)
        {
            airOption1.SetActive(true);
            airOption2.SetActive(true);
        }
        else if (playerController.airOptionsAvail <= 1)
        {
            airOption1.SetActive(true);
            airOption2.SetActive(false);
        }
        else
        {
            airOption1.SetActive(false);
            airOption2.SetActive(false);
        }
    }

    public void SetUISkin(int skinID)
    {
        playerImage.sprite = playerUISkins[skinID];
    }

    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }
}
