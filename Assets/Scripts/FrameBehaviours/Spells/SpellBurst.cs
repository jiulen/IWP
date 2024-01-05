using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellBurst : SpellFrameBehaviour
{
    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                transform.position = spawnPos;
                break;
            case 5: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
