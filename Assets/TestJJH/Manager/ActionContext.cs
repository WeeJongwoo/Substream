using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public static class ContextIdGenerator
{
    private static int _id = 0;

    /// <summary>
    /// 전역 고유 ID를 발행한다. (Thread-Safe)
    /// </summary>
    public static int Next()
    {
        return Interlocked.Increment(ref _id);
    }

    /// <summary>
    /// 필요 시 초기화 (테스트/리플레이 용)
    /// </summary>
    public static void Reset(int start = 0)
    {
        Interlocked.Exchange(ref _id, start);
    }
}

public abstract class ActionContext
{
    public bool IsDone;

    public readonly int ID;

    public ActionContext()
    {
        IsDone = false;
        ID = ContextIdGenerator.Next();
    }
}

public class BattleContext : ActionContext
{
    public int SkillID;
    public ESkillType SkillType;
    public EPresentationType PresentationType;

    public ESkillTrigger Trigger;
    public ETargetType TriggerTargetType;
    public int TriggerConditionValue;

    public EStatSource SkillSource;
    public ETargetType SkillSourceTargetType;
    public float EffectValue;
    public float UpgradeValue;
    public bool IsFixed;
    public int HitCount;

    public EScaleType ScaleType;
    public ETargetType ScaleTypeTargetType;
    public float ScaleFactor;
    public int ScaleLimit;

    public EStatusEffectType StatusType;
    public int StatusCount;
    public int RoundDuration;
    public int StatusDuration;

    public bool RefreshTarget;
    public ETargetType TargetType;
    public int TargetCount;
    public EStatSource TargetStatSource;


    public bool IsCritical;

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

public class RoundEndActionContext : ActionContext
{
    public RoundEndActionContext() : base()
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