using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataScriptableObjects 
{
    public abstract bool TranslateListToDic(int SelectUserID);
    public abstract void TranslateDicToListAtSaveDatas(int SelectUserID);
}

[System.Serializable]
public enum ECardType
{
    E_DEFAULT = 0,
    E_SKILL
}

[System.Serializable]
public enum ECardRarity
{
    E_NORMAL = 0,
    E_RARE,
    E_SUPERRARE
}

[System.Serializable]
public enum ECardSkillType
{
    E_DEFAULT = 0,
    E_DAMAGE,
    E_STATUS_EFFECT,
    E_HEAL,
    E_INCREASE,
    E_DRAW
}

[System.Serializable]
public enum ECardSkillSource
{
    E_DEFAULT = 0,
    E_ATK,
    E_BLEED,
    E_DAMAGE_INFLICTED,
    E_AETHER,
    E_DECK,
    E_DEFENSE
}

public enum ETickTrigger
{
    E_DEFAULT = 0,
    E_ON_ACTION
}