using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerLightning : PlayerFrameBehaviour
{
    [SerializeField] float ySpawnPos;
    [SerializeField] string attackAnim;

    Vector3 lightningPos;

    public override void GoToFrame()
    {
        switch (frameNum)
        {
            case 0:
                currentAnimName = attackAnim;
                AnimatorChangeAnimation(currentAnimName);
                break;
            case 6: //set position
                lightningPos = new Vector3(playerController.oppTransform.position.x, ySpawnPos, 0);
                break;
            case 20: //create lightning
                GameObject lightningObj = ShooterGameManager.Instance.GetPooledSpell("Lightning");

                SpellLightning spellLightning = lightningObj.GetComponent<SpellLightning>();
                spellLightning.spawnPos = lightningPos;
                spellLightning.ownerNum = playerController.playerNum;
                spellLightning.owner = playerController;

                lightningObj.transform.rotation = Quaternion.identity;

                //run frame 0 of explosion
                ++spellLightning.frameNum;
                spellLightning.GoToFrame();

                break;
            case 32:
                EndAnimation();
                break;
        }

        AnimatorSetFrame();
    }
}
