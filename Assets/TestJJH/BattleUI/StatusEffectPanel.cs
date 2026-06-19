using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectPanel : MonoBehaviour
{
    private ObjectPool<StatusEffectUI> m_statusEffectUIPool;
    [SerializeField]
    private Transform m_poolTransform;
    [SerializeField]
    private StatusEffectUI m_statusEffectPrefab;
    [SerializeField]
    private int m_maxStatusEffectUICount;

    private Dictionary<ESkillStatusType, StatusEffectUI> m_statusEffectUIDic = new Dictionary<ESkillStatusType, StatusEffectUI>();

    public Transform PoolTransform
    {
        get { return m_poolTransform; }
    }

    public void Initialize()
    {
        m_statusEffectUIPool = new ObjectPool<StatusEffectUI>(m_statusEffectPrefab, m_maxStatusEffectUICount, m_poolTransform);
    }

    public void ReturnUI(StatusEffectUI statusEffectUI)
    {
        m_statusEffectUIPool.ReleaseObject(statusEffectUI);
    }

    public StatusEffectUI GetUI()
    {
        return m_statusEffectUIPool.GetObject();
    }

    public void ChangeStatusEffect(Sprite uiSprite, ESkillStatusType statusType, int duration, int stack, bool isNew)
    {
        Debug.Log("UI / " + statusType.ToString() + " : " + isNew.ToString());
        if(isNew)
        {
            var ui = GetUI();
            m_statusEffectUIDic.Add(statusType, ui);
            ui.InitIalize(uiSprite, duration, stack);
        }
        else
        {
            if (duration == 0)
            {
                ReturnUI(m_statusEffectUIDic[statusType]);
                return;
            }
            else
            {
                m_statusEffectUIDic[statusType].ReInit(duration, stack);
            }
        }
    }
}
