using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DataScriptableObjects 
{
    public abstract bool TranslateListToDic(int SelectUserID);
    public abstract void TranslateDicToListAtSaveDatas(int SelectUserID);
    public abstract void ClearContainer();
}

[System.Serializable]
public enum ECardType
{
    E_DEFAULT = 0,
    E_ATTACK,
    E_SKILL
}

[System.Serializable]
public enum ECardRarity
{
    E_NONE = 0,
    E_NORMAL,
    E_RARE,
    E_SUPERRARE
}

[System.Serializable]
public enum ESkillType
{
    E_DEFAULT = 0,

    E_VARIATION,
    E_DRAW,
    E_STATUSEFFECT,
    E_ETC,

    E_DAMAGE,
    E_BOUNCE,
    E_HEAL,
    E_SHIELD,
    
    //100번부터는 스킬이 아닌 시스템 액션
    E_TURNEND = 100,
    E_ROUNDEND,
    E_UNITDYING
}

[System.Serializable]
public enum EPresentationType
{
    E_DEFAULT = 0,

    E_MAGICCAST,
    E_INSTANT,
    E_PHSICALATTACK,
    E_PASSIVETRIGGER,
    E_PAUSE
}

[System.Serializable]
public enum EStatSource
{
    E_NONE,
    E_FIXED,
    E_MAXAETHER,
    E_AETHER,
    E_DECK,
    E_DAMAGED_INFLICTED,

    E_MAXHP,
    E_DEFAULTHP,
    E_LOSTHP,
    E_HP,

    E_DEFAULTATK,
    E_ATK,

    E_DEFAULTDEF,
    E_DEF,

    E_SPEED,

    E_CRITICALRATE,
    E_CRITICALDAMAGE,

    E_AETHERRECOVER,
    E_PENETRATION,
    E_SHIELD,
}

[System.Serializable]
public enum EScaleType
{
    E_NONE,
    E_FIXED,
    E_MAXAETHER,
    E_AETHER,
    E_DECK,
    E_DAMAGED_INFLICTED,

    E_MAXHP,
    E_LOSTHP,
    E_DEFAULTHP,
    E_HP,

    E_DEFAULTATK,
    E_ATK,

    E_DEFAULTDEF,
    E_DEF,

    E_SPEED,

    E_CRITICALRATE,
    E_CRITICALDAMAGE,

    E_AETHERRECOVER,
    E_PENETRATION,
    E_SHIELD,

    // 상태이상 관련
    E_BLEED,
    E_SHOCK,
    E_OVERLOAD,
}

[System.Serializable]
public enum EStatusEffectType
{
    E_NONE = 0,
    E_BUFF,
    E_DEBUFF,
    E_BLEED,
    E_SHOCK,
    E_OVERLOAD,

    // 실제 효과
    M_MAXHP,
    M_ATK,
    M_DEF,
    M_SPEED,
    M_CRITICALRATE,
    M_CRITICALDAMAGE,
    M_AETHERRECOVER,
    M_PENETRATION,

    P_MAXHP,
    P_ATK,
    P_DEF,
    P_SPEED,
    P_CRITICALRATE,
    P_CRITICALDAMAGE,
    P_AETHERRECOVER,
    P_PENETRATION,

    E_NUM,
}

[System.Serializable]
public enum ETargetType
{
    E_NONE = 0,
    E_SELECT,
    E_SELF,
    E_ALLIES,
    E_ENEMY,
    E_ADJACENT,
    E_CHAINBEHIND
    /// 상세 타겟은 TargetCount로 결정
    /* 
     * TargetType이 E_SELF일때
     * TargetCount = 0(드로우 / 코스트회복)
     * TargetCount = 1, 본인만
     * TargetCount = 2일때 본인 + 아군 1명 대상
     * TargetCount = 3일 경우 본인 + 아군 2명 대상
     */
}

public enum ESkillTrigger
{
    E_DEFAULT = 0,
    E_CARD_USE,
    E_HAS_SHOCK,
    E_HAS_OVERROAD,
    E_WITH_FRONT
}
