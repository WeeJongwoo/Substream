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
    [SerializeField]
    private Text m_nameText;

    public Text NameText
    {
        get { return m_nameText; }
    }

    public Image Portrait
    {
        get { return m_portrait; }
    }

    public Image Arrow
    {
        get { return m_arrowImage; }
    }
}
