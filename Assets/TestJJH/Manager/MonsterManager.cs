using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : UnitManagerSystme
{
    public override void Initialize()
    {
        m_units = new List<Unit>();
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        base.InitializeReference(masterManager);
    }

    public override void DataInitialize()
    {
        for (int i = 0; i < 1; i++)
        {
            UnitTableData PU = DataBase.UnitTable(i + 1);
            UnitTableData newUnit = new UnitTableData(PU);

            newUnit.Init(this, false, i, PU.HP, PU.ATK, PU.DEF, PU.Speed, PU.CriticalRate, PU.CriticalDamage);

            m_units.Add(newUnit);
        }
    }

    public override void UseCard(Card card)
    {
        base.UseCard(card);
    }

    public override void UnitDying(Unit unit)
    {
        if (!unit.IsCharacter)
        {
            m_units.Remove(unit);
        }
    }
}
