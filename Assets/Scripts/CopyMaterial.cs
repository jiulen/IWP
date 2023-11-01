using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyMaterial : MonoBehaviour
{
    [SerializeField]
    Graphic graphic;

    private void Awake()
    {
        graphic.material = new Material(graphic.material);
    }
}
