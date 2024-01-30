using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSeismicStrike : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] float ySpawnPos;
    [SerializeField] string attackAnim;

    Vector3 seismicStrikePos;

    [SerializeField] float minX, maxX;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 10:
                float rockXPos = Mathf.Clamp(playerController.oppTransform.position.x, minX, maxX);
                seismicStrikePos = new Vector3(rockXPos, ySpawnPos, 0);
                break;
            case 14: //create seismic strike
                GameObject seismicStrikeObj = ShooterGameManager.Instance.GetPooledSpell("SeismicStrike");

                SpellSeismicStrike spellSeismicStrike = seismicStrikeObj.GetComponent<SpellSeismicStrike>();
                spellSeismicStrike.spawnPos = seismicStrikePos;
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

                //run frame 0 of explosion
                ++spellSeismicStrike.frameNum;
                spellSeismicStrike.GoToFrame();
                break;
            case 32:
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
