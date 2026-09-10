using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : UnitManagingSystem
{
    public override void Initialize()
    {
        m_isSystemAboutCharacter = true;
        m_units = new Dictionary<int, Unit>();
        m_partyCount = 4;
    }

    public override void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
        m_turnManager = masterManager.TurnManager;
    }

    /// <summary>
    /// 1. DontDestroyOnLoadManager의 Party 정보를 가져옴
    /// 2. party에 속한 key 값 {UserID, PrototypeCharacterID, InstanceID}를 모두 가져옴
    /// 3. 가져온 key 값으로 DontDestroyOnLoadManager에서 찾아낸 다음 m_character로 모두 Add
    /// 4. 파티에서 지정한 위치 순서에 맞게 정렬하여 add
    /// </summary>
    public override void DataInitialize()
    {
        // 파티 정보
        int[] characterID = { 1, 5 };
        int i = 1;
        foreach (var a in characterID)
        {
            UnitTableData PU = DataBase.UnitTable(a);
            UnitTableData newUnit = new UnitTableData(PU);

            newUnit.Init(this, true, i, PU.HP, PU.ATK, PU.DEF, PU.Speed, PU.CriticalRate, PU.CriticalDamage, PU.Penetration, PU.AetherRecoverPoint);

            m_units.Add(i, newUnit);
            i++;
        }
        m_partyCount = characterID.Length;
    }

    public override void UseCard(Card card)
    {
        
    }

    public override void UnitDying(Unit unit)
    {
        if(unit.IsCharacter)
        {
            m_units.Remove(unit.Position);
        }
    }
}
