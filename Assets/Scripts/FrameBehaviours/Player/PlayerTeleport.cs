using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerTeleport : PlayerFrameBehaviour
{
    public bool goLeft = false;

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
            case 6: //disappear
                playerController.playerCollider.enabled = false;
                break;
            case 8: //teleport behind + flip to face player
                Vector3 myPos = playerController.transform.position;
                Vector3 opponentPos = playerController.oppTransform.position;

                int tpDir = 1;
                if (myPos.x < opponentPos.x)
                {
                    tpDir = 1;

                    playerController.spriteTransform.localScale = new Vector3(Mathf.Abs(playerController.spriteTransform.localScale.x) * -1, 1, 1);
                    playerController.facingLeft = true;
                }
                else if (myPos.x > opponentPos.x)
                {
                    tpDir = -1;

                    playerController.spriteTransform.localScale = new Vector3(Mathf.Abs(playerController.spriteTransform.localScale.x), 1, 1);
                    playerController.facingLeft = false;
                }
                else
                {
                    if (goLeft)
                    {
                        tpDir = -1;

                        playerController.spriteTransform.localScale = new Vector3(Mathf.Abs(playerController.spriteTransform.localScale.x), 1, 1);
                        playerController.facingLeft = false;
                    }
                    else
                    {
                        tpDir = 1;

                        playerController.spriteTransform.localScale = new Vector3(Mathf.Abs(playerController.spriteTransform.localScale.x) * -1, 1, 1);
                        playerController.facingLeft = true;
                    }
                }

                playerController.rb.position = new Vector2(opponentPos.x + tpOffsetX * tpDir, opponentPos.y);
                break;
            case 10: //reappear
                playerController.playerCollider.enabled = true;
                break;
            case 13:
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }

    public override void EndAnimation()
    {
        base.EndAnimation();

        playerController.playerCollider.enabled = true;
    }
}
