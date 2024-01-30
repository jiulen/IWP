using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelBounds : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ShooterGameManager.Instance.isReplay)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

                if (!playerController.playerCollider.enabled)
                    return;

                Hashtable playerDieProps = new Hashtable() { { ShooterGameInfo.PLAYER_DEAD, true } };

                playerController.photonPlayer.SetCustomProperties(playerDieProps);
            }
        }
        else
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

                playerController.isDead = true;
            }
        }
    }
}
