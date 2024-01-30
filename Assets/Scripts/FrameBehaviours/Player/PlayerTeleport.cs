using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerTeleport : PlayerFrameBehaviour
{
    [SerializeField] float tpOffsetX;

    [SerializeField] string teleportAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = teleportAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 15:
                EndAnimation();

                break;
        }

        AnimatorSetFrame();
    }
}
