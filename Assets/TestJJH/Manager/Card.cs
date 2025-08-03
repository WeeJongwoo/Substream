using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card
{
    private CardData m_cardData;

    public void Initialize(CardData cardData)
    {
        m_cardData = new CardData
        {
            ID = cardData.ID,
            Name = cardData.Name,
            Type = cardData.Type,
            Rank = cardData.Rank,
            Cost = cardData.Cost,
            TargetCount = cardData.TargetCount,
            Explanation = cardData.Explanation
        };
    }

    public CardData CardData
    {
        get { return m_cardData; }
        set { m_cardData = value; }
    }

    public void Execute()
    {
        Debug.Log("Use Card In Turn: " + m_cardData.ID + 1 + " " + m_cardData.Name + " " + m_cardData.Type + " " + m_cardData.Rank + " " + m_cardData.Cost + " " + m_cardData.TargetCount + " " + m_cardData.Explanation);
        switch (m_cardData.Type)
        {
            case ECardType.E_DEFAULT:
                break;
            case ECardType.E_SKILL:
                break;
        }
    }
}