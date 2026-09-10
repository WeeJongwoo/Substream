using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

[Serializable]
public class Card
{
    [SerializeField]
    private Unit m_unit;
    [SerializeField]
    private CardTableData m_cardData;

    private float RecordAmount;

    public float RecordedAmount()
    {
        return RecordAmount;
    }

    public void Recorde(float amount)
    {
        RecordAmount += amount;
    }

    public void ReInit()
    {
        RecordAmount = 0;
    }

    public void Initialize(Unit unit, CardTableData cardData)
    {
        RecordAmount = 0;
        m_unit = unit;
        m_cardData = new CardTableData
        {
            ID = cardData.ID,
            Name = cardData.Name,
            CardType = cardData.CardType,
            CardRarity = cardData.CardRarity,
            Cost = cardData.Cost,
            Texture = cardData.Texture,
            CardText = cardData.CardText,
            SkillID = cardData.SkillID,
            TargetType = cardData.TargetType
        };
    }

    public CardTableData CardData
    {
        get { return m_cardData; }
        set { m_cardData = value; }
    }

    public Unit Unit
    {
        get { return m_unit; }
    }
}