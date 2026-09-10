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
    private List<SkillTableData> m_skills;
    private FlowScheduleManager m_skillSchedulerManager;

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

    public void Initialize(Unit unit, CardTableData cardData, FlowScheduleManager skillSchedulerManager)
    {
        RecordAmount = 0;
        m_unit = unit;
        unitId = unit.IngameUnitID();
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
        m_skillSchedulerManager = skillSchedulerManager;
        DontDestroyOnLoadManager ddo = DontDestroyOnLoadManager.Instance;
        m_skills = new List<SkillTableData>(m_cardData.SkillID.Count);
        SkillTableData Skill = ddo.SkillTable(m_cardData.SkillID[0]);
        //  public int ID;
        //  public ECardSkillType SkillType;
        //  public ECardSkillSource SkillSource;
        //  public float EffectValue;
        //  public float UpgradeEffectValue;
        //  public ECardSkillStatusType StatusType;
        //  public ECardSkillTargetType TargetType;
        //  public int TargetCount;
        //  public int HitCount;
        //  public string CardText;
        //  public int NextSkillID;
        //  public string Sound;
    }
    public int unitId;
    public CardTableData CardData
    {
        get { return m_cardData; }
        set { m_cardData = value; }
    }

    public Unit Unit
    {
        get { return m_unit; }
    }

    public void Execute()
    {
        m_skillSchedulerManager.RegistAbilityFlow(m_unit, m_cardData, true);
    }
}