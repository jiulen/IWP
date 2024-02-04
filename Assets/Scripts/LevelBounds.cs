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
            if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (!PhotonNetwork.IsMasterClient)
                    return;

                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

                if (!playerController.playerCollider.enabled)
                    return;

                GameObject particleObj = ShooterGameManager.Instance.GetPooledSpell("BoundsExplosion");

                SpellFrameBehaviour spellParticle = particleObj.GetComponent<SpellFrameBehaviour>();
                spellParticle.spawnPos = collision.transform.position;

                if (ShooterGameManager.Instance.gameOver)
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

                if (!playerController.playerCollider.enabled)
                    return;

                GameObject particleObj = ShooterGameManager.Instance.GetPooledSpell("BoundsExplosion");

                SpellFrameBehaviour spellParticle = particleObj.GetComponent<SpellFrameBehaviour>();
                spellParticle.spawnPos = collision.transform.position;

                playerController.isDead = true;
            }
        }
    }


}
