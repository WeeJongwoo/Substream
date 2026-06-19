using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlowScheduleManager : BaseSystem, IUpdatableManager
{
    private CharacterManager m_characterManager;
    private MonsterManager m_monsterManager;
    private TurnManager m_turnManager;



    private FlowScheduler m_cardFlowScheduler;
    private FlowScheduler m_skillFlowScheduler;
    private FlowScheduler m_systemFlowScheduler;

    private FlowScheduler m_currentSkillScheduler;

    private FlowScheduler m_doneFlowScheduler;

    [SerializeField]
    private float SKILLOPERATETIME;
    private float m_skillTimeRate;
    private bool m_isRunningForLogic = false;
    private bool m_isRunningForUI = false;

    private ActionOrchestrator m_actionOrchestrator;
    private FlowRecorder m_flowRecorder;

    private BattleFacade m_battleFacade;
    private UIFacade m_UIFacade;

    private Dictionary<ESkillType, IActionStrategy> m_executeStrategies;

    private ContextWriterFactory m_writerFactory;

    public FlowScheduleManager()
    {
        m_writerFactory = new ContextWriterFactory();

        m_executeStrategies = new Dictionary<ESkillType, IActionStrategy>{
            { ESkillType.E_DAMAGE, new DamageSkillStrategy() },
            { ESkillType.E_HEAL, new HealingSkillStrategy() },

            { ESkillType.E_VARIATION, new VariationSkillStrategy() },
            { ESkillType.E_STATUSEFFECT, new StatusEffectSkillStrategy()},

            { ESkillType.E_SHIELD, new ShieldSkillStrategy() },
            { ESkillType.E_CONDITIONAL_DAMAGE, new ConditionalDamageStrategy() },
            { ESkillType.E_ETC, new ETCStrategy() },

            { ESkillType.E_DRAW, new DrawSkillStrategy() },
            { ESkillType.E_TURNEND, new TurnEndStrategy() },
            { ESkillType.E_UNITDYING, new UnitDyingStrategy() },
        };
    }

    public override void Initialize()
    {
        m_skillTimeRate = SKILLOPERATETIME / 5;
        
        m_currentSkillScheduler = m_cardFlowScheduler;

        m_cardFlowScheduler = new FlowScheduler();
        m_skillFlowScheduler = new FlowScheduler();
        m_systemFlowScheduler = new FlowScheduler();
        m_doneFlowScheduler = new FlowScheduler();

        m_actionOrchestrator = new ActionOrchestrator();
        m_flowRecorder = new FlowRecorder();
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
        m_characterManager = masterManager.CharacterManager;
        m_monsterManager = masterManager.MonsterManager;
        m_turnManager = masterManager.TurnManager;

        m_battleFacade = new BattleFacade(m_masterManager, m_characterManager, m_monsterManager, 
            m_masterManager.CardManager, m_masterManager.TurnManager);
        m_UIFacade = new UIFacade(m_masterManager,
            m_masterManager.CharacterUIManager, m_masterManager.MonsterUIManager,
            m_masterManager.CardUIManager, m_masterManager.TurnUIManager);
    }

    public override void DataInitialize()
    {
        m_actionOrchestrator.DataInitialize(this.DataBase);
    }

    public Flow SelectFlow()
    {
        if (!m_systemFlowScheduler.SkillQueueIsEmpty())
            return m_systemFlowScheduler.GetFlow();

        if (!m_skillFlowScheduler.SkillQueueIsEmpty())
            return m_skillFlowScheduler.GetFlow();

        if (!m_cardFlowScheduler.SkillQueueIsEmpty())
            return m_cardFlowScheduler.GetFlow();

        return null;
    }

    public IEnumerator FlowProcess()
    {
        m_isRunningForLogic = true;
        Flow flow = SelectFlow();
        if (flow != null)
        {
            var contexts = m_actionOrchestrator.AnalyzeFlow(flow);

            // 플로우 기반 전처리 이벤트 번들 위치

            foreach(var c in contexts)
            {
                m_writerFactory.Write(flow, c);
                TargetResolution(flow, c);
                ContextProcessing(flow, c);
            }

            // 플로우 기반 후처리 이벤트 번들 위치

            m_doneFlowScheduler.RegistFlow(flow);
            yield return null;
        }
        m_isRunningForLogic = false;
    }

    public IEnumerator PresentationProcess()
    {
        m_isRunningForUI = true;
        Flow flow = m_doneFlowScheduler.GetFlow();
        if (flow != null)
        {
            yield return StartCoroutine(m_UIFacade.Execute(flow));
        }
        m_isRunningForUI = false;
    }

    public bool ContextProcessing(Flow flow, ActionContext context)
    {
        IActionStrategy strategy;
        switch(context)
        {
            case BattleContext battleContext:
                if (m_executeStrategies.TryGetValue(battleContext.SkillType, out strategy))
                {
                    return strategy.Execute(flow, battleContext, m_battleFacade);
                }
                break;
            case TurnEndActionContext turnEndActionContext:
                m_executeStrategies[ESkillType.E_TURNEND].Execute(flow, context, m_battleFacade);
                break;
            case UnitDyingActionContext unitDyingActionContext:
                m_executeStrategies[ESkillType.E_UNITDYING].Execute(flow, context, m_battleFacade);
                break;
            case DrawCardActionContext drawCardActionContext:
                m_executeStrategies[ESkillType.E_DRAW].Execute(flow, context, m_battleFacade);
                break;
        }
        return false;
    }

    public void Execute()
    {
        if(!m_isRunningForLogic)
        {
            if (!m_cardFlowScheduler.SkillQueueIsEmpty() || !m_skillFlowScheduler.SkillQueueIsEmpty() || !m_systemFlowScheduler.SkillQueueIsEmpty())
            {
                StartCoroutine(FlowProcess());
            }
        }

        if(!m_isRunningForUI)
        {
            if(!m_doneFlowScheduler.SkillQueueIsEmpty())
            {
                StartCoroutine(PresentationProcess());
            }
        }
    }

    public void CalculateTargetCount(bool targetIsCharacter, out int targetCount, out int targetMaxCount, int maxTargetCount)
    {
        if (!targetIsCharacter)
        {
            targetCount = Mathf.Min(maxTargetCount, m_monsterManager.Units.Count);
            targetMaxCount = m_monsterManager.Units.Count;
        }
        else
        {
            targetCount = Mathf.Min(maxTargetCount, m_characterManager.Units.Count);
            targetMaxCount = m_characterManager.Units.Count;
        }
    }

    public void TargetResolution(Flow flow, ActionContext context)
    {
        if (context is not BattleContext ctx) return;
        var battleContext = context as BattleContext;

        bool thisUnitIsCharacter = false;
        int thisUnitPos = -1;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        thisUnitPos = Input.CasterUnit.Position;
        thisUnitIsCharacter = Input.CasterUnit.IsCharacter;

        bool thisSkillIsTargetAllies;
        int targetCount = -1;
        int targetPartMaxCount = -1;

        /// 캐릭터 * 아군 대상 = 1 * 1 = 1 = 아군
        /// 캐릭터 * 상대 대상 = 1 * -1 = -1 = 상대
        /// 몬스터 * 아군 대상 = -1 * 1 = -1 = 상대
        /// 몬스터 * 상대 대상 = -1 * -1 = 1 = 아군
        int a = thisUnitIsCharacter ? 1 : -1;
        int b = battleContext.SkillData.TargetType != ESkillTargetType.E_ENEMY ? 1 : -1;
        thisSkillIsTargetAllies = a * b == 1 ? true : false;
        if (thisSkillIsTargetAllies)
        {
            //Debug.Log("캐릭터를 대상으로 함");
        }
        else
        {
            //Debug.Log("몬스터를 대상으로 함");
        }
        CalculateTargetCount(thisSkillIsTargetAllies, out targetCount, out targetPartMaxCount, battleContext.SkillData.TargetCount);

        switch (battleContext.SkillData.TargetType)
        {
            case ESkillTargetType.E_SELF:
                battleContext.TargetUnits.Add(new TargetPair(thisSkillIsTargetAllies, thisUnitPos));
                for (int i = 1; i < battleContext.SkillData.TargetCount;)
                {
                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, UnityEngine.Random.Range(0, targetPartMaxCount));
                    if (!battleContext.TargetUnits.Contains(newTarget))
                    {
                        battleContext.TargetUnits.Add(newTarget);
                        i++;
                    }
                }
                break;
            case ESkillTargetType.E_ALLIES:
            case ESkillTargetType.E_ENEMY:
                int j = 0;

                for (; j < targetCount;)
                {
                    int pos = UnityEngine.Random.Range(0, targetPartMaxCount);

                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, pos);

                    if (!battleContext.TargetUnits.Contains(newTarget))
                    {
                        battleContext.TargetUnits.Add(newTarget);
                        j++;
                    }
                }
                break;
            case ESkillTargetType.E_NONE:
                Debug.Log("cardSkil TargetType is none");
                break;
            default:
                Debug.Log("cardSkil TargetType is warring");
                break;
        }
        foreach(var unit in battleContext.TargetUnits)
        {
            if(unit.isCharacter)
            {
                if (m_characterManager.Units.Count < unit.position || unit.position < 0)
                    Debug.LogError("캐릭터 대상 타겟팅 오류" + unit.position);
            }
            else
            {
                if (m_monsterManager.Units.Count < unit.position || unit.position < 0)
                    Debug.LogError("몬스터 대상 타겟팅 오류" + unit.position);
            }
        }
    }

    /*=============================================================================*/
    public override void UnitDying(Unit unit)
    {
        m_cardFlowScheduler.UnitDying(unit);
        m_skillFlowScheduler.UnitDying(unit);
        m_systemFlowScheduler.UnitDying(unit);
    }

    public void RegistAbilityFlow(Unit unit, Card card)
    {
        Flow f = new Flow(new AbilityFlowInput(unit, card));

        if(card.CardData.ID > 999)
        {
            m_cardFlowScheduler.RegistFlow(f);
        }
        else
        {
            m_skillFlowScheduler.RegistFlow(f);
        }
    }

    public void RegistUnitDyingInFlow(Unit victim)
    {
        Flow f = new Flow(new UnitDyingFlowInput(victim));
        m_systemFlowScheduler.RegistFlow(f);
    }

    public void RegistSetTurnEventFlow()
    {
        Flow f = new Flow(new TurnEndFlowInput());
        m_systemFlowScheduler.RegistFlow(f);
    }
}
