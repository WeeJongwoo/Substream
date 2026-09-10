using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectStrategy
{
    public void Execute(Unit unit);
}

public class NoneStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Unit unit)
    {

    }
}


public class BleedStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Unit unit)
    {

    }
}

public class ShockStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Unit unit)
    {

    }
}

public class OverLoadStatusEffectStrategy : IStatusEffectStrategy
{
    public void Execute(Unit unit)
    {

    }
}

