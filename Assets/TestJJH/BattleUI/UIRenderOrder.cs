using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRenderOrder : MonoBehaviour
{
    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.sortingOrder = 2;
    }
}
