using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowScheduleManager : BaseSystem, IUpdatableManager
{
    private CharacterManager m_characterManager;
    private MonsterManager m_monsterManager;
    private TurnManager m_turnManager;

    private SkillScheduler m_cardSkillScheduler;
    private SkillScheduler m_unitSkillScheduler;
    private SkillScheduler m_actionScheduler;


    private SkillScheduler m_currentSkillScheduler;

    [SerializeField]
    private float SKILLOPERATETIME;
    private float m_skillTimeRate;
    private bool m_isRunning = false;

    private Queue<Flow> m_cardAbillityFlows;
    private Queue<Flow> m_skillAbillityFlows;
    private Queue<Flow> m_systmeAbillityFlows;

    private ActionOrchestrator m_actionOrchestrator;
    private FlowRecorder m_flowRecorder;

    private BattleFacade m_battleFacade;
    private UIFacade m_UIFacade;

    private Dictionary<ESkillType, IActionStrategy> m_strategies;

    public FlowScheduleManager()
    {
        m_strategies = new Dictionary<ESkillType, IActionStrategy>{
            { ESkillType.E_DAMAGE, new DamageSkillStrategy() },
            { ESkillType.E_HEAL, new HealingSkillStrategy() },
            { ESkillType.E_INCREASE, new IncreaseSkillStrategy() },
            { ESkillType.E_SHIELD, new ShieldSkillStrategy() },
            { ESkillType.E_DEBUFF, new DebuffSkillStrategy()},
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
        
        m_currentSkillScheduler = m_cardSkillScheduler;

        m_cardSkillScheduler = new SkillScheduler();
        m_unitSkillScheduler = new SkillScheduler();
        m_actionScheduler = new SkillScheduler();

        m_cardAbillityFlows = new Queue<Flow>();
        m_skillAbillityFlows = new Queue<Flow>();
        m_systmeAbillityFlows = new Queue<Flow>();

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

    public Flow SelectFlow()
    {
        if (m_systmeAbillityFlows.Count > 0)
            return m_systmeAbillityFlows.Dequeue();

        if (m_skillAbillityFlows.Count > 0)
            return m_skillAbillityFlows.Dequeue();

        if (m_cardAbillityFlows.Count > 0)
            return m_cardAbillityFlows.Dequeue();

        return null;
    }

    public IEnumerator ProcessFlow()
    {
        Flow flow = SelectFlow();
        if(flow != null)
        {
            var contexts = m_actionOrchestrator.AnalyzeFlow(flow);
            yield return null;
            foreach(var c in contexts)
            {
                ResolveTargetInContext(flow, c);
                WriteContext(flow, c);
                Execute(flow, c, m_battleFacade, m_UIFacade);
            }
        }
    }

    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        if (m_strategies.TryGetValue(context.SkillType, out var strategy))
        {
            return strategy.Execute(flow, context, battleFacade, uiFacade);
        }
        return false;
    }

    public void Execute()
    {
        if(!m_isRunning)
        {
            if (m_cardAbillityFlows.Count + m_skillAbillityFlows.Count + m_systmeAbillityFlows.Count > 0)
            {
                StartCoroutine(ProcessFlow());
            }
        }
        m_isRunning = true;

        m_isRunning = false;
        if(m_UIFacade.EventQueueIsNotEmpty())
        {
            m_UIFacade.Execute();
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

    public void ResolveTargetInContext(Flow flow, ActionContext context)
    {
#if UNITY_EDITOR
        Debug.Log("###########스킬 로드 시작###########");
#endif
        bool thisUnitIsCharacter = false;
        int thisUnitpos = -1;
        bool thisSkillIsTargetAllies;
        int targetCount = -1;
        int targetPartMaxCount = -1;

        switch (flow.Input)
        {
            case AbilityFlowInput Input:
                thisUnitIsCharacter = Input.CasterUnit.IsCharacter;
                thisUnitpos = Input.CasterUnit.Position;
                break;
            case UnitDyingFlowInput Input:
                thisUnitIsCharacter = Input.Victim.IsCharacter;        
                thisUnitpos = Input.Victim.Position;
                break;
            case TurnEndFlowInput Input:
                thisUnitIsCharacter = m_turnManager.CurrentTurnUnit.IsCharacter;        
                break;
        }

#if UNITY_EDITOR
        Debug.Log("###########스킬 로드 완료###########");
        Debug.Log("###########타겟 지정 시작###########");
#endif
        
        /// 캐릭터 * 아군 대상 = 1 * 1 = 1 = 아군
        /// 캐릭터 * 상대 대상 = 1 * -1 = -1 = 상대
        /// 몬스터 * 아군 대상 = -1 * 1 = -1 = 상대
        /// 몬스터 * 상대 대상 = -1 * -1 = 1 = 아군
        int a = thisUnitIsCharacter ? 1 : -1;
        int b = context.SkillData.TargetType != ESkillTargetType.E_ENEMY ? 1 : -1;
        thisSkillIsTargetAllies = a * b == 1 ? true : false;
        if (thisSkillIsTargetAllies)
        {
            //Debug.Log("캐릭터를 대상으로 함");
        }
        else
        {
            //Debug.Log("몬스터를 대상으로 함");
        }
        CalculateTargetCount(thisSkillIsTargetAllies, out targetCount, out targetPartMaxCount, context.SkillData.TargetCount);

        switch (context.SkillData.TargetType)
        {
            case ESkillTargetType.E_SELF:
                context.TargetUnits.Add(new TargetPair(thisSkillIsTargetAllies, thisUnitpos));
                for (int i = 1; i < context.SkillData.TargetCount;)
                {
                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, UnityEngine.Random.Range(0, targetPartMaxCount));
                    if (!context.TargetUnits.Contains(newTarget))
                    {
                        context.TargetUnits.Add(newTarget);
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

                    if (!context.TargetUnits.Contains(newTarget))
                    {
                        context.TargetUnits.Add(newTarget);
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
    }

    private void WriteContext(Flow flow, ActionContext context)
    {
        m_isRunning = true;
#if UNITY_EDITOR
        Debug.Log("###########스킬 사용 시작###########");
#endif
        float CriticalTriggerRate = 0;
        switch (flow.Input)
        {
            case AbilityFlowInput Input:
                CriticalTriggerRate = Input.CasterUnit.CriticalTriggerRate.Now;
                break;
            case UnitDyingFlowInput Input:
                break;
            case TurnEndFlowInput Input:
                break;
        }

        context.SkillType = context.SkillData.SkillType;
        context.SkillTrigger = context.SkillData.Trigger;
        context.TriggerConditionValue = context.SkillData.TriggerConditionValue;
        context.SkillSource = context.SkillData.SkillSource;
        context.StatusType = context.SkillData.StatusType;
        context.IsCritical = (UnityEngine.Random.Range(0, 101) < (CriticalTriggerRate * 100));
        context.Value = context.SkillData.EffectValue;
        context.TargetSource = context.SkillData.TargetSource;
    }

    /*=============================================================================*/
    public override void UnitDying(Unit unit)
    {
        m_unitSkillScheduler.UnitDying(unit);
        m_cardSkillScheduler.UnitDying(unit);
    }

    public void UnitDying(TargetPair targetPair)
    {
        ActionContext s = new ActionContext();
        s.TargetUnits = new List<TargetPair>{ targetPair };
        s.SkillType = ESkillType.E_UNITDYING;


    }

    public void RegistAbilityFlow(Unit unit, CardTableData card, bool casterIsCard)
    {
        foreach (var id in card.SkillID)
        {
            SkillTableData cardSkill = DontDestroyOnLoadManager.Instance.SkillTable(id);
        }
    }

    public void RegistUnitDyingFlow(Unit victim)
    {
        Flow f = new Flow(new UnitDyingFlowInput(victim));
    }

    public void RegistSetTurnEventFlow()
    {
        Flow f = new Flow(new TurnEndFlowInput());
    }
}
