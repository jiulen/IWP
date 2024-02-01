using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelBounds : MonoBehaviour
{
    [SerializeField] GameObject boundsExplosionObj;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ShooterGameManager.Instance.isReplay)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Instantiate(boundsExplosionObj, collision.transform.position, Quaternion.identity);

                if (!PhotonNetwork.IsMasterClient)
                    return;

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
                Instantiate(boundsExplosionObj, collision.transform.position, Quaternion.identity);

                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

                playerController.isDead = true;
            }
        }
    }
}
