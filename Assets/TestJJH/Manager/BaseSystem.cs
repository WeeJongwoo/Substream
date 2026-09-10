using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static UnityEngine.GraphicsBuffer;

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

public abstract class UnitManagingSystem : BaseSystem
{
    [SerializeField]
    protected Dictionary<int, Unit> m_units;

    protected int m_partyCount;

    protected bool m_isSystemAboutCharacter;

    protected TurnManager m_turnManager;

    public readonly Dictionary<EStatusEffectType, IStatusEffectStrategy> StatusEffectExecuteStrategy = new Dictionary<EStatusEffectType, IStatusEffectStrategy> {
        {EStatusEffectType.E_NONE, new NoneStatusEffectStrategy() },
        {EStatusEffectType.E_BLEED, new BleedStatusEffectStrategy() },
        {EStatusEffectType.E_SHOCK, new ShockStatusEffectStrategy() },
        {EStatusEffectType.E_OVERLOAD, new OverLoadStatusEffectStrategy() },
        };

    public List<Unit> Units
    {
        get 
        {
            List <Unit> result = new List<Unit>(m_units.Count);
            foreach (var unit in m_units.Values)
            {
                result.Add(unit);
            }
            return result; 
        }
    }

    public Unit Unit(int position)
    {
        return m_units[position];
    }

    public override void SetTurn()
    {
        
    }

    public void SetTurn(Flow flow)
    {
        foreach(var u in m_units)
        {
            u.Value.SetTurn(flow);
        }
    }

    public override void SetRound()
    {
        
    }

    public void SetRound(Flow flow)
    {
        foreach (var u in m_units)
        {
            u.Value.SetRound(flow);
        }
    }

