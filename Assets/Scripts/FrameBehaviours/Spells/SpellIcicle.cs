using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellIcicle : SpellFrameBehaviour
{
    [SerializeField] string startAnim, repeatableAnim, hitAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = startAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 17: //end
                EndAnimation();

                rb.velocity = Vector2.zero;
                break;
        }

        AnimatorSetFrame();
    }
}
