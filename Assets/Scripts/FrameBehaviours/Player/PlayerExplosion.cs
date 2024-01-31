using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerExplosion : PlayerFrameBehaviour
{
    public bool goLeft = false;

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
            case 17: //create explosion
                GameObject explosionObj = ShooterGameManager.Instance.GetPooledSpell("Explosion");

                SpellExplosion spellExplosion = explosionObj.GetComponent<SpellExplosion>();
                spellExplosion.spawnPos = explosionSpawnPoint.position;

                if (goLeft)
                {
                    spellExplosion.knockbackDirection = Vector2.left;
                    spellExplosion.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    spellExplosion.knockbackDirection = Vector2.right;
                    spellExplosion.transform.localScale = new Vector3(1, 1, 1);
                }

                spellExplosion.ownerNum = playerController.playerNum;
                spellExplosion.owner = playerController;

                explosionObj.transform.rotation = Quaternion.identity;

                //run frame 0 of explosion
                ++spellExplosion.frameNum;
                spellExplosion.GoToFrame();

                break;
            case 36: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
