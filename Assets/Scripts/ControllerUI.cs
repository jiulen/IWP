using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class ControllerUI : MonoBehaviour
{
    PlayerController.PlayerActions selectedAction;

    [SerializeField] ControllerAction[] controllerActions;
    Dictionary<PlayerController.PlayerActions, GameObject> controllerObjects = new();

    [SerializeField] Toggle flipToggle;

    private void Awake()
    {
        foreach (ControllerAction controllerAction in controllerActions)
        {
            controllerObjects.Add(controllerAction.toggleAction, controllerAction.gameObject);
        }
    }

    public void SetAction(PlayerController.PlayerActions toggleAction)
    {
        selectedAction = toggleAction;
    }

    public void LockInAction()
    {
        Hashtable playerActionProps = new Hashtable() { { ShooterGameInfo.PLAYER_SELECTED_ACTION, (int)selectedAction },
                                                        { ShooterGameInfo.PLAYER_FLIP, flipToggle.isOn } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerActionProps);

        gameObject.SetActive(false);
    }

    public void SetUI(List<PlayerController.PlayerActions> unavailableActions, bool forceBurst)
    {
        flipToggle.isOn = false;

        if (forceBurst)
        {
            foreach (KeyValuePair<PlayerController.PlayerActions, GameObject> keyValuePair in controllerObjects)
            {
                if (keyValuePair.Key == PlayerController.PlayerActions.BURST)
                {
                    keyValuePair.Value.SetActive(true);
                }
                else
                {
                    keyValuePair.Value.SetActive(false);
                }
            }
        }
        else
        {
            foreach (KeyValuePair<PlayerController.PlayerActions, GameObject> keyValuePair in controllerObjects)
            {
                if (unavailableActions.Contains(keyValuePair.Key))
                {
                    keyValuePair.Value.SetActive(false);
                }
                else
                {
                    keyValuePair.Value.SetActive(true);
                }
            }
        }
    }
}
