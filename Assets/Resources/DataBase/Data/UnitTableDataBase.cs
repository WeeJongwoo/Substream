using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class UnitTableData : Unit
{
    // 사용하는 유닛의 정보
    public int ID;
    public string Name;
    public int HP;
    public int ATK;
    public int DEF;
    public int Speed;
    public float CriticalRate;
    public float CriticalDamage;
    public int AetherRecorverPoint;
    public string CharacterAnimationPrefab;

    public override int IngameUnitID()
    {
        return ID;
    }
    public UnitTableData()
    {

    }
    public UnitTableData(UnitTableData Prototype)
    {
        this.ID = Prototype.ID;
        this.Name = Prototype.Name;
        this.HP = Prototype.HP;
        this.ATK = Prototype.ATK;
        this.DEF = Prototype.DEF;
        this.Speed = Prototype.Speed;
        this.CriticalRate = Prototype.CriticalRate;
        this.CriticalDamage = Prototype.CriticalDamage;
        this.AetherRecorverPoint = Prototype.AetherRecorverPoint;
        this.CharacterAnimationPrefab = Prototype.CharacterAnimationPrefab;
    }
}

[System.Serializable]
public class UnitTableDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 (int,int,int)형식, 순서대로 UserID, PrototypeUnitID, InstanceID
    public Dictionary<int, UnitTableData> UnitTable = new Dictionary<int, UnitTableData>();


    public List<UnitTableData> UnitTableList = new List<UnitTableData>();

    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in UnitTableList)
        {
            if (!UnitTable.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in UnitTableList)
        {
            /*
            data.Day = LocalUserDataDic[data.ID].Day;
            data.Gold = LocalUserDataDic[data.ID].Gold;
            data.DeathEssence = LocalUserDataDic[data.ID].DeathEssence;
            data.DarkEssence = LocalUserDataDic[data.ID].DarkEssence;
            data.UnitInstanceCounter = LocalUserDataDic[data.ID].UnitInstanceCounter;
            data.Floor = LocalUserDataDic[data.ID].Floor;
            data.BusEnhance = LocalUserDataDic[data.ID].BusEnhance;
            data.PrisonEnhance = LocalUserDataDic[data.ID].PrisonEnhance;
            data.HealthEnhance = LocalUserDataDic[data.ID].HealthEnhance;
            data.ErosionEnhance = LocalUserDataDic[data.ID].ErosionEnhance;
            data.GYMEnhance = LocalUserDataDic[data.ID].GYMEnhance;
            data.SmithEnhance = LocalUserDataDic[data.ID].SmithEnhance;
            data.BattleEfficiency = LocalUserDataDic[data.ID].BattleEfficiency;
            data.BattleReward = LocalUserDataDic[data.ID].BattleReward;
            */
        }
    }

    public override void ClearContainer()
    {
        UnitTableList.Clear();
        UnitTable.Clear();
    }
}