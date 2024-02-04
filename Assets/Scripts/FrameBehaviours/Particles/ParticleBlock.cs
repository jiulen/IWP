using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleBlock : SpellFrameBehaviour
{
    public Transform followTransform;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                AnimatorChangeAnimation("particleAnim");
                transform.position = spawnPos;
                break;
            case 11: //end
                EndAnimation();
                break;
        }

        if (followTransform != null)
        {
            transform.position = followTransform.position;
        }

        AnimatorSetFrame();
    }
}
