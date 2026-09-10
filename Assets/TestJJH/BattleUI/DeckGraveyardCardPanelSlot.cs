using Spine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckGraveyardCardPanelSlot : CardPanelSlot
{
    [Header("Skill")]
    [SerializeField]
    protected CardSlot m_skillCardSlot;

    public CardSlot SkillCardSlot
    {
        get { return m_skillCardSlot; }
    }

    public override void ReleaseObject(CardSlot cardSlot)
    {
        cardSlot.m_inPool = true;
        m_slotObjectPool.ReleaseObject(cardSlot);
    }

    public override void AddSlot(CardSlot cardSlot)
    {
        cardSlot.gameObject.SetActive(true);
        cardSlot.m_inPool = true;
        cardSlot.transform.parent = m_gridTranform;
    }


    public override void SetCardEvent(CardSlot cardSlot)
    {
        cardSlot.Button.transform.localPosition = (new Vector3(0, 0, 0));
        cardSlot.Button.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
    }

    public override void OnMouseCardEvent(CardSlot cardSlot)
    {
        cardSlot.Button.transform.localPosition = (new Vector3(0, 0, 0));
        cardSlot.Button.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
    }

    public override void SetCardEvent()
    {
        foreach (var slot in m_slotDic)
        {
            slot.Value.Button.transform.localPosition = (new Vector3(0, 0, 0));
            slot.Value.Button.transform.localScale = new Vector3(0.85f, 0.85f, 0.85f);
        }
    }
}
