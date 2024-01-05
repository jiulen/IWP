using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerWhirlwind : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] Transform whirlwindSpawnPoint;

    [SerializeField] string grabAnim, throwAnim;

    SpellWhirlwind spellWhirlwind;

    //Phases
    //0 is grab, 1 is throw

    int phase = 0;
    bool startPhase_1 = false;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            switch (frameNum)
            {
                case 0:
                    currentAnimName = grabAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 15: //create whirlwind + end
                    GameObject whirlwindObj = ShooterGameManager.Instance.GetPooledSpell("Whirlwind");

                    spellWhirlwind = whirlwindObj.GetComponent<SpellWhirlwind>();
                    spellWhirlwind.spawnPos = whirlwindSpawnPoint.position;
                    if (goLeft)
                    {
                        spellWhirlwind.knockbackDirection = Vector2.left;
                        spellWhirlwind.grabKnockbackDirection = Vector2.right;
                        whirlwindObj.transform.localScale = new Vector3(-1, 1, 1);
                    }
                    else
                    {
                        spellWhirlwind.knockbackDirection = Vector2.right;
                        spellWhirlwind.grabKnockbackDirection = Vector2.left;
                        whirlwindObj.transform.localScale = new Vector3(1, 1, 1);
                    }
                    spellWhirlwind.ownerNum = playerController.playerNum;
                    spellWhirlwind.owner = playerController;
                    spellWhirlwind.whirlwindTransform = whirlwindSpawnPoint;

                    spellWhirlwind.phase = 0;

                    whirlwindObj.transform.rotation = Quaternion.identity;

                    //run frame 0 of explosion
                    ++spellWhirlwind.frameNum;
                    spellWhirlwind.GoToFrame();

                    break;
                case 30: //switch to throw if hit grab
                    if (spellWhirlwind.hitPlayer)
                    {
                        startPhase_1 = true;
                        phase = 1;
                    }
                    break;
                case 38: //end after miss grab
                    Debug.Log("huh");
                    EndAnimation();
                    break;
            }
        }
        else if (phase == 1)
        {
            Debug.Log("phase 1");

            if (startPhase_1)
            {
                frameNum = 0;

                startPhase_1 = false;
            }

            switch (frameNum)
            {
                case 0:
                    currentAnimName = throwAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 11: //end
                    Debug.Log("end");
                    EndAnimation();
                    break;
            }
        }

        AnimatorSetFrame();
    }

    public override void EndAnimation()
    {
        base.EndAnimation();

        phase = 0;
    }
}
