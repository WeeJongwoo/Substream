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

    // 전투 관련
    public void ApplyDamage(Flow flow, BattleContext context)
    {
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() { 
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() { 
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.DamageToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.DamageToUnit(flow, context, target.position);
            }
        }

        
        // 종료 번들 실행
        Unit Caster = ((AbilityFlowInput)flow.Input).CasterUnit;
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

    public void ConditionalDamage(Flow flow, BattleContext context)
    {
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() { 
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() { 
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() { 
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        switch (context.SkillTrigger)
        {
            case ESkillTrigger.E_DEFAULT:
                break;
            case ESkillTrigger.E_CARD_USE:
                break;
            case ESkillTrigger.E_ON_TARGET_HAS_SHOCK:
                foreach (var target in context.TargetUnits)
                {
                    bool isCharacter = target.isCharacter;

                    if (isCharacter)
                    {
                        m_characterManager.TargetHasShockConditionalDamageToUnit(
                            flow, context, target.position
                            );
                    }
                    else
                    {
                        m_monsterManager.TargetHasShockConditionalDamageToUnit(
                            flow, context, target.position
                            );
                    }
                }
                break;
            case ESkillTrigger.E_WITH_FRONT:
                break;
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

    public void ApplyHeal(Flow flow, BattleContext context)
    {
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() { 
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() { 
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.HealToUnit(flow, context, target.position);
            }
            else
            {
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
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() {
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.ShieldToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.ShieldToUnit(flow, context, target.position);
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

    // 수치 변화
    public void ChangeVariation(Flow flow, BattleContext context, Unit CasterUnit)
    {
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() { 
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() {
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (context.TargetSource == EStatType.E_AETHER)
            {
                int CoverAether = 0;
                if (context.EffectValue == -1)
                    CoverAether = 
                        m_turnManager.CurrentTurnMaxEtherCount - m_turnManager.CurrentAetherCount;
                else CoverAether = (int)context.EffectValue;
                m_turnManager.UseAether(-CoverAether);
                flow.Record(new ChangeAetherResult());
                continue;
            }
            if (isCharacter)
            {
                m_characterManager.VaritationStatToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.VaritationStatToUnit(flow, context, target.position);
            }
        }
    }

    // 상태 관련
    public void AddStatusEffect(Flow flow, BattleContext context , Unit CasterUnit)
    {
        // 시전 기록
        switch (context.PresentationType)
        {
            case EPresentationType.E_DEFAULT:
                break;
            case EPresentationType.E_MAGICCAST:
                flow.Record(new CastResult()
                {
                    Caster = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_PHSICALATTACK:
                flow.Record(new AttackResult()
                {
                    Attacker = new TargetPair() {
                        isCharacter = ((AbilityFlowInput)(flow.Input)).CasterUnit.IsCharacter,
                        position = ((AbilityFlowInput)(flow.Input)).CasterUnit.Position },
                    Target = new TargetPair() {
                        isCharacter = context.TargetUnits.Count > 0 ? context.TargetUnits[0].isCharacter : true,
                        position = context.TargetUnits.Count > 0 ? context.TargetUnits[0].position : 0 },
                    IsCritical = context.IsCritical
                });
                break;
            case EPresentationType.E_INSTANT:
                break;
            case EPresentationType.E_PASSIVETRIGGER:
                break;
        }

        // 실제 처리
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.AddStatusEffectToUnit(flow, context, target.position);
            }
            else
            {
                m_monsterManager.AddStatusEffectToUnit(flow, context, target.position);
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

    // 카드 관련
    public void DrawCard(Flow flow, DrawCardActionContext context)
    { 
        m_cardManager.DrawCard(flow, context.Amount);

        flow.Record(new CardDrawResult()
        {
            Amount = context.Amount
        });
    }

    public void DrawCard(Flow flow, BattleContext context)
    {
        m_cardManager.DrawCard(flow, ((int)context.EffectValue));

        flow.Record(new CardDrawResult()
        {
            Amount = ((int)context.EffectValue)
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

    public void UnitDying(Flow flow, UnitDyingActionContext context)
    {
        TargetPair victim = context.Victim;

        if (victim.isCharacter)
        {
            var unit = m_characterManager.Unit(victim.position);
        }
        else
        {
            var unit = m_monsterManager.Unit(victim.position);
        }

        flow.Record(new UnitDyingResult()
        {
            Victim = victim
        });
    }

    public void ETC(Flow flow, BattleContext context)
    {

    }
}