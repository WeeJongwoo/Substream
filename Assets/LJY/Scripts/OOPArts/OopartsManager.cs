using System.Collections.Generic;
using UnityEngine;

public class OopartsManager : MonoBehaviour
{
    public static OopartsManager Instance;

    // 현재 보유 중인 오파츠 리스트
    private List<BaseOoparts> activeOoparts = new List<BaseOoparts>();

    public void AddOoparts(BaseOoparts ooparts)
    {
        activeOoparts.Add(ooparts);
        ooparts.OnEquip();
    }

    public void RemoveOoparts(BaseOoparts ooparts)
    {
        activeOoparts.Remove(ooparts);
        ooparts.OnUnequip();
    }

    // --- 이벤트 브로드캐스팅 함수들 ---

    public void TriggerBattleStart()
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnBattleStart();
        }
    }

    public void TriggerBattleEnd()
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnBattleEnd();
        }
    }

    public void TriggerTurnStart()
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnTurnStart();
        }
    }

    public void TriggerTurnEnd()
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnTurnEnd();
        }
    }

    /// <summary>
    /// 데미지를 받을 때, 오파츠들이 데미지를 경감시키거나 증폭시킬 수 있도록 처리
    /// </summary>
    public int ProcessIncomingDamage(int originalDamage)
    {
        int finalDamage = originalDamage;
        foreach (var ooparts in activeOoparts) {
            finalDamage = ooparts.OnTakeDamage(finalDamage);
        }
        return finalDamage;
    }

    /// <summary>
    /// HP가 감소/증가 했을 때, 오파츠 효과 발동 처리
    /// </summary>
    /// <param name="target">HP 변경 대상</param>
    public void TriggerUnitHPChanged(Unit target)
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnUnitHPChanged(target);
        }
    }

    public void TriggerDealDamage(int damage, Unit attacker, Unit target)
    {
        foreach (var ooparts in activeOoparts) {
            ooparts.OnDealDamage(damage, attacker, target);
        }
    }

    /// <summary>
    /// 체력이 0이 되었을 때 호출
    /// </summary>
    /// <param name="target">부활 효과 적용 대상</param>
    /// <returns>true라면 부활</returns>
    public bool TryRevive(Unit target)
    {
        foreach (var ooparts in activeOoparts) {
            // 부활 효과를 가진 오파츠가 있다면 true 반환
            if (ooparts.OnUnitDeath(target)) {
                return true;
            }
        }
        return false;
    }
}