using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BattleFacade
{
    private readonly MasterManager m_masterManager;
    private readonly CharacterManager m_characterManager;
    private readonly MonsterManager m_monsterManager;
    private readonly CardManager m_cardManager;
    private readonly TurnManager m_turnManager;

    public BattleFacade(MasterManager masterManager,
        CharacterManager characterManager,
        MonsterManager monsterManager,
        CardManager cardManager,
        TurnManager turnManager)
    {
        m_characterManager = characterManager;
        m_monsterManager = monsterManager;
        m_cardManager = cardManager;
        m_turnManager = turnManager;
        m_masterManager = masterManager;
    }

    public Unit GetCurrentUnit()
    {
        return m_turnManager.CurrentTurnUnit;
    }

    public bool IsAlive(Unit unit)
    {
        if (unit.IsCharacter)
        {
            return m_characterManager.IsAlive(unit);
        }
        else
        {
            return m_monsterManager.IsAlive(unit);
        }
    }

    private bool CheckTrigger(Flow flow, BattleContext context)
    {
        switch (context.Trigger)
        {
            case ESkillTrigger.E_DEFAULT:
            case ESkillTrigger.E_CARD_USE:
            case ESkillTrigger.E_WITH_FRONT:
                return true;
            case ESkillTrigger.E_HAS_SHOCK:
                switch (context.TriggerTargetType)
                {
                    case ETargetType.E_NONE:
                        break;
                    case ETargetType.E_ALLIES:
                        break;
                    case ETargetType.E_ENEMY:
                        break;
                    case ETargetType.E_ADJACENT:
                    case ETargetType.E_CHAINBEHIND:
                        break;
                    case ETargetType.E_SELECT:
                        int SelectPos = (flow.Input as CardAbilityFlowInput).SelectTargetPosition;
                        if(SelectPos > 0)
                        {
                            if(m_characterManager.Unit(SelectPos).GetNumericStatusEffect(EStatusEffectType.E_SHOCK) > context.TriggerConditionValue) 
                                return true;
                        }
                        else
                        {
                            if (m_monsterManager.Unit(SelectPos * -1).GetNumericStatusEffect(EStatusEffectType.E_SHOCK) > context.TriggerConditionValue)
                                return true;
                        }
                        break;
                    case ETargetType.E_SELF:
                        int SelfPos = (flow.Input as CardAbilityFlowInput).CasterUnit.Position;
                        if (SelfPos > 0)
                        {
                            if (m_characterManager.Unit(SelfPos).GetNumericStatusEffect(EStatusEffectType.E_SHOCK) > context.TriggerConditionValue)
                                return true;
                        }
                        else
                        {
                            if (m_monsterManager.Unit(SelfPos * -1).GetNumericStatusEffect(EStatusEffectType.E_SHOCK) > context.TriggerConditionValue)
                                return true;
                        }
                        break;
                }
                break;
            case ESkillTrigger.E_HAS_OVERROAD:
                switch (context.TriggerTargetType)
                {
                    case ETargetType.E_NONE:
                    case ETargetType.E_ALLIES:
                    case ETargetType.E_ENEMY:
                        break;
                    case ETargetType.E_ADJACENT:
                    case ETargetType.E_CHAINBEHIND:
                        break;
                    case ETargetType.E_SELECT:
                        int SelectPos = (flow.Input as CardAbilityFlowInput).SelectTargetPosition;
                        if (SelectPos > 0)
                        {
                            if (m_characterManager.Unit(SelectPos).GetNumericStatusEffect(EStatusEffectType.E_OVERLOAD) > context.TriggerConditionValue)
                                return true;
                        }
                        else
                        {
                            if (m_monsterManager.Unit(SelectPos * -1).GetNumericStatusEffect(EStatusEffectType.E_OVERLOAD) > context.TriggerConditionValue)
                                return true;
                        }
                        break;
                    case ETargetType.E_SELF:
                        int SelfPos = (flow.Input as CardAbilityFlowInput).CasterUnit.Position;
                        if (SelfPos > 0)
                        {
                            if (m_characterManager.Unit(SelfPos).GetNumericStatusEffect(EStatusEffectType.E_OVERLOAD) > context.TriggerConditionValue)
                                return true;
                        }
                        else
                        {
                            if (m_monsterManager.Unit(SelfPos * -1).GetNumericStatusEffect(EStatusEffectType.E_OVERLOAD) > context.TriggerConditionValue)
                                return true;
                        }
                        break;
                }
                break;
        }
        return false;
    }

    private void RecordPresentation(Flow flow, BattleContext context, Unit CasterUnit)
    {
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair()
                    {
                        isCharacter = CasterUnit.IsCharacter,
                        position = CasterUnit.Position
                    },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair()
                    {
                        isCharacter = CasterUnit.IsCharacter,
                        position = CasterUnit.Position
                    },
                    Target = new TargetPair()
                    {
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0
                    },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }
    }

    // 전투 관련
    public void ApplyDamage(Flow flow, BattleContext context)
    {
        RecordPresentation(flow, context,((CardAbilityFlowInput)(flow.Input)).CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.DamageToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.DamageToUnit(flow, context, target.position);
            }
        }

        
        // 종료 번들 실행
        Unit Caster = ((CardAbilityFlowInput)flow.Input).CasterUnit;
        /*
        if (Caster.IsCharacter)
        {
            m_characterManager.DamageEventBundle(flow, context, Caster.Position);
        }
        else
        {
            m_monsterManager.DamageEventBundle(flow, context, Caster.Position);
        }*/
    }

    public void ApplyBounce(Flow flow, BattleContext context)
    {
        RecordPresentation(flow, context, ((CardAbilityFlowInput)(flow.Input)).CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.BounceToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.BounceToUnit(flow, context, target.position);
            }
        }

        /*
        if (Caster.IsCharacter)
        {
            m_characterManager.DamageEventBundle(flow, context, Caster.Position);
        }
        else
        {
            m_monsterManager.DamageEventBundle(flow, context, Caster.Position);
        }*/
    }

    public void ApplyHeal(Flow flow, BattleContext context)
    {
        RecordPresentation(flow, context, ((CardAbilityFlowInput)(flow.Input)).CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.DamageToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.DamageToUnit(flow, context, target.position);
            }
        }

        /*
        if (Caster.IsCharacter)
        {
            m_characterManager.DamageEventBundle(flow, context, Caster.Position);
        }
        else
        {
            m_monsterManager.DamageEventBundle(flow, context, Caster.Position);
        }*/

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.HealToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.HealToUnit(flow, context, target.position);
            }
        }

        /*
        // 종료 번들 실행
        Unit Caster = ((AbilityFlowInput)flow.Input).CasterUnit;

        if (Caster.IsCharacter)
        {
            m_characterManager.DamageEventBundle(flow, context, Caster.Position);
        }
        else
        {
            m_monsterManager.DamageEventBundle(flow, context, Caster.Position);
        }*/
    }

    public void ApplyShield(Flow flow, BattleContext context )
    {
        RecordPresentation(flow, context, ((CardAbilityFlowInput)(flow.Input)).CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.ShieldToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.ShieldToUnit(flow, context, target.position);
            }
        }

        /*
        // 종료 번들 실행
        if (Caster.IsCharacter)
        {
            m_characterManager.DamageEventBundle(flow, context, Caster.Position);
        }
        else
        {
            m_monsterManager.DamageEventBundle(flow, context, Caster.Position);
        }*/
    }

    
    // 수치 변화
    public void ApplyChangeVariation(Flow flow, BattleContext context, Unit CasterUnit)
    {
        RecordPresentation(flow, context, CasterUnit);  

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        switch (context.TargetStatSource)
        {
            // 타겟 없어도 작동해야됨 
            case EStatSource.E_MAXAETHER:
                m_turnManager.ExpendMaxAetherCount((int)context.EffectValue);
                flow.Record(new ChangeAetherResult());
                return;
            case EStatSource.E_AETHER:
                int CoverAether = 0;
                if (context.EffectValue >= 99)
                    CoverAether =
                        m_turnManager.CurrentTurnMaxEtherCount - m_turnManager.CurrentAetherCount;
                else CoverAether = (int)context.EffectValue;
                m_turnManager.UseAether(-CoverAether);
                flow.Record(new ChangeAetherResult());
                return;
        }

        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.NumericStatusEffecttToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.NumericStatusEffecttToUnit(flow, context, target.position);
            }
        }
    }

    // 단일 상태이상 관련만
    public void ApplyStatusEffect(Flow flow, BattleContext context , Unit CasterUnit)
    {
        RecordPresentation(flow, context, CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.CheckTargetAlive(target.position);
                m_characterManager.AddStatusEffectToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.CheckTargetAlive(target.position);
                m_monsterManager.AddStatusEffectToUnit(flow, context, target.position);
            }
        }
    }

    // 카드 관련
    public void DrawCard(Flow flow, CardAbilityFlowInput input, BattleContext context)
    {
        RecordPresentation(flow, context, input.CasterUnit);

        if (!CheckTrigger(flow, context))
        {
            return;
        }

        m_cardManager.DrawCard(input.CasterUnit, (int)context.EffectValue);
        m_cardManager.DrawNewHandCard();
        m_cardManager.AddTemtQueueCardsToHand();
        flow.Record(new CardDrawResult()
        {
            Amount = (int)context.EffectValue
        });
    }

    public void DrawCard(Flow flow, SystemDrawCardFlowInput input, DrawCardActionContext context)
    {
        m_cardManager.DrawCard(input.CasterUnit, ((int)context.Amount));
        m_cardManager.DrawNewHandCard();
        m_cardManager.AddTemtQueueCardsToHand();
        flow.Record(new CardDrawResult()
        {
            Amount = ((int)context.Amount)
        });
    }

    public void DiscardCard(Flow flow, BattleContext context)
    {

    }

    // 턴 관련
    public void TurnEnd(Flow flow, TurnEndActionContext context )
    {
        m_characterManager.SetTurn(flow);
        m_monsterManager.SetTurn(flow);

        m_masterManager.ApplySetTurn();

        flow.Record(new TurnEndResult()
        { });
    }

    public void RoundEnd(Flow flow, RoundEndActionContext context)
    {
        m_masterManager.ApplySetRound();

        flow.Record(new RoundEndResult()
        { });
    }

    public void UnitDying(Flow flow, UnitDyingActionContext context)
    {
        TargetPair victim = context.Victim;

        if (victim.isCharacter)
        {
            m_characterManager.Unit(victim.position).UnitDead(flow);
            m_masterManager.UnitDying(m_characterManager.Unit(victim.position));
        }
        else
        {
            m_monsterManager.Unit(victim.position).UnitDead(flow);
            m_masterManager.UnitDying(m_monsterManager.Unit(victim.position));
        }

        flow.Record(new UnitDyingResult()
        {
            Victim = victim
        });
    }

    public void ETC(Flow flow, BattleContext context)
    {
        if (!CheckTrigger(flow, context))
        {
            return;
        }
    }
}