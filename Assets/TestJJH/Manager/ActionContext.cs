using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class ActionContext
{
    public bool IsDone;

    public ActionContext()
    {
        IsDone = false;
    }
}

public class BattleContext : ActionContext
{
    public EPresentationType PresentationType;

    public ESkillType SkillType;
    public ESkillTrigger SkillTrigger;
    public int TriggerConditionValue;

    public ESkillSource SkillSource;
    public ESkillStatusType StatusType;
    public int StatusDuration;

    public bool IsCritical;
    public float CriticalValueRate;
    public float EffectValue;
    public int HitCount;
    public EStatType TargetSource;

    public List<TargetPair> TargetUnits;

    public SkillTableData SkillData;

    public BattleContext() : base()
    {
        TargetUnits = new List<TargetPair>();
    }
}

public class TurnEndActionContext : ActionContext
{
    public TurnEndActionContext() : base()
    {
    }
}

public class UnitDyingActionContext : ActionContext
{
    public TargetPair Victim;
    public UnitDyingActionContext() : base()
    {

    }
}

public class DrawCardActionContext : ActionContext
{
    public int Amount;
    public DrawCardActionContext() : base()
    {
    }
}