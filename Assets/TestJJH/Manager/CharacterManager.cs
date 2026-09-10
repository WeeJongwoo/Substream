using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : UnitManagerSystme
{
    public override void Initialize()
    {
        m_units = new List<Unit>();
        m_partyCount = 4;
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        base.InitializeReference(masterManager);
    }

    /// <summary>
    /// 1. DontDestroyOnLoadManager의 Party 정보를 가져옴
    /// 2. party에 속한 key 값 {UserID, PrototypeCharacterID, InstanceID}를 모두 가져옴
    /// 3. 가져온 key 값으로 DontDestroyOnLoadManager에서 찾아낸 다음 m_character로 모두 Add
    /// 4. 파티에서 지정한 위치 순서에 맞게 정렬하여 add
    /// </summary>
    public override void DataInitialize()
    {   
        for (int i = 0; i < m_partyCount; i++)
        {
            UnitTableData PU = DataBase.UnitTable(i + 1);
            UnitTableData newUnit = new UnitTableData(PU);

            newUnit.Init(this, true, i, PU.HP, PU.ATK, PU.DEF, PU.Speed, PU.CriticalRate, PU.CriticalDamage);
            
            m_units.Add(newUnit);
        }
        for (int i = m_partyCount; i < 4; i++)
        {
            this.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public override void UseCard(Card card)
    {
        base.UseCard(card);
    }

    public override void UnitDying(Unit unit)
    {
        if(unit.IsCharacter)
        {
            m_units.Remove(unit);
        }
    }
}
