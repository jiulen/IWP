using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CopyMaterial : MonoBehaviour
{
    [SerializeField]
    Graphic graphic;

    [SerializeField]
    SpriteRenderer sr;

    private void Awake()
    {
        if (graphic != null) graphic.material = new Material(graphic.material);

        if (sr != null) sr.material = new Material(sr.material);
    }
}
