using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerIcicle : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] Transform icicleSpawnPoint, iciclePos1, iciclePos2;

    [SerializeField] string attackAnim;

    SpellIcicle spellIcicle;

    public bool icicleEnded = false;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);

                GameObject icicleObj = ShooterGameManager.Instance.GetPooledSpell("Icicle");

                spellIcicle = icicleObj.GetComponent<SpellIcicle>();
                spellIcicle.spawnPos = icicleSpawnPoint.position;
                if (goLeft)
                {
                    spellIcicle.knockbackDirection = Vector2.left;
                    icicleObj.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    spellIcicle.knockbackDirection = Vector2.right;
                    icicleObj.transform.localScale = new Vector3(1, 1, 1);
                }
                spellIcicle.ownerNum = playerController.playerNum;
                spellIcicle.owner = playerController;
                spellIcicle.playerIcicle = this;

                icicleObj.transform.rotation = Quaternion.identity;

                spellIcicle.position0 = icicleSpawnPoint;
                spellIcicle.position1 = iciclePos1;
                spellIcicle.position2 = iciclePos2;

                //run frame 0 of icicle
                ++spellIcicle.frameNum;
                spellIcicle.GoToFrame();

                icicleEnded = false;

                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;

                break;
            case 20: //resume falling, icicle gone
                rb.gravityScale = 1;
                break;
            case 32: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    public override void EndAnimation() //dont disable behaviour here, do in player controller
    {
        base.EndAnimation();

        if (!icicleEnded)
        {
            ShooterGameManager.Instance.ReturnPooledObject(spellIcicle);
        }

        rb.gravityScale = 1;
    }
}
