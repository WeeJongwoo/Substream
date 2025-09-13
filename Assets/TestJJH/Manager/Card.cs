using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class Card
{
    private Unit m_unit;
    private CardTableData m_cardData;
    private CardSkillTableData m_Skills;
    private SkillScheduleManager m_skillScheduler;
    public void Initialize(Unit unit, CardTableData cardData, SkillScheduleManager skillScheduler)
    {
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
        };

        m_skillScheduler = skillScheduler;
        
        DontDestroyOnLoadManager ddo = DontDestroyOnLoadManager.Instance;
        CardSkillTableData Skill = ddo.CardSkillTable(m_cardData.SkillID);
        m_Skills = (Skill);
    }

    public CardTableData CardData
    {
        get { return m_cardData; }
        set { m_cardData = value; }
    }

    public void Execute()
    {
        //Debug.Log("Use Card In Turn: " + m_cardData.ID + " " + m_cardData.Name + " " + m_cardData.CardType + " " + m_cardData.CardRarity + " " + m_cardData.Cost + " " + m_cardData.Texture + " " + m_cardData.CardText + " " + m_cardData.SkillID);
        switch (m_cardData.CardType)
        {
            case ECardType.E_DEFAULT:
                Debug.Log("DEFAULT DATA IS IN CARD :" + m_cardData.ID + "," + m_cardData.Name);
                break;
            case ECardType.E_ATTACK:

                break;
            case ECardType.E_SKILL:
                break;
        }

        m_skillScheduler.RegistCardSkill(m_unit, m_Skills, CardData);
    }
}