using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleOut : SpellFrameBehaviour
{
    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                AnimatorChangeAnimation("particleAnim");
                transform.position = spawnPos;
                break;
            case 29: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
