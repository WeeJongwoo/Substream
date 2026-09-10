using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IStatusEffectStrategy
{
    public void Execute(Flow flow, Unit unit);
}

public class NoneStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Flow flow, Unit unit)
    {

    }
}


public class BleedStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Flow flow, Unit unit)
    {
        float HPRate = 0.005f;

        float DEFPoint = unit.DefendValue.Now;

        float FinalAmount = HPRate * unit.HealthValue.Max * unit.GetSpecialStatusEffect(EStatusEffectType.E_BLEED) * (1 - DEFPoint / (DEFPoint + 1000));

        unit.ToDamage(flow, true, FinalAmount);
    }
}

public class ShockStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Flow flow, Unit unit)
    {
        float Fixed = 5f;

        float FinalAmount = Fixed * unit.GetSpecialStatusEffect(EStatusEffectType.E_SHOCK);

        unit.ToDamage(flow, true, FinalAmount);
    }
}

public class OverLoadStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Flow flow, Unit unit)
    {
        float HPRate = 0.003f;

        float FinalAmount = HPRate * unit.HealthValue.Max * unit.GetSpecialStatusEffect(EStatusEffectType.E_OVERLOAD);

        unit.ToDamage(flow, true, FinalAmount);
    }
}

