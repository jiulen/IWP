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
    [SerializeField] GameObject airOption1, airOption2, airOption3, airOption4;

    public void SetUISkin(int skinID)
    {
        playerImage.sprite = playerUISkins[skinID];
    }

    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void UpdatePlayerInfo(float playerBurstMeterVal, int playerKnockbackMulti, int playerAirOptions)
    {
        burstBgImage.fillAmount = 1 - playerBurstMeterVal;
        burstMeterImage.fillAmount = playerBurstMeterVal;

        knockbackText.text = playerKnockbackMulti + "%";

        airOption1.SetActive(false);
        airOption2.SetActive(false);
        airOption3.SetActive(false);
        airOption4.SetActive(false);

        if (playerAirOptions > 0) airOption1.SetActive(true);
        if (playerAirOptions > 1) airOption2.SetActive(true);
        if (playerAirOptions > 2) airOption3.SetActive(true);
        if (playerAirOptions > 3) airOption4.SetActive(true);
    }
}
