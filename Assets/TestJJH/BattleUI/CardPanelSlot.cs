using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardPanelSlot : MonoBehaviour
{
    [SerializeField]
    private GameObject m_cardSlotPrefab;
    private CardSlot m_cardSlot;

    [SerializeField]
    private int m_slotMaxCount;
    [SerializeField]
    private Transform m_slotPoolTransform;
    [SerializeField]
    private Transform m_gridTranform;
    [SerializeField]
    private Image m_BGI;
    [SerializeField]
    private Button m_closeButton;

    private ObjectPool<CardSlot> m_slotObjectPool;
    private Dictionary<int, CardSlot> m_slotDic;

    [SerializeField]
    private GameObject m_sliderBar;
    [SerializeField]
    private GridLayoutGroup m_gridLayoutGroup;
    [SerializeField]
    private ScrollRect m_scrollRect;

    public ScrollRect ScrollRect
    {
        get { return m_scrollRect; }
    }

    public Transform GridTransform
    {
        get
        {
            return m_gridTranform;
        }
    }

    public Button CloseButton
    { 
        get { return m_closeButton; } 
    }

    public void TurnOn()
    {
        this.gameObject.SetActive(true);
    }

    public void TurnOff()
    {
        this.gameObject.SetActive(false);
    }

    public void Initialize()
    {
        m_cardSlot = m_cardSlotPrefab.GetComponentInChildren<CardSlot>();
        m_slotObjectPool = new ObjectPool<CardSlot>(m_cardSlot, m_slotMaxCount, m_slotPoolTransform);
        m_slotDic = new Dictionary<int, CardSlot>();

        
        for (int i = m_gridTranform.childCount - 1; i >= 0; i--)
        {
            Destroy(m_gridTranform.GetChild(i).gameObject);
        }

        m_gridLayoutGroup = m_gridTranform.gameObject.GetComponent<GridLayoutGroup>();
    }

    public void TurnOnCloseButton()
    {
        m_closeButton.gameObject.SetActive(true);
    }

    public void TurnOffCloseButton()
    {
        m_closeButton.gameObject.SetActive(false);
    }

    public CardSlot GetObject()
    {
        var obj = m_slotObjectPool.GetObject();
        obj.m_inPool = false;
        return obj;
    }

    public void ReleaseObject(CardSlot cardSlot)
    {
        if(m_slotDic.ContainsKey(cardSlot.s_num))
        {
            m_slotDic.Remove(cardSlot.s_num);
            m_slotObjectPool.ReleaseObject(cardSlot);
            cardSlot.m_inPool = true;
        }
    }

    public void AddSlot(CardSlot cardSlot)
    {
        if(m_slotDic.ContainsKey(cardSlot.s_num))
        {
            Debug.Log("같은 키가 있는 카드를 추가함");
            m_slotDic[cardSlot.s_num] = cardSlot;
            return;
        }
        m_slotDic.Add(cardSlot.s_num, cardSlot);
    }

    public void SetTurn()
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

    public void SetCardEvent(CardSlot cardSlot)
    {
        foreach (var slot in m_slotDic)
        {
            if (slot.Value == cardSlot)
            {
                continue;
            }
            slot.Value.s_isReady = false;
            slot.Value.s_isDrag = false;
            slot.Value.MouseExit();
        }
    }
    public void SetCardEvent()
    {
        foreach (var slot in m_slotDic)
        {
            slot.Value.s_isReady = false;
            slot.Value.s_isDrag = false;
            slot.Value.MouseExit();
        }
    }

    public void SetBGI(Color color)
    {
        m_BGI.color = color;
    }
}
