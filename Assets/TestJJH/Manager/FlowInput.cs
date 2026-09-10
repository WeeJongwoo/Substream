using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlowInput 
{

}

public class CardAbilityFlowInput : FlowInput
{
    public Unit CasterUnit;
    public Card CasterCard;
    public int SelectTargetPosition;

    public CardAbilityFlowInput(Unit unit, Card card, int selectTargetPosition)
    {
        CasterUnit = unit;
        CasterCard = card;
        SelectTargetPosition = selectTargetPosition;    
    }
}

public class SkillAbilityFlowInput : FlowInput
{
    public Unit CasterUnit;
    public Card CasterCard;
    public int SelectTargetPosition;

    public SkillAbilityFlowInput(Unit unit, Card card, int selectTargetPosition)
    {
        CasterUnit = unit;
        CasterCard = card;
        SelectTargetPosition = selectTargetPosition;
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

public class RoundEndFlowInput : FlowInput
{
    public RoundEndFlowInput()
    {

    }
}

public class SystemDrawCardFlowInput : FlowInput
{
    public Unit CasterUnit;
    public int Amount;
    public SystemDrawCardFlowInput(Unit unit, int amount)
    {
        CasterUnit = unit;
        Amount = amount;    
    }
}