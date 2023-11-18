using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellIcicle : SpellFrameBehaviour
{
    //Phases
    //0 is start, 1 is repeatable, 2 is hit

    [SerializeField] string startAnim, repeatableAnim, hitAnim;

    public override void GoToFrame()
    {
        if (phase == 0)
        {
            switch (frameNum)
            {
                case 0:
                    gameObject.transform.position = spawnPos;

                    currentAnimName = startAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 17: //end start
                    break;
            }
        }
        else if (phase == 1)
        {
            switch (frameNum)
            {
                case 0:
                    currentAnimName = repeatableAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 17: //end
                    EndAnimation();

                    rb.velocity = Vector2.zero;
                    break;
            }
        }
        else if (phase == 2)
        {
            switch (frameNum)
            {
                case 0:
                    currentAnimName = hitAnim;
                    AnimatorChangeAnimation(currentAnimName);
                    break;
                case 17: //end
                    EndAnimation();

                    rb.velocity = Vector2.zero;
                    break;
            }
        }

        AnimatorSetFrame();
    }
}
