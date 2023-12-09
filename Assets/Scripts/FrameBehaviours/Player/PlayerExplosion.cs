using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerExplosion : PlayerFrameBehaviour
{
    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //create explosion + end
                GameObject explosionObj = ShooterGameManager.Instance.GetPooledSpell("Explosion");

                SpellExplosion spellExplosion = explosionObj.GetComponent<SpellExplosion>();
                spellExplosion.spawnPos = playerController.oppTransform.position;
                spellExplosion.ownerNum = playerController.playerNum;
                spellExplosion.owner = playerController;

                explosionObj.transform.rotation = Quaternion.identity;

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
