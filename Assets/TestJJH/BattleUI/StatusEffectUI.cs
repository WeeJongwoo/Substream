using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectUI : MonoBehaviour
{
    private ObjectPool m_statusEffectUIPool;
    [SerializeField]
    private Transform m_poolTransform;
    [SerializeField]
    private GameObject m_statusEffectPrefab;
    [SerializeField]
    private int m_maxStatusEffectUICount;

    public Transform PoolTransform
    {
        get { return m_poolTransform; }
    }

    public void Initialize()
    {
        m_statusEffectUIPool = new ObjectPool(m_statusEffectPrefab, m_maxStatusEffectUICount, m_poolTransform);
    }
}
