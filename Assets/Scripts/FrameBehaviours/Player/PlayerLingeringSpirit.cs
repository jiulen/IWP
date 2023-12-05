using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLingeringSpirit : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] Transform spiritSpawnPoint;

    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //create lingering spirit
                GameObject spiritObj = ShooterGameManager.Instance.GetPooledSpell("LingeringSpirit");

                SpellLingeringSpirit spellLingeringSpirit = spiritObj.GetComponent<SpellLingeringSpirit>();
                spellLingeringSpirit.spawnPos = spiritSpawnPoint.position;
                if (goLeft)
                {
                    spellLingeringSpirit.targetDir = Vector2.left;
                }
                else
                {
                    spellLingeringSpirit.targetDir = Vector2.right;
                }
                spellLingeringSpirit.ownerNum = playerController.playerNum;
                spellLingeringSpirit.target = playerController.oppTransform;
                spellLingeringSpirit.startSpell = true;

                spiritObj.transform.rotation = Quaternion.identity;

                if (goLeft)
                {
                    spiritObj.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    spiritObj.transform.localScale = new Vector3(1, 1, 1);
                }
                break;
            case 23: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
