using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SpineRendererOrder : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        MeshRenderer m_meshRenderer = GetComponent<MeshRenderer>();
        m_meshRenderer.sortingOrder = 1;
        
    }
}