    public void DamageEventBundle(Flow flow, BattleContext context, int pos)
    {
        // 흡혈 번들
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        Unit CastUnit = m_units[pos];
        float Value = flow.Collector.GetResults<ChangeHPResult>().Sum(r => r.IsDamage ? r.Amount : 0);

        // 회복량 계산
        float FinalAmount = Value * context.EffectValue;


        // 초과 회복 계산
        float OverHeal = Unit(pos).HealthValue.Now + FinalAmount - Unit(pos).HealthValue.Max;

        // 오버 힐 처리
        if (OverHeal > 0) FinalAmount -= OverHeal;

        Unit(pos).HealthValue.Now += FinalAmount;

        // 기록
        var Record = new ChangeHPResult()
        {
            Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = pos },
            IsDamage = false,
            AttackStatusType = EStatusEffectType.E_NONE,
            Amount = FinalAmount,
        };
        flow.Record(Record);
    }

    public float ResolveActionUseStatValue(Flow flow, BattleContext context, Unit unit, int targetPosition)
    {
        float result = 1;
        switch (context.SkillSource)
        {
            case EStatSource.E_NONE:
                result = 0;
                break;
            case EStatSource.E_FIXED:
                break;

            case EStatSource.E_MAXAETHER:
                result = m_turnManager.CurrentTurnMaxEtherCount;
                break;
            case EStatSource.E_AETHER:
                result = m_turnManager.CurrentAetherCount;

                break;
            case EStatSource.E_DAMAGED_INFLICTED:
                result = flow.TotalDamage;

                break;
            case EStatSource.E_MAXHP:
                result = unit.HealthValue.Max;
                break;
            case EStatSource.E_DEFAULTHP:
                result = unit.HealthValue.Base;
                break;
            case EStatSource.E_LOSTHP:
                result = unit.HealthValue.Max - unit.HealthValue.Now;
                break;
            case EStatSource.E_HP:
                result = unit.HealthValue.Now;
                break;

            case EStatSource.E_DEFAULTATK:
                result = unit.AttackValue.Base;
                break;
            case EStatSource.E_ATK:
                result = unit.AttackValue.Now;
                break;


            case EStatSource.E_DEFAULTDEF:
                result = unit.DefendValue.Base;
                break;
            case EStatSource.E_DEF:
                result = unit.DefendValue.Now;
                break;

            case EStatSource.E_SPEED:
                result = unit.SpeedValue.Now;
                break;

            case EStatSource.E_CRITICALRATE:
                result = unit.CriticalRate.Now;
                break;
            case EStatSource.E_CRITICALDAMAGE:
                result = unit.CriticalDamageValue.Now;
                break;

            case EStatSource.E_AETHERRECOVER:
                result = unit.AetherRecoverValue.Now;
                break;
            case EStatSource.E_PENETRATION:
                result = unit.PenetrationRate.Now;
                break;
            case EStatSource.E_SHIELD:
                result = unit.ShieldValue.Now;
                break;
        }
        return result;
    }

    public float ResolveActionScaleStatValue(Flow flow, BattleContext context, Unit unit, int targetPosition)
    {
        float result = 1;
        switch (context.ScaleType)
        {
            case EScaleType.E_NONE:
            case EScaleType.E_FIXED:
            case EScaleType.E_DECK:
                break;
            case EScaleType.E_MAXAETHER:
                result = m_turnManager.CurrentTurnMaxEtherCount;
                break;
            case EScaleType.E_AETHER:
                result = m_turnManager.CurrentAetherCount;
                break;

            case EScaleType.E_DAMAGED_INFLICTED:
                result = (int)(flow.TotalDamage);
                break;

            case EScaleType.E_MAXHP:
                result = (int)unit.HealthValue.Max;
                break;
            case EScaleType.E_DEFAULTHP:
                result = (int)unit.HealthValue.Base;
                break;
            case EScaleType.E_LOSTHP:
                result = (int)(unit.HealthValue.Max - unit.HealthValue.Now);
                break;
            case EScaleType.E_HP:
                result = (int)unit.HealthValue.Now;
                break;

            case EScaleType.E_DEFAULTATK:
                result = (int)unit.AttackValue.Base;
                break;
            case EScaleType.E_ATK:
                result = (int)unit.AttackValue.Now;
                break;


            case EScaleType.E_DEFAULTDEF:
                result = (int)unit.DefendValue.Base;
                break;
            case EScaleType.E_DEF:
                result = (int)unit.DefendValue.Now;
                break;

            case EScaleType.E_SPEED:
                result = (int)unit.SpeedValue.Now;
                break;

            case EScaleType.E_CRITICALRATE:
                result = (int)unit.CriticalRate.Now;
                break;
            case EScaleType.E_CRITICALDAMAGE:
                result = (int)unit.CriticalDamageValue.Now;
                break;

            case EScaleType.E_AETHERRECOVER:
                result = (int)unit.AetherRecoverValue.Now;
                break;
            case EScaleType.E_PENETRATION:
                result = (int)unit.PenetrationRate.Now;
                break;
            case EScaleType.E_SHIELD:
                result = (int)unit.ShieldValue.Now;
                break;

            case EScaleType.E_BLEED:
                result = unit.GetNumericStatusEffect(EStatusEffectType.E_BLEED);
                break;
            case EScaleType.E_SHOCK:
                result = unit.GetNumericStatusEffect(EStatusEffectType.E_SHOCK);
                break;
            case EScaleType.E_OVERLOAD:
                result = unit.GetNumericStatusEffect(EStatusEffectType.E_OVERLOAD);
                break;
        }
        if(result > context.ScaleLimit)
        {
            if(context.ScaleLimit != 99)
                result = context.ScaleLimit;
        }

        return result;
    }

    public void DamageToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
        float Stat = ResolveActionUseStatValue(flow, context, CastUnit, targetPosition);
        
        // 피해량 계수
        float TrueAmount = context.EffectValue;

        float ScaleCount = ResolveActionScaleStatValue(flow, context, CastUnit, targetPosition);

        float ScaleFactor = context.ScaleFactor;

        // 증폭 효과
        float ScaleAmount = (ScaleCount * ScaleFactor);

        // 고정 계수 사용 안하면
        if (context.SkillSource != EStatSource.E_FIXED)
        {
            // 피해량 계산
            TrueAmount *= Stat;
            ScaleAmount *= TrueAmount;
        }

        for (int i = 0; i < context.HitCount; i++)
        {
            // 치명타 적용
            float FinalAmount = (1.0f + (context.IsCritical ? CastUnit.CriticalDamageValue.Now : 0.0f)) * TrueAmount;

            // 최종 데미지
            FinalAmount += ScaleAmount;

            float DEFPoint = Unit(targetPosition).DefendValue.Now;

            DEFPoint = (DEFPoint - DEFPoint * (1 - CastUnit.PenetrationRate.Now));

            // 방어력 + 감쇄 효과
            FinalAmount = FinalAmount * (1 - DEFPoint / (DEFPoint + 1000));

            // 피해량 적용
            Unit(targetPosition).ToDamage(flow, true, FinalAmount);
        }

        // 사망 콜
        if (Unit(targetPosition).HealthValue.Now < 0)
        {
            m_masterManager.UnitDying(Unit(targetPosition));
            return;
        }

        AddStatusEffectToUnit(flow, context, targetPosition);
    }

    public void BounceToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
        float Stat = ResolveActionUseStatValue(flow, context, CastUnit, targetPosition);

        // 피해량 계수
        float TrueAmount = context.EffectValue;

        // 고정 계수 사용 안하면
        if (context.SkillSource != EStatSource.E_FIXED)
        {
            // 피해량 계산
            TrueAmount *= Stat;
        }

        for (int i = 0; i < context.HitCount; i++)
        {
            // 치명타 적용
            float FinalAmount = (1.0f + (context.IsCritical ? CastUnit.CriticalDamageValue.Now : 0.0f)) * TrueAmount;

            float DEFPoint = Unit(targetPosition).DefendValue.Now;

            DEFPoint = (DEFPoint - DEFPoint * (1 - CastUnit.PenetrationRate.Now));

            // 방어력 + 감쇄 효과
            FinalAmount = FinalAmount * (1 - DEFPoint / (DEFPoint + 1000));

            // 피해량 적용
            Unit(targetPosition).ToDamage(flow, true, FinalAmount);
        }

        // 사망 콜
        if (Unit(targetPosition).HealthValue.Now < 0)
        {
            m_masterManager.UnitDying(Unit(targetPosition));
            return;
        }

        AddStatusEffectToUnit(flow, context, targetPosition);


        // 바운스 적용
        float ScaleCount = ResolveActionScaleStatValue(flow, context, CastUnit, targetPosition);

        float ScaleFactor = context.ScaleFactor;

        // 증폭 효과
        float ScaleAmount = ScaleCount * ScaleFactor;

        for (int j = 0; j < ScaleAmount; j++)
        {
            // 새로운 타겟
            int NewTargetPosition = UnityEngine.Random.Range(0, m_units.Keys.Count);
            int i = 0;
            foreach(int p in m_units.Keys)
            {
                if(i == NewTargetPosition)
                {
                    NewTargetPosition = m_units[p].Position;
                    break;
                }
                i++;
            }

            // 새로운 치명타 여부
            float CriticalTriggerRate = ((CardAbilityFlowInput)(flow.Input)).CasterUnit.CriticalRate.Now * 100;
            bool IsCritical = (UnityEngine.Random.Range(0, 101) < (CriticalTriggerRate * 100));

            // 치명타 적용
            float FinalAmount = (1.0f + (IsCritical ? CastUnit.CriticalDamageValue.Now : 0.0f)) * TrueAmount;

            float DEFPoint = Unit(NewTargetPosition).DefendValue.Now;

            // 감쇄 효과
            DEFPoint = (DEFPoint - DEFPoint * (1 - CastUnit.PenetrationRate.Now));

            // 방어력
            FinalAmount = FinalAmount * (1 - DEFPoint / (DEFPoint + 1000));

            // 피해량 적용
            Unit(targetPosition).ToDamage(flow, true, FinalAmount);
        }
    }

    public void HealToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
        float Stat = ResolveActionUseStatValue(flow, context, CastUnit, targetPosition); ;

        // 피해량 계수
        float TrueAmount = context.EffectValue;

        float ScaleCount = ResolveActionScaleStatValue(flow, context, CastUnit, targetPosition);

        float ScaleFactor = context.ScaleFactor;

        // 증폭 효과
        float ScaleAmount = (ScaleCount * ScaleFactor);

        // 고정 계수 사용 안하면
        if (context.SkillSource != EStatSource.E_FIXED)
        {
            // 피해량 계산
            TrueAmount *= Stat;

            ScaleAmount *= TrueAmount;
        }

        for (int i = 0; i < context.HitCount; i++)
        {

            // 치명타 적용
            // float FinalAmount = (1.0f + (context.IsCritical ? CastUnit.CriticalValueRate.Now : 0.0f)) * FinalAmount;

            // 최종 힐량
            float FinalAmount = TrueAmount + ScaleAmount;

            Unit(targetPosition).ToHeal(flow, false, FinalAmount);
        }

        AddStatusEffectToUnit(flow, context, targetPosition);
    }

    public void ShieldToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;
        
        // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
        float Stat = ResolveActionUseStatValue(flow, context, CastUnit, targetPosition); ;

        // 피해량 계수
        float TrueAmount = context.EffectValue;

        float ScaleCount = ResolveActionScaleStatValue(flow, context, CastUnit, targetPosition);

        float ScaleFactor = context.ScaleFactor;

        // 증폭 효과
        float ScaleAmount = (ScaleCount * ScaleFactor);

        // 고정 계수 사용 안하면
        if (context.SkillSource != EStatSource.E_FIXED)
        {
            // 피해량 계산
            TrueAmount *= Stat;
            // 증폭 효과
            ScaleAmount *= TrueAmount;
        }

        for (int i = 0; i < context.HitCount; i++)
        {
            // 치명타 적용
            // TrueAmount = (1.0f + (context.IsCritical ? CastUnit.CriticalDamage.Now : 0.0f)) * TrueAmount;

            // 최종 데미지
            float FinalAmount = TrueAmount + ScaleAmount;

            Unit(targetPosition).ToSheild(flow, FinalAmount);
        }

        AddStatusEffectToUnit(flow, context, targetPosition);
    }

    public void NumericStatusEffecttToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        if (m_isSystemAboutCharacter)
        {
            Debug.Log("캐릭터 시스템의 유닛에게 수치 변화 추가" + targetPosition);
        }
        else
        {
            Debug.Log("몬스터 시스템의 유닛에게 수치 변화 추가" + targetPosition);
        }

        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
        float Stat = ResolveActionUseStatValue(flow, context, CastUnit, targetPosition); ;

        // 피해량 계수
        float TrueAmount = context.EffectValue;

        float ScaleCount = ResolveActionScaleStatValue(flow, context, CastUnit, targetPosition);

        float ScaleFactor = context.ScaleFactor;

        // 증폭 효과
        float ScaleAmount = (ScaleCount * ScaleFactor);

        if (context.SkillSource != EStatSource.E_FIXED)
        {
            // 증감 수치 계산
            TrueAmount *= Stat;

            TrueAmount *= ScaleAmount;
            // 치명타 적용
            // FinalAmount = (1.0f + (context.IsCritical ? context.CriticalValueRate : 0.0f)) * FinalAmount;
        }

        for (int i = 0; i < context.HitCount; i++)
        {
            float FinalAmount = TrueAmount;

            Unit(targetPosition).AddVaritationStat(flow, context.TargetStatSource, context.RoundDuration, context.StatusDuration, FinalAmount);
        }
    }

    public void AddStatusEffectToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        if (context.StatusType == EStatusEffectType.E_NONE || context.StatusType == EStatusEffectType.E_NUM) return;

        if (m_isSystemAboutCharacter)
        {
            Debug.Log("캐릭터 시스템의 유닛에게 상태이상 추가" + targetPosition);
        }
        else
        {
            Debug.Log("몬스터 시스템의 유닛에게 상태이상 추가" + targetPosition);
        }

        Unit CastUnit;
        CardAbilityFlowInput Input = (CardAbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        for (int i = 0; i < context.HitCount; i++)
        {
            Unit(targetPosition).AddStatusEffect(flow, context.StatusType, context.RoundDuration, context.StatusDuration, context.TriggerConditionValue);
        }
    }

    public void CheckTargetAlive(int targetPosition)
    {
        if (targetPosition - 1 >= m_units.Count || targetPosition < 1)
        {
            Debug.LogError("타겟팅 실패 확인 바람" + targetPosition);
            return;
        }
    }

    public bool IsAlive(Unit unit)
    {
        return m_units.ContainsValue(unit);
    }
}