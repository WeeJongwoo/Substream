using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionStrategy 
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade);
}

public class DamageSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyDamage(context);
        return true;
    }
}

public class ConditionalDamageStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ConditionalDamage(context);
        return true;
    }
}

public class HealingSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyHeal(context);
        return true;
    }
}

public class IncreaseSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.AddStatusEffect(context);
        return true;
    }
}

public class ShieldSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ApplyShield(context);
        return true;
    }
}

public class DebuffSkillStrategy : IActionStrategy
{ 
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.AddStatusEffect(context);
        return true;
    }
}

public class ETCStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.ETC(context);
        return true;
    }
}








public class TurnEndStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {   battleFacade.TurnEnd(context);
        return true;
    }
}


public class DrawSkillStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        AbilityFlowInput input = (AbilityFlowInput)flow.Input;
        if (!battleFacade.IsAlive(input.CasterUnit))
        {
            return false;
        }
        battleFacade.DrawCard(context);
        return true;
    }
}


public class UnitDyingStrategy : IActionStrategy
{
    public bool Execute(Flow flow, ActionContext context, BattleFacade battleFacade, UIFacade uiFacade)
    {
        UnitDyingFlowInput input = (UnitDyingFlowInput)flow.Input;
        battleFacade.UnitDying(context);
        return true;
    }
}