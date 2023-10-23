using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterGameManager : MonoBehaviour
{
    int currentFrame;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        Physics2D.Simulate(Time.fixedDeltaTime);
        currentFrame++;
    }
}
