using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIEvent
{
    protected UIFacade Facade;
    public abstract void Execute();
}

class UIDamageEvent : UIEvent
{
    public bool SourceUnitIsCharacter;
    public int SourceUnitPos;

    public bool TargetUnitIsCharacter;
    public int TargetUnitPos;

    public int Damage;

    public UIDamageEvent(UIFacade facade, bool sourceUnitIsCharacter, int sourceUnitpos,
        bool targetunitIsCharacter, int targetUnitPos,
        int damage)
    {
        Facade = facade;

        SourceUnitIsCharacter = sourceUnitIsCharacter;
        SourceUnitPos = sourceUnitpos;
        TargetUnitIsCharacter = targetunitIsCharacter;
        TargetUnitPos = targetUnitPos;
        Damage = damage;
    }

    public override void Execute()
    {
        Facade.CharacterUIManager.DamageEvent(SourceUnitIsCharacter, SourceUnitPos, TargetUnitIsCharacter, TargetUnitPos, Damage);
        Facade.MonsterUIManager.DamageEvent(SourceUnitIsCharacter, SourceUnitPos, TargetUnitIsCharacter, TargetUnitPos, Damage);
    }
}

class UIHealEvent : UIEvent
{
    public bool SourceUnitIsCharacter;
    public int SourceUnitPos;

    public bool TargetUnitIsCharacter;
    public int TargetUnitPos;

    public int Damage;

    public UIHealEvent(UIFacade facade, bool sourceUnitIsCharacter, int sourceUnitpos,
        bool targetunitIsCharacter, int targetUnitPos,
        int damage)
    {
        Facade = facade;

        SourceUnitIsCharacter = sourceUnitIsCharacter;
        SourceUnitPos = sourceUnitpos;
        TargetUnitIsCharacter = targetunitIsCharacter;
        TargetUnitPos = targetUnitPos;
        Damage = damage;
    }
    public override void Execute()
    {
        Facade.CharacterUIManager.HealEvent(SourceUnitIsCharacter, SourceUnitPos, TargetUnitIsCharacter, TargetUnitPos, Damage);
        Facade.MonsterUIManager.HealEvent(SourceUnitIsCharacter, SourceUnitPos, TargetUnitIsCharacter, TargetUnitPos, Damage);
    }
}

class UIUnitDeathEvent : UIEvent
{
    public bool DeathUnitIsCharacter;
    public int DeathUnitPos;

    public Unit DeathUnit;

    public UIUnitDeathEvent(UIFacade facade, Unit unit)
    {
        Facade = facade;

        DeathUnit = unit;
    }

    public override void Execute()
    {
        Facade.MasterManager.ApplyUIUnitDying(DeathUnit);
    }
}

class UIDrawCardEvent : UIEvent
{
    public int Count;

    public UIDrawCardEvent(UIFacade facade, int count)
    {
        Facade = facade;
        Count = count;
    }

    public override void Execute()
    {
        Facade.CardUIManager.DrawCard();
    }
}

class UIEndTurnEvent : UIEvent
{
    MasterManager master;
    public UIEndTurnEvent(UIFacade facade, MasterManager master)
    {
        Facade = facade;
        this.master = master;
    }
    public override void Execute()
    {
        master.ApplyUISetTurn();
    }
}
