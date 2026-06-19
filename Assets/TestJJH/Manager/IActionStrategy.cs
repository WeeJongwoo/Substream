using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionStrategy 
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade);
}

public class DamageSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyDamage(flow, (BattleContext)context);
        return true;
    }
}

public class ConditionalDamageStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ConditionalDamage(flow, (BattleContext)context);
        return true;
    }
}

public class HealingSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyHeal(flow, (BattleContext)context);
        return true;
    }
}
public class ShieldSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyShield(flow, (BattleContext)context);
        return true;
    }
}

public class VariationSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade )
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ChangeVariation(flow, (BattleContext)context, input.CasterUnit);
        return true;
    }
}

public class StatusEffectSkillStrategy : IActionStrategy
{ 
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.AddStatusEffect(flow, (BattleContext)context, input.CasterUnit);
        return true;
    }
}

public class ETCStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ETC(flow, (BattleContext)context);
        return true;
    }
}








public class TurnEndStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {   battleFacade.TurnEnd(flow, (TurnEndActionContext)context);
        return true;
    }
}




public class DrawSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.DrawCard(flow, (BattleContext)context);
        return true;
    }
}


public class UnitDyingStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        UnitDyingFlowInput input = (UnitDyingFlowInput)flow.Input;
        battleFacade.UnitDying(flow, (UnitDyingActionContext)context);
        return true;
    }
}



/*========================================================*/

public class DrawSystemStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade)
    {
        SystemDrawCardFlowInput input = (SystemDrawCardFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.DrawCard(flow, (DrawCardActionContext)context);
        return true;
    }
}