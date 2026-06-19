using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

class ContextWriterFactory
{
    private Dictionary<Type, IContextWriter> _writers;

    public ContextWriterFactory()
    {
        _writers = new Dictionary<Type, IContextWriter>
        {
            { typeof(BattleContext), new BattleContextWriter() },
            { typeof(TurnEndActionContext), new TurnEndContextWriter() },
            { typeof(UnitDyingActionContext), new UnitDyingContextWriter() },
            { typeof(DrawCardActionContext), new DrawCardContextWriter() },
        };
    }

    public void Write(Flow flow, ActionContext context)
    {
        _writers[context.GetType()].WriteContext(flow, context);
    }
}

interface IContextWriter
{
    void WriteContext(Flow flow, ActionContext context);
}

class BattleContextWriter : IContextWriter
{
    public void WriteContext(Flow flow, ActionContext context)
    {
        var ctx = context as BattleContext;
#if UNITY_EDITOR
        Debug.Log("###########스킬 컨텍스트 작성 시작###########");
#endif
        float CriticalTriggerRate = 0;
        CriticalTriggerRate = ((AbilityFlowInput)(flow.Input)).CasterUnit.CriticalTriggerRate.Now;
        ctx.PresentationType = ctx.SkillData.PresentationType;
        ctx.HitCount = ctx.SkillData.HitCount;
        ctx.SkillType = ctx.SkillData.SkillType;
        ctx.SkillTrigger = ctx.SkillData.Trigger;
        ctx.TriggerConditionValue = ctx.SkillData.TriggerConditionValue;
        ctx.SkillSource = ctx.SkillData.SkillSource;
        ctx.StatusType = ctx.SkillData.StatusType;
        ctx.IsCritical = (UnityEngine.Random.Range(0, 101) < (CriticalTriggerRate * 100));
        ctx.EffectValue = ctx.SkillData.EffectValue;
        ctx.TargetSource = ctx.SkillData.TargetSource;
        ctx.StatusDuration = ctx.SkillData.StatusDuration;
    }
}

class TurnEndContextWriter : IContextWriter
{
    public void WriteContext(Flow flow, ActionContext context)
    {
        // 턴 종료 시 필요한 값 세팅
        var ctx = context as TurnEndActionContext;

    }
}

class UnitDyingContextWriter : IContextWriter
{
    public void WriteContext(Flow flow, ActionContext context)
    {
        var ctx = context as UnitDyingActionContext;

        var victim = ((UnitDyingFlowInput)(flow.Input)).Victim;

        // Victim 기반 후처리 세팅
        ctx.Victim.isCharacter = victim.IsCharacter;
        ctx.Victim.position = victim.Position;
    }
}

class DrawCardContextWriter : IContextWriter
{
    public void WriteContext(Flow flow, ActionContext context)
    {
        var ctx = context as DrawCardActionContext;

        var CasterUnit = ((SystemDrawCardFlowInput)(flow.Input)).CasterUnit;

        // Caster 드로우시 특수능력 후처리 세팅
        // 현재는 없음
    }
}