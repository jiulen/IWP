using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJump : PlayerFrameBehaviour
{
    public bool goLeft = false;

    [SerializeField] float jumpForce, forwardForce;
    [SerializeField] string jumpAnim;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                rb.velocity = Vector2.zero;

                currentAnimName = jumpAnim;
                AnimatorChangeAnimation(currentAnimName);

                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                Vector2 forwardDir = Vector2.right * (goLeft ? -1 : 1);
                rb.AddForce(forwardDir * forwardForce, ForceMode2D.Impulse);

                break;
            case 35: //end
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}

