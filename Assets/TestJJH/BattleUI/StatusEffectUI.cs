using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectUI : UIObject
{
    [SerializeField]
    private Image m_statusEffectImage;
    [SerializeField]
    private Text m_statusEffectDuration;
    [SerializeField]
    private Text m_statusEffectStackCount;


    public void InitIalize(Sprite image, int duration, int stack)
    {
        m_statusEffectImage.sprite = image;
        
        m_statusEffectDuration.text = duration.ToString();
        if (duration <= 0) m_statusEffectDuration.text = " ";

        m_statusEffectStackCount.text = stack.ToString();
        if (stack <= 0) m_statusEffectStackCount.text = " ";
    }

    public void ReInit(int duration, int stack)
    {
        m_statusEffectDuration.text = duration.ToString();
        if (duration <= 0) m_statusEffectDuration.text = " ";

        m_statusEffectStackCount.text = stack.ToString();
        if (stack <= 0) m_statusEffectStackCount.text = " ";
    }
}
