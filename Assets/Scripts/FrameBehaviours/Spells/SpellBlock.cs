using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBlock : SpellFrameBehaviour
{
    //Phases
    //0 is blocking, 1 is hit

    public bool startPhase_1;

    [SerializeField] string blockingAnim, shrinkAnim;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            switch (frameNum)
            {
                case 0:
                    currentAnimName = blockingAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 59: //end blocking
                    phase = 1;
                    break;
            }
        }
        else if (phase == 1)
        {
            if (startPhase_1)
            {
                frameNum = 0;

                startPhase_1 = false;
            }

            switch (frameNum)
            {
                case 0:
                    currentAnimName = shrinkAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;

                case 11: //end
                    EndAnimation();
                    break;
            }
        }

        AnimatorSetFrame();
    }

    protected override void OnTriggerEnter2D(Collider2D collision) //empty func since no checks
    {
    }

    public override void EndAnimation()
    {
        base.EndAnimation();

        activeSpell = false;
    }
}
