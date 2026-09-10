using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : UnitManagingSystem
{
    public override void Initialize()
    {
        m_isSystemAboutCharacter = false;
        m_units = new Dictionary<int, Unit>();
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
        m_turnManager = masterManager.TurnManager;
    }

    public override void DataInitialize()
    {
        // 파티 정보
        int[] monsterID = { 1001 };
        int i = 1;
        foreach (var a in monsterID)
        {
            UnitTableData MU = DataBase.UnitTable(a);
            UnitTableData newUnit = new UnitTableData(MU);

            newUnit.Init(this, false, i, MU.HP, MU.ATK, MU.DEF, MU.Speed, MU.CriticalRate, MU.CriticalDamage, MU.Penetration, MU.AetherRecoverPoint);

            m_units.Add(i, newUnit);
            i++;
        }
        m_partyCount = monsterID.Length;
    }

    public override void UseCard(Card card)
    {
        base.UseCard(card);
    }

    public override void UnitDying(Unit unit)
    {
        if (!unit.IsCharacter)
        {
            m_units.Remove(unit.Position);
        }
    }
}
