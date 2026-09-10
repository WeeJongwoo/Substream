using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BattleFacade : IManagerFacade
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
    public void ApplyDamage(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        foreach(var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.DamageToUnit(flow, context, battleFacade, uiFacade);//target.isCharacter, target.position, context.Value, context.IsCritical, context.CriticalValueRate);
            }
            else
            {
                m_monsterManager.DamageToUnit(flow, context, battleFacade, uiFacade);
            }
        }
    }

    public void ApplyHeal(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.HealToUnit(flow, context, battleFacade, uiFacade);//context, target.isCharacter, target.position, context.Value, context.IsCritical, context.CriticalValueRate);
            }
            else
            {
                m_monsterManager.HealToUnit(flow, context, battleFacade, uiFacade);
            }
        }
    }

    public void ApplyShield(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.HealToUnit(flow, context, battleFacade, uiFacade);
            }
            else
            {
                m_monsterManager.HealToUnit(flow, context, battleFacade, uiFacade);
            }
        }
    }

    // 상태 관련
    public void AddStatusEffect(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                m_characterManager.AddStatusEffect(flow, context, battleFacade, uiFacade);
            }
            else
            {
                m_monsterManager.AddStatusEffect(flow, context, battleFacade, uiFacade);
            }
        }
    }

    // 카드 관련
    public void DrawCard(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        m_cardManager.DrawCard(context, (int)context.Value);
    }

    public void DiscardCard(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {

    }

    // 턴 관련
    public void TurnEnd(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        m_masterManager.ApplySetTurn();
        uiFacade.EndTurn();
    }

    public void UnitDying(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        
        foreach (var target in context.TargetUnits)
        {
            bool isCharacter = target.isCharacter;

            if (isCharacter)
            {
                var unit = m_characterManager.Unit(target.position);
                m_masterManager.ApplyUnitDying(unit);
                uiFacade.UnitDead(unit);
            }
            else
            {
                var unit = m_monsterManager.Unit(target.position);
                m_masterManager.ApplyUnitDying(unit);
                uiFacade.UnitDead(unit);
            }
        }
    }

    public void ConditionalDamage(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        switch (context.SkillTrigger)
        {
            case ESkillTrigger.E_DEFAULT:
                break;
            case ESkillTrigger.E_CARD_USE:
                break;
            case ESkillTrigger.E_ON_TARGET_HAS_SHOCK:
                break;
            case ESkillTrigger.E_WITH_FRONT:
                break;
        }
    }

    public void ETC(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {

    }
}