using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFall : PlayerFrameBehaviour
{
    public bool startFall = false;

    [SerializeField] float fallForce;
    [SerializeField] string fallAnim;

    public override void GoToFrame()
    {
        if (frameNum > 35)
        {
            frameNum %= 36;
        }

        switch (frameNum)
        {
            case 0:
                if (startFall)
                {
                    startFall = false;

                    currentAnimName = fallAnim;
                    AnimatorChangeAnimation(currentAnimName);

                    rb.AddForce(Vector2.down * fallForce, ForceMode2D.Impulse);
                }
                break;
        }

        if (playerController.isGrounded && ShooterGameManager.Instance.currentFrame % 6 == 5)
        {
            EndAnimation();
        }

        AnimatorSetFrame();
    }
}

