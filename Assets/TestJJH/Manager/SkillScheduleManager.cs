using System.Collections.Generic;
using UnityEngine;

public class SkillScheduleManager : BaseManager, IUpdatableManager
{
    private CardSkillScheduler m_cardSkillScheduler;
    private UnitSkillScheduler m_unitSkillScheduler;

    private ISkillScheduler m_currentSkillScheduler;

    [SerializeField]
    private float SKILLOPERATETIME;
    private float m_currentSkillTime;
    public void RegistCardSkill(Unit unit, CardSkillTableData cardSkill, CardTableData Card)
    {
        m_cardSkillScheduler.RegisteSkill(unit, cardSkill, Card);   
    }

    public void RegistUnitSkill(Unit unit, CardSkillTableData cardSkill, CardTableData Card)
    {
        m_unitSkillScheduler.RegisteSkill(unit, cardSkill, Card);
    }

    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;

        m_cardSkillScheduler = new CardSkillScheduler();
        m_unitSkillScheduler = new UnitSkillScheduler();

        m_currentSkillTime = 0;

        m_currentSkillScheduler = m_cardSkillScheduler;

        m_cardSkillScheduler.Initialize(masterManager, turnManager);
        m_unitSkillScheduler.Initialize(masterManager, turnManager);
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
        
    }

    public void Execute()
    {
        //dont have a registered skill or a registered skill
        if (m_currentSkillScheduler.Caster() == null && 
            m_cardSkillScheduler.SkillQueueIsEmpty() && 
            m_unitSkillScheduler.SkillQueueIsEmpty()) 
            return;

        if (!m_cardSkillScheduler.SkillQueueIsEmpty()) m_currentSkillScheduler = m_cardSkillScheduler;
        if (!m_unitSkillScheduler.SkillQueueIsEmpty()) m_currentSkillScheduler = m_unitSkillScheduler;

        //dont have a current registered skill
        if (m_currentSkillScheduler.Caster() == null)
        {
            ScheduleEnter();
        }

        //After skill registration
        //unless it's a valid skill registration without a unit
        if (m_currentSkillScheduler.Caster() != null)
        {
            ScheduleExecute();
        }

        //past the allotted time
        if (m_currentSkillTime > SKILLOPERATETIME)
        {
            ScheduleEnd();
        }
    }

    private void ScheduleEnter()
    {
        m_currentSkillScheduler.Enter();
    }

    private void ScheduleExecute()
    {
        m_currentSkillTime += Time.deltaTime;
        m_currentSkillScheduler.Execute();
    }

    private void ScheduleEnd()
    {
        m_currentSkillScheduler.Exit();
        m_currentSkillTime = 0;
    }
}
