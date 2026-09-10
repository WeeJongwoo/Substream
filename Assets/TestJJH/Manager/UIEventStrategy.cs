using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIEventStrategy
{
    protected UIFacade Facade;
    public abstract void Execute(ContextResult contextResult);
}

class UIDefaultEvent : UIEventStrategy
{
    public UIDefaultEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        Debug.Log("UI디폴트 이벤트 실행");
    }
}

class UIAttackEvent : UIEventStrategy
{ 
    public UIAttackEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result =  contextResult as AttackResult;

        if (result.Attacker.isCharacter) {
            Facade.CharacterUIManager.AttackEvent(result.Attacker.position,
                result.Target.isCharacter, result.Target.position);
        }
        else
        {
            Facade.MonsterUIManager.AttackEvent(result.Attacker.position,
                result.Target.isCharacter, result.Target.position);
        }
    }
}

class UICastEvent : UIEventStrategy
{
    public UICastEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as CastResult;

        if (result.Caster.isCharacter)
        {
            Facade.CharacterUIManager.CastEvent(result.Caster.position);
        }
        else
        {
            Facade.MonsterUIManager.CastEvent(result.Caster.position);
        }
    }
}

class UISkillEvent : UIEventStrategy
{
    public UISkillEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as CastResult;

        if (result.Caster.isCharacter)
        {
            Facade.CharacterUIManager.CastEvent(result.Caster.position);
        }
        else
        {
            Facade.MonsterUIManager.CastEvent(result.Caster.position);
        }
    }
}

class UIChangeHPEvent : UIEventStrategy
{
    public UIChangeHPEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as ChangeHPResult;

        if (result.Target.isCharacter)
        {
            Facade.CharacterUIManager.ChangeHPEvent(result.Target.position, result.IsDamage, result.AttackStatusType, (int)result.Amount);
        }
        else
        {
            Facade.MonsterUIManager.ChangeHPEvent(result.Target.position, result.IsDamage, result.AttackStatusType, (int)result.Amount);
        }
    }
}

class UIAddShieldEvent : UIEventStrategy
{

    public UIAddShieldEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as AddShieldResult;
        if (result.Target.isCharacter)
        {
            Facade.CharacterUIManager.ShieldEvent(result.Target.position, (int)result.Amount);
        }
        else
        {
            Facade.MonsterUIManager.ShieldEvent(result.Target.position, (int)result.Amount);
        }
    }
}

class UIChangeStackEvent : UIEventStrategy
{

    public UIChangeStackEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as ChangeStackResult;
        
        if (result.Target.isCharacter)
        {
            Facade.CharacterUIManager.ChangeStack(result.Target.position, result.StatusType, result.TurnDuration, result.Stack, result.IsNew);
        }
        else
        {
            Facade.MonsterUIManager.ChangeStack(result.Target.position, result.StatusType, result.TurnDuration, result.Stack, result.IsNew);
        }
    }
}

class UIChangeAetherEvent : UIEventStrategy
{

    public UIChangeAetherEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as ChangeAetherResult;

        Facade.TurnUIManager.SetRoundAetherInfo();
    }
}











class UIDrawCardEvent : UIEventStrategy
{
    public UIDrawCardEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as CardDrawResult;

        //Facade.CardUIManager.DrawNewHandCard();
    }
}

class UIUseCardEvent : UIEventStrategy
{
    public UIUseCardEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as CardUseResult;

        ///Facade.CardUIManager.DrawCard();
        ///

    }
}

class UIUnitDeathEvent : UIEventStrategy
{
    public UIUnitDeathEvent(UIFacade facade)
    {
        Facade = facade;
    }

    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as UnitDyingResult;

        Facade.MasterManager.ApplyUIUnitDying(result.Victim);
    }
}

class UIEndTurnEvent : UIEventStrategy
{
    public UIEndTurnEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        Facade.MasterManager.ApplyUISetTurn();
    }
}

class UIEndRoundEvent : UIEventStrategy
{
    public UIEndRoundEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        Facade.MasterManager.ApplyUISetRound();
    }
}

class UIPausaeEvent : UIEventStrategy
{
    public UIPausaeEvent(UIFacade facade)
    {
        Facade = facade;
    }
    public override void Execute(ContextResult contextResult)
    {
        var result = contextResult as PauseResult;
    }
}

