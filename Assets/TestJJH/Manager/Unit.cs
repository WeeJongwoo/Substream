using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using static UnityEditor.Timeline.TimelinePlaybackControls;


[System.Serializable]
public class StatusEffect
{
    public ESkillStatusType Effect;
    public int Duration;
    public float Value;

    public StatusEffect(ESkillStatusType effect, int duration, float value)
    {
        Effect = effect;
        Duration = duration;
        Value = value;
    }
}

public class StatusEffectManager
{
    public Dictionary<ESkillStatusType, List<StatusEffect>> m_statusEffect = new Dictionary<ESkillStatusType, List<StatusEffect>>();

    public StatusEffectManager()
    {
        m_statusEffect.Add(ESkillStatusType.E_BLEED, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.E_SHOCK, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.E_OVERLOAD, new List<StatusEffect>());

        m_statusEffect.Add(ESkillStatusType.M_ATK, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.M_DEF, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.M_SPEED, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.M_CRITICALTRIGGERRATE, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.M_CRITICALVALUERATE, new List<StatusEffect>());

        m_statusEffect.Add(ESkillStatusType.P_ATK, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.P_DEF, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.P_SPEED, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.P_CRITICALTRIGGERRATE, new List<StatusEffect>());
        m_statusEffect.Add(ESkillStatusType.P_CRITICALVALUERATE, new List<StatusEffect>());
    }

    public void AddStatusEffect(StatusEffect statusEffect, Flow flow, bool isCharacter, int position)
    {
        var Record = new ChangeStackResult()
        {
            Target = new TargetPair() { isCharacter = isCharacter, position = position },
            StatusType = statusEffect.Effect,
            Duration = statusEffect.Duration,
            Stack = ((int)statusEffect.Value),
            IsNew = false
        };
        if (m_statusEffect[statusEffect.Effect].Count == 0)
        {
            Record.IsNew = true;
        }
        m_statusEffect[statusEffect.Effect].Add(statusEffect);
        flow.Record(Record);
    }

    public int HasShock()
    {
        if (m_statusEffect[ESkillStatusType.E_SHOCK].Count != 0)
        {
            return m_statusEffect[ESkillStatusType.E_SHOCK].Sum(r => 1);
        }
        return 0;
    }

    public int HasOverload()
    {
        if (m_statusEffect[ESkillStatusType.E_OVERLOAD].Count != 0)
        {
            return m_statusEffect[ESkillStatusType.E_OVERLOAD].Sum(r => 1);
        }
        return 0;
    }

    public int HasBleed()
    {
        if (m_statusEffect[ESkillStatusType.E_BLEED].Count != 0)
        {
            return m_statusEffect[ESkillStatusType.E_BLEED].Sum(r => 1);
        }
        return 0;
    }

    public void SetTurn(Flow flow, bool isCharacter, int position)
    {
        // 전체 상태이상 순회
        foreach(var l in m_statusEffect)
        {
            if (l.Value.Count == 0)
            {
                continue;
            }
            // 전체 지속시간 감소
            // 지속시간 0인 상태이상 삭제

            for (int i = 0; i < l.Value.Count; i++)
            {
                l.Value[i].Duration--;
            }

            l.Value.RemoveAll(se => se.Duration == 0);

            for (int i = 0; i < l.Value.Count; i++)
            {
                if (l.Value[i].Duration < 0)
                    l.Value[i].Duration = -1;
            }

            var Record = new ChangeStackResult()
            {
                Target = new TargetPair() { isCharacter = isCharacter, position = position },
                StatusType = l.Key,
                Duration = l.Value.Sum(r => r.Duration),
                Stack = ((int)l.Value.Sum(r => r.Value)),
                IsNew = false
            };
            flow.Record(Record);
        }
    }
}

public struct IStatModifier
{
    public int Duration;
    public float Amount;
}


[System.Serializable]
public class Stat
{
    [SerializeField]
    public readonly float Base = 0;
    [SerializeField]
    public float Now = 0;
    public float Max
    {
        get
        {
            float value = Base;

            foreach (var mod in modifiers)
                value += mod.Amount;

            return value;
        }
    }
    private LinkedList<IStatModifier> modifiers = new LinkedList<IStatModifier>();

    public Stat(float Base)
    {
        this.Base = Base;
        this.Now = Base;
    }

    public void AddModifie(int duration, float amount)
    {
        IStatModifier newModifie;
        newModifie.Duration = duration;
        newModifie.Amount = amount;
        modifiers.AddLast(newModifie);
        Now += amount;
    }

    public void SetModifie(IStatModifier modifier)
    {
        Now -= modifier.Amount;
    }

    public void SetTurn()
    {
        var toRemove = new List<IStatModifier>();
        foreach (var modifierIt in modifiers)
        {
            var modifierDuration = modifierIt.Duration;
            modifierDuration--;
            if (modifierIt.Duration <= 0)
            {
                SetModifie(modifierIt);
                toRemove.Add(modifierIt);
            }
        }

        foreach (var node in toRemove)
        {
            modifiers.Remove(node);
        }
    }
}

