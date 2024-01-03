using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSeismicStrike : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] float ySpawnPos;
    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //create lightning + end
                GameObject seismicStrikeObj = ShooterGameManager.Instance.GetPooledSpell("SeismicStrike");

                SpellSeismicStrike spellSeismicStrike = seismicStrikeObj.GetComponent<SpellSeismicStrike>();
                spellSeismicStrike.spawnPos = new Vector3(playerController.oppTransform.position.x, ySpawnPos, 0);
                if (goLeft)
                {
                    spellSeismicStrike.knockbackDirection = new Vector2(-0.25f, 1);
                    spellSeismicStrike.knockbackDirection.Normalize();
                }
                else
                {
                    spellSeismicStrike.knockbackDirection = new Vector2(0.25f, 1);
                    spellSeismicStrike.knockbackDirection.Normalize();
                }
                spellSeismicStrike.ownerNum = playerController.playerNum;
                spellSeismicStrike.owner = playerController;

                seismicStrikeObj.transform.rotation = Quaternion.identity;

                if (goLeft)
                {
                    seismicStrikeObj.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    seismicStrikeObj.transform.localScale = new Vector3(1, 1, 1);
                }

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}