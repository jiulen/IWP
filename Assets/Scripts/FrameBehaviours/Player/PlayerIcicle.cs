using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerIcicle : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] Transform icicleSpawnPoint;

    [SerializeField] string attackAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //create icicle + end
                GameObject icicleObj = ShooterGameManager.Instance.GetPooledSpell("Icicle");

                SpellIcicle spellIcicle = icicleObj.GetComponent<SpellIcicle>();
                spellIcicle.spawnPos = icicleSpawnPoint.position;
                if (goLeft)
                {
                    spellIcicle.targetDir = Vector2.left;
                }
                else
                {
                    spellIcicle.targetDir = Vector2.right;
                }

                icicleObj.transform.rotation = Quaternion.identity;

                if (goLeft)
                {
                    icicleObj.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    icicleObj.transform.localScale = new Vector3(1, 1, 1);
                }

                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
