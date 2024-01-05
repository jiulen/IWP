using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerExplosion : PlayerFrameBehaviour
{
    [SerializeField] string attackAnim;

    [SerializeField] Transform explosionSpawnPoint;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 8: //create explosion
                GameObject explosionObj = ShooterGameManager.Instance.GetPooledSpell("Explosion");

                SpellExplosion spellExplosion = explosionObj.GetComponent<SpellExplosion>();
                spellExplosion.spawnPos = explosionSpawnPoint.position;
                spellExplosion.ownerNum = playerController.playerNum;
                spellExplosion.owner = playerController;
                spellExplosion.playerExplosion = this;

                explosionObj.transform.rotation = Quaternion.identity;

                //run frame 0 of explosion
                ++spellExplosion.frameNum;
                spellExplosion.GoToFrame();

                break;
            case 23: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
