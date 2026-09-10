using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public abstract class CardPanelSlot : MonoBehaviour
{
    [Header("Card")]
    [SerializeField]
    protected CardSlot m_cardSlot;
    [SerializeField]
    protected int m_slotMaxCount;

    protected ObjectPool<CardSlot> m_slotObjectPool;

    [Header("Pool")]
    [SerializeField]
    protected Transform m_slotPoolTransform;
    [SerializeField]
    protected Transform m_gridTranform;

    protected Dictionary<int, CardSlot> m_slotDic;

    [Header("Grid")]
    [SerializeField]
    protected GridLayoutGroup m_gridLayoutGroup;

    public Transform GridTransform
    {
        get { return m_gridTranform; }
    }

    public virtual void Initialize()
    {
        m_slotObjectPool = new ObjectPool<CardSlot>(m_cardSlot, m_slotMaxCount, m_slotPoolTransform);
        m_slotDic = new Dictionary<int, CardSlot>();

        for (int i = m_gridTranform.childCount - 1; i >= 0; i--)
        {
            Destroy(m_gridTranform.GetChild(i).gameObject);
        }
    }

    public CardSlot GetObject()
    {
        var obj = m_slotObjectPool.GetObject();
        obj.m_inPool = false;
        obj.transform.parent = m_gridTranform;
        return obj;
    }

    public virtual void ReleaseObject(CardSlot cardSlot)
    {
        if(m_slotDic.ContainsKey(cardSlot.s_num))
        {
            m_slotDic.Remove(cardSlot.s_num);
            m_slotObjectPool.ReleaseObject(cardSlot);
            cardSlot.m_inPool = true;
        }
    }

    public virtual void AddSlot(CardSlot cardSlot)
    {
        if(m_slotDic.ContainsKey(cardSlot.s_num))
        {
            Debug.Log(this.gameObject.transform.parent.name + "같은 키가 있는 카드를 추가함");
            ReleaseObject(cardSlot);
            return;
        }
        m_slotDic.Add(cardSlot.s_num, cardSlot);
    }

    public virtual void Synchronization()
    {
        foreach (var slot in m_slotDic.Values)
        {
            if (!slot.m_inPool)
            {
                m_slotObjectPool.ReleaseObject(slot);
            }
        }
        m_slotDic.Clear();
    }

    public abstract void SetCardEvent(CardSlot cardSlot);

    public abstract void OnMouseCardEvent(CardSlot cardSlot);

    public abstract void SetCardEvent();
}
