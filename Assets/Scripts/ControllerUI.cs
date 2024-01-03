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
    Dictionary<PlayerController.PlayerActions, Toggle> controllerObjects = new();

    [SerializeField] Toggle flipToggle;

    bool isInit = false;

    private void Awake()
    {
        if (!isInit) InitControllerUI();
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

    public void SetUI(List<PlayerController.PlayerActions> unavailableActions, bool forceBurst, bool blocking)
    {
        if (!isInit) InitControllerUI();

        flipToggle.isOn = false;

        bool selectNew;

        if (forceBurst)
        {
            selectNew = (selectedAction != PlayerController.PlayerActions.BURST) && (selectedAction != PlayerController.PlayerActions.SKIP);
            bool selectedFirst = false;

            foreach (KeyValuePair<PlayerController.PlayerActions, Toggle> keyValuePair in controllerObjects)
            {
                if (keyValuePair.Key == PlayerController.PlayerActions.BURST || keyValuePair.Key == PlayerController.PlayerActions.SKIP)
                {
                    keyValuePair.Value.gameObject.SetActive(true);
                    if (selectNew && !selectedFirst)
                    {
                        keyValuePair.Value.isOn = true;
                        selectedFirst = true;

                        selectedAction = keyValuePair.Key;
                    }
                }
                else
                {
                    keyValuePair.Value.gameObject.SetActive(false);
                    keyValuePair.Value.isOn = false;
                }
            }
        }
        else if (blocking)
        {
            selectNew = (selectedAction != PlayerController.PlayerActions.CONTINUE_BLOCK) && (selectedAction != PlayerController.PlayerActions.STOP_BLOCK);
            bool selectedFirst = false;

            foreach (KeyValuePair<PlayerController.PlayerActions, Toggle> keyValuePair in controllerObjects)
            {
                if (keyValuePair.Key == PlayerController.PlayerActions.CONTINUE_BLOCK || keyValuePair.Key == PlayerController.PlayerActions.STOP_BLOCK)
                {
                    keyValuePair.Value.gameObject.SetActive(true);
                    if (selectNew && !selectedFirst)
                    {
                        keyValuePair.Value.isOn = true;
                        selectedFirst = true;

                        selectedAction = keyValuePair.Key;
                    }
                }
                else
                {
                    keyValuePair.Value.gameObject.SetActive(false);
                    keyValuePair.Value.isOn = false;
                }
            }
        }
        else
        {
            selectNew = unavailableActions.Contains(selectedAction);
            bool selectedFirst = false;

            foreach (KeyValuePair<PlayerController.PlayerActions, Toggle> keyValuePair in controllerObjects)
            {
                if (unavailableActions.Contains(keyValuePair.Key))
                {
                    keyValuePair.Value.gameObject.SetActive(false);
                    keyValuePair.Value.isOn = false;
                }
                else
                {
                    keyValuePair.Value.gameObject.SetActive(true);

                    if (selectNew && !selectedFirst)
                    {
                        keyValuePair.Value.isOn = true;
                        selectedFirst = true;

                        selectedAction = keyValuePair.Key;
                    }
                }
            }
        }
    }

    void InitControllerUI()
    {
        isInit = true;

        foreach (ControllerAction controllerAction in controllerActions)
        {
            controllerAction.InitToggle();

            controllerObjects.Add(controllerAction.toggleAction, controllerAction.GetComponent<Toggle>());
        }
    }
}
