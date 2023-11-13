using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAnimator : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Animator>().speed = 0;
    }
}
