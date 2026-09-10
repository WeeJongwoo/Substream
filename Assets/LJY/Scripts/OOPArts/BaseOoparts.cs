using UnityEngine;

public abstract class BaseOoparts
{
    public int oopartsId;
    public string oopartsName;

    // 획득/해제 시점
    public virtual void OnEquip() { }
    public virtual void OnUnequip() { }

    // 전투 페이즈 시점
    public virtual void OnBattleStart() { }
    public virtual void OnBattleEnd() { }
    public virtual void OnTurnStart() { }
    public virtual void OnTurnEnd() { }

    // 액션 시점 (카드, 데미지, 힐 등)
    public virtual void OnCardPlayed(Card card) { }

    /// <summary>
    /// 데미지 계산 시 개입 (받는 데미지를 수정하여 반환)
    /// </summary>
    public virtual int OnTakeDamage(int damage) { return damage; }

    /// <summary>
    /// 체력이 변경되었을 때 (피격 후, 회복 후 등) 호출
    /// </summary>
    public virtual void OnUnitHPChanged(Unit target) { }

    /// <summary>
    /// 공격 시 개입
    /// </summary>
    public virtual int OnDealDamage(int damage, Unit attacker, Unit target) { return damage; }

    // 특수 시점
    /// <summary>
    /// 반환값이 true이면 플레이어 사망을 취소시킴
    /// </summary>
    /// <returns>부활 여부</returns>
    public virtual bool OnUnitDeath(Unit target) { return false; }

}