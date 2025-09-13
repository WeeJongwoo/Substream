using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkillScheduler
{
    public Unit Caster();
    public bool SkillQueueIsEmpty();
    public void RegisteSkill(Unit unit, CardSkillTableData cardSkill, CardTableData Card);
    public void Initialize(MasterManager masterManager, TurnManager turnManager);
    public void Execute();
    public void Enter();
    public void Exit();
}

public class CardSkillScheduler : ISkillScheduler
{
    MasterManager m_masterManager;

    Queue<CardSkill> m_skillQueue;
    CardSkill NULLSKILL;
    CardSkill m_currentSkill;

    public Unit Caster()
    {
        return m_currentSkill.CasterUnit;
    }

    public bool SkillQueueIsEmpty()
    {
        if (m_skillQueue.Count == 0)
            return true;
        return false;
    }

    public CardSkillScheduler()
    {
        m_skillQueue = new Queue<CardSkill>();
        NULLSKILL = new CardSkill(null, null, null);

        m_currentSkill = NULLSKILL;
    }

    public void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
    }

    public void RegisteSkill(Unit unit, CardSkillTableData cardSkill, CardTableData Card)
    {
        CardSkill temt = new CardSkill(unit, cardSkill, Card);
        m_skillQueue.Enqueue(temt);

        if (cardSkill.NextSkillID != 0)
        {
            CardSkill NextSkill = new CardSkill(unit, DontDestroyOnLoadManager.Instance.CardSkillTable(cardSkill.NextSkillID), Card);
            m_skillQueue.Enqueue(NextSkill);
        }
    }

    public void Enter()
    {
        if (m_skillQueue.Count == 0)
        {
            return;
        }
        m_currentSkill = m_skillQueue.Dequeue();
        m_currentSkill.Enter();
    }

    public void Execute()
    {
        Debug.Log(m_currentSkill.SkillData.ID+","+ m_currentSkill.CasterCard.ID + ":" + m_currentSkill.CasterCard.CardText );
        m_currentSkill.Execute();
    }

    public void Exit()
    {
        if (m_currentSkill.SkillData.NextSkillID != 0)
        {
            m_currentSkill = m_skillQueue.Dequeue();
        }
        else
        {
            m_currentSkill.End();
            m_currentSkill = NULLSKILL;
        }
    }
}

public class UnitSkillScheduler : ISkillScheduler
{
    MasterManager m_masterManager;

    Queue<CardSkill> m_skillQueue;
    CardSkill NULLSKILL;
    CardSkill m_currentSkill;

    public Unit Caster()
    {
        return m_currentSkill.CasterUnit;
    }

    public bool SkillQueueIsEmpty()
    {
        if (m_skillQueue.Count == 0)
            return true;
        return false;
    }

    public UnitSkillScheduler()
    {
        m_skillQueue = new Queue<CardSkill>();
        NULLSKILL = new CardSkill(null, null, null);

        m_currentSkill = NULLSKILL;
    }

    public void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
    }

    public void RegisteSkill(Unit unit, CardSkillTableData cardSkill, CardTableData Card)
    {
        CardSkill temt = new CardSkill(unit, cardSkill, Card);
        m_skillQueue.Enqueue(temt);

        if (cardSkill.NextSkillID != 0)
        {
            CardSkill NextSkill = new CardSkill(unit, DontDestroyOnLoadManager.Instance.CardSkillTable(cardSkill.NextSkillID), Card);
            m_skillQueue.Enqueue(NextSkill);
        }
    }

    public void Enter()
    {
        if (m_skillQueue.Count == 0)
        {
            return;
        }
        m_currentSkill = m_skillQueue.Dequeue();
        m_currentSkill.Enter();
    }

    public void Execute()
    {
        Debug.Log(m_currentSkill.CasterCard.ID + ":" + m_currentSkill.CasterCard.CardText);
        m_currentSkill.Execute();
    }

    public void Exit()
    {
        if (m_currentSkill.SkillData.NextSkillID != 0)
        {
            m_currentSkill = m_skillQueue.Dequeue();
        }
        else
        {
            m_currentSkill.End();
            m_currentSkill = NULLSKILL;
        }
    }
}