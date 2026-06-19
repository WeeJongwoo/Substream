using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
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
    protected List<Unit> m_units;

    protected int m_partyCount;

    protected bool m_isSystemAboutCharacter;

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

    public void SetTurn(Flow flow)
    {
        foreach(var u in m_units)
        {
            u.SetTurn(flow);
        }
    }

    public void DamageEventBundle(Flow flow, BattleContext context, int pos)
    {
        // 흡혈 번들
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        Unit CastUnit = m_units[pos];
        float Value = flow.Results.GetResults<ChangeHPResult>().Sum(r => r.IsDamage ? r.Amount : 0);

        // 회복량 계산
        float FinalAmount = Value * context.EffectValue;


        // 초과 회복 계산
        float OverHeal = Unit(pos).HealthPoint.Now + FinalAmount - Unit(pos).HealthPoint.Max;

        // 오버 힐 처리
        if (OverHeal > 0) FinalAmount -= OverHeal;

        Unit(pos).HealthPoint.Now += FinalAmount;

        // 기록
        var Record = new ChangeHPResult()
        {
            Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = pos },
            IsDamage = false,
            AttackStatusType = ESkillStatusType.E_NONE,
            Amount = FinalAmount,
            OverAmount = OverHeal
        };
        flow.Record(Record);
    }

    public float ResolveActionUseStatValue(Flow flow, BattleContext context, Unit unit)
    {
        switch (context.SkillSource)
        {
            case ESkillSource.E_NONE:
                return 0;
            case ESkillSource.E_ATK:
                return unit.AttackPoint.Now;
            case ESkillSource.E_DAMAGED_INFLICTED:
                return flow.TotalDamage;
            case ESkillSource.E_AETHER:
                return context.EffectValue;
            case ESkillSource.E_DECK:
                return context.EffectValue;
            case ESkillSource.E_DEF:
                return unit.DefendPoint.Now;
            case ESkillSource.E_SPEED:
                return unit.SpeedPoint.Now;
            case ESkillSource.E_FIXED:
                return unit.SpeedPoint.Now;
            case ESkillSource.E_HP:
                return unit.HealthPoint.Now;
        }
        return 0;
    }

    public void DamageToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        /// 컨텍스트에서 직접 참조해서 쓸거
        /// SkillType : 스킬의 타입
        /// SkillTrigger : 조건부 추가 데미지
        /// SkillSource : 기준 파라미터 값 
        /// StatusType : 부여하는 상태이상
        /// StatusDuration : 상태이상 지속 시간 (아직 데이터 되지 없음)
        /// IsCritical : 치명타 인지 아닌지
        /// CriticalValueRate : 치명타 계수 배율
        /// Value : 기준 파라미터 값의 배율
        /// HitCount : 효과의 적용 횟수
        /// TargetSource : 적용할 대상의 파라미터
        
        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;
        for (int i = 0; i < context.HitCount; i++)
        {
            // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
            float Stat = ResolveActionUseStatValue(flow, context, CastUnit); ;
            float FinalAmount = context.EffectValue;
            // 고정 계수 사용 안하면
            if (context.SkillSource != ESkillSource.E_FIXED)
            {
                // 피해량 계산
                FinalAmount *= Stat;

                // 치명타 적용
                FinalAmount = (1.0f + (context.IsCritical ? context.CriticalValueRate : 0.0f)) * FinalAmount;

                // 방어력 + 감쇄 효과
                FinalAmount = FinalAmount * (1 - Unit(targetPosition).DefendPoint.Now / (Unit(targetPosition).DefendPoint.Now + 1000));
            }

            // 피해량 적용
            Unit(targetPosition).HealthPoint.Now -= FinalAmount;


            float OverAmount = 0;

            if (Unit(targetPosition).HealthPoint.Now < 0)
            {
                OverAmount -= Unit(targetPosition).HealthPoint.Now;
                Unit(targetPosition).HealthPoint.Now = -0.00001f;
            }

            // 기록
            var Record = new ChangeHPResult()
            {
                Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = targetPosition },
                IsDamage = true,
                AttackStatusType = ESkillStatusType.E_NONE,
                Amount = FinalAmount,
                OverAmount = OverAmount,
            };
            flow.Record(Record);
        }
        AddStatusEffectToUnit(flow, context, targetPosition);
       
        // 사망 콜
        if (Unit(targetPosition).HealthPoint.Now < 0)
        {
            m_masterManager.UnitDying(Unit(targetPosition));
            return;
        }
    }

    public void HealToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        for (int i = 0; i < context.HitCount; i++)
        {
            // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
            float Stat = ResolveActionUseStatValue(flow, context, CastUnit);
            float FinalAmount = context.EffectValue;

            if (context.SkillSource != ESkillSource.E_FIXED)
            {
                // 회복량 계산
                FinalAmount = Stat * context.EffectValue;

                // 회복 증폭 효과 계산
                // 아직 회복 증폭 없어서 비워둠 쩔수
            }

            // 초과 회복 계산
            float OverHeal = Unit(targetPosition).HealthPoint.Now + FinalAmount - Unit(targetPosition).HealthPoint.Max;

            // 오버 힐 처리
            if (OverHeal > 0) FinalAmount -= OverHeal;

            Unit(targetPosition).HealthPoint.Now += FinalAmount;

            // 기록
            var Record = new ChangeHPResult()
            {
                Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = targetPosition },
                IsDamage = false,
                AttackStatusType = ESkillStatusType.E_NONE,
                Amount = FinalAmount,
                OverAmount = OverHeal
            };
            flow.Record(Record);
        }
    }

    public void ShieldToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;
        for (int i = 0; i < context.HitCount; i++)
        {
            float Value = ResolveActionUseStatValue(flow, context, CastUnit);
            float FinalAmount = context.EffectValue;

            if (context.SkillSource != ESkillSource.E_FIXED)
            {
                // 쉴드량 계산
                FinalAmount = Value * context.EffectValue;
                // 쉴드 증폭 효과 계산
                // 아직 회복 증폭 없어서 비워둠 쩔수
            }

            Unit(targetPosition).ShieldPoint.Now += FinalAmount;

            var Record = new AddShieldResult()
            {
                Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = targetPosition },
                Amount = FinalAmount
            };
            // 기록
            flow.Record(Record);
        }
    }

    public void VaritationStatToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        if (targetPosition >= m_units.Count || targetPosition < 0) Debug.LogError("타겟팅 실패 확인 바람" + targetPosition);
        if (m_isSystemAboutCharacter)
        {
            Debug.Log("캐릭터 시스템의 유닛에게 수치 변화 추가" + targetPosition);
        }
        else
        {
            Debug.Log("몬스터 시스템의 유닛에게 수치 변화 추가" + targetPosition);
        }

        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;
        for (int i = 0; i < context.HitCount; i++)
        {
            // 고정 계수 고려하여 최종 데미지 = EffectValue(사용할 계수) 로 시작
            float Stat = ResolveActionUseStatValue(flow, context, CastUnit); ;
            float FinalAmount = context.EffectValue;
            if (context.SkillSource != ESkillSource.E_FIXED)
            {
                // 증감 수치 계산
                FinalAmount *= Stat;

                // 치명타 적용
                // FinalAmount = (1.0f + (context.IsCritical ? context.CriticalValueRate : 0.0f)) * FinalAmount;
            }

            Unit(targetPosition).VaritationStat(flow, context.TargetSource, context.StatusDuration, FinalAmount);
        }
    }

    public void AddStatusEffectToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        if (context.StatusType == ESkillStatusType.E_NONE || context.StatusType == ESkillStatusType.E_NUM) return;
        if (targetPosition >= m_units.Count || targetPosition < 0) Debug.LogError("타겟팅 실패 확인 바람" + targetPosition);
        if(m_isSystemAboutCharacter)
        {
            Debug.Log("캐릭터 시스템의 유닛에게 상태이상 추가" + targetPosition);
        }
        else
        {
            Debug.Log("몬스터 시스템의 유닛에게 상태이상 추가" + targetPosition);
        }

        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        for (int i = 0; i < context.HitCount; i++)
        {
            float Value = ResolveActionUseStatValue(flow, context, CastUnit);

            Unit(targetPosition).AddStatusEffect(flow, context.StatusType, context.StatusDuration, context.TriggerConditionValue);
        }
    }

    public void TargetHasShockConditionalDamageToUnit(Flow flow, BattleContext context, int targetPosition)
    {
        Debug.Log("쇼크 있어서 조건부 대미지 로직 가동");
        if (m_units[targetPosition].HasShock() == 0) return;

        Unit CastUnit;
        AbilityFlowInput Input = (AbilityFlowInput)flow.Input;
        CastUnit = Input.CasterUnit;

        for (int i = 0; i < context.HitCount; i++)
        {
            if (context.EffectValue != 0)
            {
                float Value = ResolveActionUseStatValue(flow, context, CastUnit);

                float CriticalDamageValue = context.CriticalValueRate;

                // 피해량 계산
                float FinalAmount = Value * context.EffectValue;

                // 치명타 적용
                //FinalAmount = (1.0f + (context.IsCritical ? context.CriticalValueRate * CriticalDamageValue : 0.0f)) * FinalAmount;

                // 방어력 + 감쇄 효과
                FinalAmount = FinalAmount * (1 - Unit(targetPosition).DefendPoint.Now / (Unit(targetPosition).DefendPoint.Now + 1000));

                // 피해량 적용
                Unit(targetPosition).HealthPoint.Now -= FinalAmount;

                float OverAmount = 0;

                if (Unit(targetPosition).HealthPoint.Now < 0) OverAmount -= Unit(targetPosition).HealthPoint.Now;

                var Record = new ChangeHPResult()
                {
                    Target = new TargetPair() { isCharacter = m_isSystemAboutCharacter, position = targetPosition },
                    IsDamage = true,
                    AttackStatusType = ESkillStatusType.E_NONE,
                    Amount = FinalAmount
                };
                flow.Record(Record);
            }
            AddStatusEffectToUnit(flow, context, targetPosition);
        }
        // 사망 콜
        if (Unit(targetPosition).HealthPoint.Now < 0)
        {
            m_masterManager.UnitDying(Unit(targetPosition));
            return;
        }
    }

    public bool IsAlive(Unit unit)
    {
        return m_units.Contains(unit);
    }
}