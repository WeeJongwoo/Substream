using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class ResultIdGenerator
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


public enum EResultType
{
    E_DEFAULT = 0,

    // 유닛 행동
    E_ATTACK,
    E_CAST,
    E_SKILL,

    // 수치 변화
    E_CHANGEHP,
    E_ADDSHIELD,
    E_CHANGESTACK,
    E_CHANGEAETHER,

    // 카드 관련
    E_CARDDRAW,
    E_CARDUSE,

    // 시스템
    E_UNITDYING,
    E_TURNEND,
    E_ROUNDEND,
    E_PAUSE
}

public class ContextResult
{
    public EResultType ResultType;

    public readonly int ID;

    public int SourceSkillID;
    public ContextResult(EResultType resultType)
    {
        ResultType = resultType;
        ID = ResultIdGenerator.Next();
    }
}




public class AttackResult : ContextResult
{
    /// <summary>
    /// 물리 공격 관련
    /// 공격 행위자
    /// 피격 대상자
    /// 치명타 여부
    /// </summary>
    public TargetPair Attacker;
    public TargetPair Target;
    public bool IsCritical;

    public AttackResult() : base(EResultType.E_ATTACK)
    {

    }
}

public class CastResult : ContextResult
{
    /// <summary>
    /// 마법 공격 관련
    /// 마법 시전자
    /// 치명타 여부
    /// </summary>
    public TargetPair Caster;
    public bool IsCritical;

    public CastResult() : base(EResultType.E_CAST)
    {

    }
}

public class ChangeHPResult : ContextResult
{
    /// <summary>
    /// 대상
    /// 체력 변경 상황 유형
    /// 공격 종류(안쓸듯)
    /// 체력 변경 수치
    /// 오버된 수치
    /// </summary>
    public TargetPair Target;
    public bool IsDamage;
    public EStatusEffectType AttackStatusType;
    public float Amount;
    public float OverAmount;
    public ChangeHPResult() : base(EResultType.E_CHANGEHP)
    {

    }
}

public class AddShieldResult : ContextResult
{
    public TargetPair Target;
    public float Amount;
    public AddShieldResult() : base(EResultType.E_ADDSHIELD)
    {

    }
}

public class ChangeStackResult : ContextResult
{
    public TargetPair Target;
    public EStatusEffectType StatusType;
    public int RoundDuration;
    public int TurnDuration;
    public int Stack;

    public bool IsNew;

    public ChangeStackResult() : base(EResultType.E_CHANGESTACK)
    {
        
    }
}

public class ChangeAetherResult : ContextResult
{
    public ChangeAetherResult() : base(EResultType.E_CHANGEAETHER)
    {

    }
}

public class CardDrawResult : ContextResult
{
    public int Amount;
    public CardDrawResult() : base(EResultType.E_CARDDRAW)
    {

    }
}

public class CardUseResult : ContextResult
{
    public int Amount;
    public CardUseResult() : base(EResultType.E_CARDUSE)
    {

    }
}



public class UnitDyingResult : ContextResult
{
    public TargetPair Victim;

    public UnitDyingResult() : base(EResultType.E_UNITDYING)
    {

    }
}

public class TurnEndResult : ContextResult
{
    public TurnEndResult() : base(EResultType.E_TURNEND)
    {

    }
}

public class RoundEndResult : ContextResult
{
    public RoundEndResult() : base(EResultType.E_ROUNDEND)
    {

    }
}

public class PauseResult : ContextResult
{
    public float PauseTime;
    public PauseResult() : base(EResultType.E_PAUSE)
    {

    }
}
