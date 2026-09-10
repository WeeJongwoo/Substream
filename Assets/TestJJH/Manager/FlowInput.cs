using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowInput 
{

}

public class AbilityFlowInput : FlowInput
{
    public Unit CasterUnit;
    public Card CasterCard;

    public AbilityFlowInput(Unit unit, Card card)
    {
        CasterUnit = unit;
        CasterCard = card;
    }
}

public class UnitDyingFlowInput : FlowInput
{
    public Unit Victim;

    public UnitDyingFlowInput(Unit victim)
    {
        Victim = victim;    
    }
}

public class TurnEndFlowInput : FlowInput
{
    public TurnEndFlowInput()
    {

    }
}