[System.Serializable]
public abstract class Unit
{
    private UnitManagingSystem m_system;
    private bool m_isCharacter = true;
    public GameObject thisObject;
    public bool isDead = false;
    private int m_position;
    public int Position
    {
        get { return m_position; }
    }
    public abstract int IngameUnitID();

    public bool IsCharacter
    {
        get { return m_isCharacter; }
    }

    public Stat HealthPoint => m_stats[EStatType.E_HP];
    public Stat AttackPoint => m_stats[EStatType.E_ATK];
    public Stat DefendPoint => m_stats[EStatType.E_DEF];
    public Stat SpeedPoint => m_stats[EStatType.E_SPEED];
    public Stat CriticalTriggerRate => m_stats[EStatType.E_CRITICALTRIGGERRATE];
    public Stat CriticalValueRate => m_stats[EStatType.E_CRITICALVALUERATE];
    public Stat ShieldPoint => m_stats[EStatType.E_SHIELD];

    [SerializeField]
    public Stat DebugHP;
    [SerializeField]
    private UnitSlot m_ui;

    // 전투 관련 기본 스탯
    [SerializeField]
    private Dictionary<EStatType, Stat> m_stats = new Dictionary<EStatType, Stat>();

    // 상태이상 관련 (출혈, 감전 등)
    private StatusEffectManager m_statusEffect = new StatusEffectManager();
    public int HasShock()
    {
        return m_statusEffect.HasShock();
    }

    public int HasOverload()
    {
        return m_statusEffect.HasOverload();
    }
    public int HasBleed()
    {
        return m_statusEffect.HasBleed();
    }

    public void Init(UnitManagingSystem system, bool isCharacter, int pos, float hp, float atk, float def, float speed, float CriticalTriggerRate, float CriticalValueRate)
    {
        m_system = system;
        m_isCharacter = isCharacter;
        m_position = pos;

        m_stats.Add(EStatType.E_HP, new Stat(hp));
        m_stats.Add(EStatType.E_ATK, new Stat(atk));
        m_stats.Add(EStatType.E_DEF, new Stat(def));
        m_stats.Add(EStatType.E_SPEED, new Stat(speed));
        m_stats.Add(EStatType.E_CRITICALTRIGGERRATE, new Stat(CriticalTriggerRate));
        m_stats.Add(EStatType.E_CRITICALVALUERATE, new Stat(CriticalValueRate));
        m_stats.Add(EStatType.E_SHIELD, new Stat(0));

        DebugHP = m_stats[EStatType.E_HP];
    }

    public void SetTurn(Flow flow)
    {
        // 상태이상 자체 효과
        m_system.StatusEffectExecuteStrategy[ESkillStatusType.E_BLEED].Execute(this);
        m_system.StatusEffectExecuteStrategy[ESkillStatusType.E_SHOCK].Execute(this);
        m_system.StatusEffectExecuteStrategy[ESkillStatusType.E_OVERLOAD].Execute(this);

        m_statusEffect.SetTurn(flow, this.IsCharacter, this.Position);

        foreach (var s in m_stats)
        {
            s.Value.SetTurn();
        }
    }

    public void AddStatusEffect(Flow flow, ESkillStatusType effect, int duration, float value)
    {
        m_statusEffect.AddStatusEffect( new StatusEffect(effect, duration, value), flow, this.IsCharacter, this.Position);
    }

    public void VaritationStat(Flow flow, EStatType statType, int duration, float value)
    {
        Debug.Log(statType + " : add" + value);
        m_stats[statType].AddModifie(duration, value);

        ESkillStatusType statusType = ESkillStatusType.E_NONE;
        switch(statType)
        {
            case EStatType.E_NONE:
            case EStatType.E_HP:
            case EStatType.E_SHIELD:
            case EStatType.E_AETHER:
            case EStatType.E_DECK:
                statusType = ESkillStatusType.E_NONE;
                break;
            case EStatType.E_ATK:
                if(value > 0) statusType = ESkillStatusType.P_ATK;
                else statusType = ESkillStatusType.M_ATK;
                break;
            case EStatType.E_DEF:
                if (value > 0) statusType = ESkillStatusType.P_DEF;
                else statusType = ESkillStatusType.M_DEF;
                break;
            case EStatType.E_SPEED:
                if (value > 0) statusType = ESkillStatusType.P_SPEED;
                else statusType = ESkillStatusType.M_SPEED;
                break;
            case EStatType.E_CRITICALTRIGGERRATE:
                if (value > 0) statusType = ESkillStatusType.P_CRITICALTRIGGERRATE;
                else statusType = ESkillStatusType.M_CRITICALTRIGGERRATE;
                break;
            case EStatType.E_CRITICALVALUERATE:
                if (value > 0) statusType = ESkillStatusType.P_CRITICALVALUERATE;
                else statusType = ESkillStatusType.M_CRITICALVALUERATE;
                break;
        }
        AddStatusEffect(flow, statusType, duration, value);
    }

    public void UnitDead()
    {
        //m_system.RemoveUnit(this);
    }
}
