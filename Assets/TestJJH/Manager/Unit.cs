using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[System.Serializable]
public struct StatusEffect
{
    public ESkillStatusType Effect;
    public int Duration;
    public float Value;
    public Unit Caster;
}

public struct IStatModifier
{
    public int Duration;
    public float Amount;
}

public enum EStatType
{
    HP,
    ATK,
    DEF,
    Speed,
    CriticalTriggerRate,
    CriticalValueRate
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

    public void SetTurn()
    {
        foreach (var modifierIt in modifiers)
        {
            var modifierDuration = modifierIt.Duration;
            modifierDuration--;
            if (modifierIt.Duration < 0)
            {
                modifiers.Remove(modifierIt);
                Now -= modifierIt.Amount;
            }
        }
    }
}

[System.Serializable]
public abstract class Unit
{
    private UnitManagerSystme m_system;
    private bool m_isCharacter = true;
    public GameObject thisObject;
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


    public Stat HealthPoint => m_stats[EStatType.HP];
    public Stat AttackPoint => m_stats[EStatType.ATK];
    public Stat DefendPoint => m_stats[EStatType.DEF];
    public Stat SpeedPoint => m_stats[EStatType.Speed];
    public Stat CriticalTriggerRate => m_stats[EStatType.CriticalTriggerRate];
    public Stat CriticalValueRate => m_stats[EStatType.CriticalValueRate];

    [SerializeField]
    public Stat DebugHP;
    [SerializeField]
    private UnitSlot m_ui;

    // 전투 관련 기본 스탯
    [SerializeField]
    private Dictionary<EStatType, Stat> m_stats = new Dictionary<EStatType, Stat>();

    // 상태이상 관련 (출혈, 감전 등)
    [SerializeField]
    private LinkedList<StatusEffect> m_statusEffect = new LinkedList<StatusEffect>();

    public LinkedList<StatusEffect> StatusEffect
    {
        get { return m_statusEffect; }
    }

    public void Init(UnitManagerSystme system, bool isCharacter, int pos, float hp, float atk, float def, float speed, float CriticalTriggerRate, float CriticalValueRate)
    {
        m_system = system;
        m_isCharacter = isCharacter;
        m_position = pos;

        m_stats.Add(EStatType.HP, new Stat(hp));
        m_stats.Add(EStatType.ATK, new Stat(atk));
        m_stats.Add(EStatType.DEF, new Stat(def));
        m_stats.Add(EStatType.Speed, new Stat(speed));
        m_stats.Add(EStatType.CriticalTriggerRate, new Stat(CriticalTriggerRate));
        m_stats.Add(EStatType.CriticalValueRate, new Stat(CriticalValueRate));

        DebugHP = m_stats[EStatType.HP];
    }

    public void SetTurn()
    {
        // 상태이상 자체 효과
        foreach (var effectIT in m_statusEffect)
        {
            m_system.StatusEffectExecuteStrategy[effectIT.Effect].Execute(this);
        }

        var effectIterator = m_statusEffect.First;
        while(effectIterator.Next != null)
        {
            var next = effectIterator.Next;
            var effectDuration = effectIterator.Value.Duration;
            effectDuration--;
            if (effectDuration < 0)
            {
                m_statusEffect.Remove(effectIterator);
                continue;
            }
            effectIterator = next;
        }
    }

    public void AddBuff(ESkillStatusType effect, int duration, float value, Unit caster)
    {
        StatusEffect newEffect;
        newEffect.Effect = effect;
        newEffect.Duration = duration;
        newEffect.Value = value;
        newEffect.Caster = caster;
        m_statusEffect.AddFirst(newEffect);
    }

    public void AddDeBuff(ESkillStatusType effect, int duration, float value, Unit caster)
    {
        StatusEffect newEffect;
        newEffect.Effect = effect;
        newEffect.Duration = duration;
        newEffect.Value = value;
        newEffect.Caster = caster;
        m_statusEffect.AddLast(newEffect);
    }

    public void UnitDead()
    {
        //m_system.RemoveUnit(this);
    }
}
