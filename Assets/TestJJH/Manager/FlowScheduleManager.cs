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

    private FlowScheduler m_reservationFlowScheduler;

    private FlowScheduler m_skillFlowScheduler;
    private FlowScheduler m_systemFlowScheduler;
    private FlowScheduler m_counterFlowScheduler;

    private FlowScheduler m_doneFlowScheduler;

    [SerializeField]
    private float SKILLOPERATETIME;
    private float m_skillTimeRate;
    private bool m_isRunningForLogic = false;
    private bool m_isRunningForUI = false;

    private ContextGenerator m_contextGenerator;
    private FlowRecorder m_flowRecorder;

    private BattleFacade m_battleFacade;
    private UIFacade m_UIFacade;

    private Dictionary<ESkillType, IActionStrategy> m_executeStrategies;

    public FlowScheduleManager()
    {
        m_executeStrategies = new Dictionary<ESkillType, IActionStrategy>{
            { ESkillType.E_DAMAGE, new DamageSkillStrategy() },
            { ESkillType.E_HEAL, new HealSkillStrategy() },

            { ESkillType.E_VARIATION, new VariationSkillStrategy() },
            { ESkillType.E_STATUSEFFECT, new StatusEffectSkillStrategy()},

            { ESkillType.E_SHIELD, new ShieldSkillStrategy() },
            { ESkillType.E_ETC, new ETCStrategy() },

            { ESkillType.E_DRAW, new DrawSkillStrategy() },
            { ESkillType.E_TURNEND, new TurnEndStrategy() },
            { ESkillType.E_ROUNDEND, new RoundEndStrategy() },
            { ESkillType.E_UNITDYING, new UnitDyingStrategy() },
        };
    }

    public override void Initialize()
    {
        m_skillTimeRate = SKILLOPERATETIME / 5;

        m_reservationFlowScheduler = new FlowScheduler();
        
        m_skillFlowScheduler = new FlowScheduler();
        m_systemFlowScheduler = new FlowScheduler();
        m_counterFlowScheduler = new FlowScheduler();
        
        m_doneFlowScheduler = new FlowScheduler();

        m_contextGenerator = new ContextGenerator();
        m_flowRecorder = new FlowRecorder();

        ContextIdGenerator.Reset();
        FlowIdGenerator.Reset();
        ResultIdGenerator.Reset();
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
        m_contextGenerator.DataInitialize(this.DataBase);
        m_flowRecorder.Initialize(m_characterManager, m_monsterManager);
    }

    public Flow SelectFlow()
    {
        // 반격보다 스킬이 더 빠르면 좋을지 스킬보다 반격이 더 빠르면 좋을지 고민 중
        // 우선도 시스템 들어가면 변경되어야함
        if (!m_systemFlowScheduler.SkillQueueIsEmpty())
            return m_systemFlowScheduler.GetFlow();
        
        if (!m_skillFlowScheduler.SkillQueueIsEmpty())
            return m_skillFlowScheduler.GetFlow();

        if (!m_counterFlowScheduler.SkillQueueIsEmpty())
            return m_counterFlowScheduler.GetFlow();
        // 여기까지가 간섭 플로우 스케쥴러

        if (!m_reservationFlowScheduler.SkillQueueIsEmpty())
            return m_reservationFlowScheduler.GetFlow();
        return null;
    }

    public IEnumerator FlowProcess()
    {
        m_isRunningForLogic = true;
        Flow flow = SelectFlow();
        List<TargetPair> TargetUnits = new List<TargetPair>();
        if (flow != null)
        {
            // 묘지&덱 관련 처리 때문에
            switch (flow.Input)
            {
                case CardAbilityFlowInput Input:
                    m_masterManager.ApplyUseCard(Input.CasterCard);
                    break;
                case SkillAbilityFlowInput Input:
                    break;
                case UnitDyingFlowInput Input:
                    break;
                case TurnEndFlowInput Input:
                    break;
                case RoundEndFlowInput Input:
                    break;
                case SystemDrawCardFlowInput Input:
                    break;
            }

            var contexts = m_contextGenerator.AnalyzeFlow(flow);

            // 플로우 기반 전처리 이벤트 번들 위치

            int i = 0;
            foreach(var c in contexts)
            {
                int resultStartIndex = flow.Collector.ResultCount;

                // 타겟 지정
                if (i == 0)
                {
                    TargetResolutionForFirstContext(flow,c, TargetUnits);
                }
                else
                {
                    TargetResolution(flow, c, TargetUnits);
                }
                
                ContextProcessing(flow, c);
                i++;

                if (c is BattleContext bc)
                {
                    flow.Collector.SetSourceSkillID(resultStartIndex, bc.SkillID);
                }
            }

            // 플로우 기반 후처리 이벤트 번들 위치

            m_flowRecorder.SaveRecord(flow);
            m_doneFlowScheduler.RegistFlow(flow);
            yield return null;
        }
        TargetUnits = null;
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
            case RoundEndActionContext turnEndActionContext:
                m_executeStrategies[ESkillType.E_ROUNDEND].Execute(flow, context, m_battleFacade);
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
            if (!m_reservationFlowScheduler.SkillQueueIsEmpty() || !m_skillFlowScheduler.SkillQueueIsEmpty() || !m_systemFlowScheduler.SkillQueueIsEmpty() || !m_counterFlowScheduler.SkillQueueIsEmpty())
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

    private void TargetResolution(Flow flow, ActionContext context, List<TargetPair> targetUnits)
    {
        if (context is not BattleContext ctx) return;
        var battleContext = context as BattleContext;
        if (!battleContext.RefreshTarget)
        {
            foreach (var targetPair in targetUnits) {
                battleContext.TargetUnits.Add(targetPair);
            }
            return;
        }
        targetUnits.Clear();
        bool thisUnitIsCharacter = false;
        int thisUnitPos = -1;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;

        /// <summary>
        /// /- 선택한 유닛 -/
        /// 아군 유닛 선택 1,2,3,4, 적 선택 -1,-2,-3,-4
        /// 선택 없이 시전 0
        /// 
        /// /- 특이 타겟 유형 -/
        /// E_ADJACENT,
        /// E_CHAINBEHIND
        /// 
        /// </summary>
   
        int selectTargetPosition = Input.SelectTargetPosition;
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
        int b = battleContext.TargetType != ETargetType.E_ENEMY ? 1 : -1;
        thisSkillIsTargetAllies = a * b == 1 ? true : false;
        if (thisSkillIsTargetAllies)
        {
            //Debug.Log("캐릭터를 대상으로 함");
        }
        else
        {
            //Debug.Log("몬스터를 대상으로 함");
        }

        if (!thisSkillIsTargetAllies)
        {
            targetCount = Mathf.Min(battleContext.TargetCount, m_monsterManager.Units.Count);
            battleContext.TargetCount = m_monsterManager.Units.Count;
        }
        else
        {
            targetCount = Mathf.Min(battleContext.TargetCount, m_characterManager.Units.Count);
            battleContext.TargetCount = m_characterManager.Units.Count;
        }

        switch (battleContext.TargetType)
        {
            case ETargetType.E_SELF:
                targetUnits.Add(new TargetPair(thisSkillIsTargetAllies, thisUnitPos));
                for (int i = 1; i < battleContext.TargetCount;)
                {
                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, UnityEngine.Random.Range(0, targetPartMaxCount) + 1);
                    if (!targetUnits.Contains(newTarget))
                    {
                        targetUnits.Add(newTarget);
                        i++;
                    }
                }
                break;
            case ETargetType.E_ALLIES:
            case ETargetType.E_ENEMY:
                int j = 0;

                for (; j < targetCount;)
                {
                    int pos = UnityEngine.Random.Range(0, targetPartMaxCount) + 1;

                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, pos);

                    if (!targetUnits.Contains(newTarget))
                    {
                        targetUnits.Add(newTarget);
                        j++;
                    }
                }
                break;
            case ETargetType.E_ADJACENT:
                TargetPair newAdjacentTarget1;
                newAdjacentTarget1 = new TargetPair(thisSkillIsTargetAllies, Input.SelectTargetPosition - battleContext.TargetCount);
                if (!targetUnits.Contains(newAdjacentTarget1) && newAdjacentTarget1.position > 0)
                {
                    targetUnits.Add(newAdjacentTarget1);
                }

                TargetPair newAdjacentTarget2;
                newAdjacentTarget2 = new TargetPair(thisSkillIsTargetAllies, Input.SelectTargetPosition + battleContext.TargetCount);
                if (!targetUnits.Contains(newAdjacentTarget2) && newAdjacentTarget1.position < targetPartMaxCount)
                {
                    targetUnits.Add(newAdjacentTarget2);
                }
                break;
            case ETargetType.E_CHAINBEHIND:
                TargetPair newChainBehindTarget;
                newChainBehindTarget = new TargetPair(thisSkillIsTargetAllies, Input.SelectTargetPosition + battleContext.TargetCount);
                if (!targetUnits.Contains(newChainBehindTarget) && newChainBehindTarget.position < targetPartMaxCount)
                {
                    targetUnits.Add(newChainBehindTarget);
                }
                break;
            case ETargetType.E_NONE:
                Debug.Log("cardSkil TargetType is none");
                break;
            default:
                Debug.Log("cardSkil TargetType is warring");
                break;
        }
        foreach(var unit in targetUnits)
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
            battleContext.TargetUnits.Add(unit);
        }
    }

    private void TargetResolutionForFirstContext(Flow flow, ActionContext context, List<TargetPair> targetUnits)
    {
        if (context is not BattleContext ctx) return;
        targetUnits.Clear();
        var battleContext = context as BattleContext;

        bool thisUnitIsCharacter = false;
        int thisUnitPos = -1;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;

        /// <summary>
        /// /- 선택한 유닛 -/
        /// 아군 유닛 선택 1,2,3,4, 적 선택 -1,-2,-3,-4
        /// 선택 없이 시전 0
        /// </summary>

        int selectTargetPosition = Input.SelectTargetPosition;
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
        int b = battleContext.TargetType != ETargetType.E_ENEMY ? 1 : -1;
        thisSkillIsTargetAllies = a * b == 1 ? true : false;
        if (thisSkillIsTargetAllies)
        {
            //Debug.Log("캐릭터를 대상으로 함");
        }
        else
        {
            //Debug.Log("몬스터를 대상으로 함");
        }

        if (!thisSkillIsTargetAllies)
        {
            targetCount = Mathf.Min(battleContext.TargetCount, m_monsterManager.Units.Count);
            battleContext.TargetCount = m_monsterManager.Units.Count;
        }
        else
        {
            targetCount = Mathf.Min(battleContext.TargetCount, m_characterManager.Units.Count);
            battleContext.TargetCount = m_characterManager.Units.Count;
        }

        switch (battleContext.TargetType)
        {
            case ETargetType.E_SELF:
                targetUnits.Add(new TargetPair(thisSkillIsTargetAllies, thisUnitPos));
                for (int i = 1; i < battleContext.TargetCount;)
                {
                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, UnityEngine.Random.Range(0, targetPartMaxCount) + 1);
                    if (!targetUnits.Contains(newTarget))
                    {
                        targetUnits.Add(newTarget);
                        i++;
                    }
                }
                break;
            case ETargetType.E_ALLIES:
            case ETargetType.E_ENEMY:
                TargetPair selectTarget;
                selectTarget = new TargetPair(thisSkillIsTargetAllies, Mathf.Abs(selectTargetPosition));

                if (!targetUnits.Contains(selectTarget))
                {
                    targetUnits.Add(selectTarget);
                }

                int j = 1;

                for (; j < targetCount;)
                {
                    int pos = UnityEngine.Random.Range(0, targetPartMaxCount) + 1;

                    TargetPair newTarget;
                    newTarget = new TargetPair(thisSkillIsTargetAllies, pos);

                    if (!targetUnits.Contains(newTarget))
                    {
                        targetUnits.Add(newTarget);
                        j++;
                    }
                }
                break;
            case ETargetType.E_NONE:
                Debug.Log("cardSkil TargetType is none");
                break;
            default:
                Debug.Log("cardSkil TargetType is warring");
                break;
        }
        foreach (var unit in targetUnits)
        {
            if (unit.isCharacter)
            {
                if (m_characterManager.Units.Count < unit.position || unit.position < 0)
                    Debug.LogError("캐릭터 대상 타겟팅 오류" + unit.position);
            }
            else
            {
                if (m_monsterManager.Units.Count < unit.position || unit.position < 0)
                    Debug.LogError("몬스터 대상 타겟팅 오류" + unit.position);
            }
            battleContext.TargetUnits.Add(unit);
        }
    }


    /*=============================================================================*/
    public override void UnitDying(Unit unit)
    {
        m_reservationFlowScheduler.UnitDying(unit);
        m_skillFlowScheduler.UnitDying(unit);
        m_systemFlowScheduler.UnitDying(unit);
        m_counterFlowScheduler.UnitDying(unit);
    }

    public void RegistCharacterSkillAbilityFlow(Card card, int selectTargetPosition)
    {
        Flow f = new Flow(new SkillAbilityFlowInput(card.Unit, card, selectTargetPosition));

        if (card.CardData.ID <= 999)
        {
            m_skillFlowScheduler.RegistFlow(f);
        }
    }

    public void RegistCardAbilityFlow(Card card, int selectTargetPosition)
    {
        if (m_turnManager.IsTurnInputLocked) return;

        Flow f = new Flow(new CardAbilityFlowInput(card.Unit, card, selectTargetPosition));

        if(card.CardData.ID > 999)
        {
            m_reservationFlowScheduler.RegistFlow(f);
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
        m_reservationFlowScheduler.RegistFlow(f);
    }

    public void RegistSetRoundEventFlow()
    {
        Flow f = new Flow(new RoundEndFlowInput());
        m_reservationFlowScheduler.RegistFlow(f);
    }

    public void RegistSystemEventFlow()
    {
        //시스템, 룰 마스터, 오파츠에 의한 간섭
    }

    public override void SetTurn()
    {
        m_flowRecorder.SetTurn();
    }

    public override void SetRound()
    {
        m_flowRecorder.SetRound();
    }
}
