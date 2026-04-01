using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSystem : BaseManager
{
    public event Action OnSynchronization;
    public override void Synchronization()
    {
        if (OnSynchronization != null)
        {
            Action clone = (Action)OnSynchronization.Clone();
            clone?.Invoke();
        }
    }
}

public abstract class UnitManagerSystme : BaseSystem
{
    [SerializeField]
    protected List<Unit> m_units;

    protected int m_partyCount;

    public readonly Dictionary<ESkillStatusType, IStatusEffectStrategy> StatusEffectExecuteStrategy = new Dictionary<ESkillStatusType, IStatusEffectStrategy> {
        {ESkillStatusType.E_NONE, new NoneStatusEffectStrategy() },
        {ESkillStatusType.E_BLEED, new BleedStatusEffectStrategy() },
        {ESkillStatusType.E_SHOCK, new ShockStatusEffectStrategy() },
        {ESkillStatusType.E_OVERLOAD, new OverLoadStatusEffectStrategy() },
        };

    public List<Unit> Units
    {
        get { return m_units; }
    }

    public Unit Unit(int position)
    {
        return m_units[position];
    }

    public override void SetTurn()
    {
        
    }

    public void DamageToUnit(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        float CriticalDamageValue = 0.3f; // 고정 치명타 피해량 계수
        float FinalAmount = Amount;
        FinalAmount = (1.0f + (isCritical ? criticalDamageRate * CriticalDamageValue : 0.0f)) * FinalAmount;
        FinalAmount = FinalAmount * (1 - m_units[targetPosition].DefendPoint.Now / (m_units[targetPosition].DefendPoint.Now + 1000));
        m_units[targetPosition].HealthPoint.Now -= FinalAmount;

        context.UIApplyHelper.ApplyDamage(context.CasterUnit.isCharacter, context.CasterUnit.position, targetIsCharacter, targetPosition, FinalAmount);

        if(context.StatusType != ESkillStatusType.E_NONE)
        {
            AddStatusEffect(context, 
                targetIsCharacter, targetPosition, 
                context.StatusType, 
                1/*여기 상태 이상 지속 시간 context값*/, 
                0/*여기 지속 피해 관련 수치 context값*/, 
                context.CastUnit);
        }

        // 사망 콜
        if (m_units[targetPosition].HealthPoint.Now < 0)
        {
            m_masterManager.UnitDying(new TargetPair(targetIsCharacter, targetPosition));
            return;
        }
    }

    public void HealToUnit(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        Debug.Log("힐" + Amount);
        m_units[targetPosition].HealthPoint.Now += Amount;

        // 오버 힐 처리
        if (m_units[targetPosition].HealthPoint.Now > m_units[targetPosition].HealthPoint.Max)
            m_units[targetPosition].HealthPoint.Now = m_units[targetPosition].HealthPoint.Max;

        context.UIApplyHelper.ApplyHeal(context.CasterUnit.isCharacter, context.CasterUnit.position, targetIsCharacter, targetPosition, Amount);

        if (context.StatusType != ESkillStatusType.E_NONE)
        {
            AddStatusEffect(context,
                targetIsCharacter, targetPosition,
                context.StatusType,
                1/*여기 상태 이상 지속시간 context값*/,
                0/*여기 지속 피해 관련 수치 context값*/,
                context.CastUnit);
        }

        context.UIApplyHelper.ApplyHeal(context.CasterUnit.isCharacter, context.CasterUnit.position, targetIsCharacter, targetPosition, Amount);
    }

    public void AddStatusEffect(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        m_units[targetPosition].AddBuff(effect, duration, value, caster);
        context.UIApplyHelper.AddStatusEffect(targetIsCharacter, targetPosition, effect, duration, caster);
    }

    public bool IsAlive(Unit unit)
    {
        return m_units.Contains(unit);
    }
}