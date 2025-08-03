using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class MonsterData : Unit
{
    public int UserID;
    public int PrototypeUnitID;
    public int InstanceID;
    public int Speed;
}

[System.Serializable]
public class MonsterDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 (int,int,int)형식, 순서대로 UserID, PrototypeUnitID, InstanceID
    public Dictionary<(int, int, int), MonsterData> Monster = new Dictionary<(int, int, int), MonsterData>();


    public List<MonsterData> MonsterList = new List<MonsterData>();

    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in MonsterList)
        {
            if (SelectUserID == data.UserID)
            {
                var key = (data.UserID, data.PrototypeUnitID, data.InstanceID);
                Monster.Add(key, data);
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in MonsterList)
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
}