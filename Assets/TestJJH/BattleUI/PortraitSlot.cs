using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortraitSlot : MonoBehaviour
{
    [SerializeField]
    private Image m_portrait;
    [SerializeField]
    private Image m_arrowImage;

    public Image Portrait
    {
        get { return m_portrait; }
        set { m_portrait = value; }
    }

    public Image Arrow
    {
        get { return m_arrowImage; }
        set { m_arrowImage = value; }
    }
}
