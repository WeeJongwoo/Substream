using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CharacterTableData : Unit
{
    public int ID;
    public string ChracterName;
    public int HP;
    public int ATK;
    public int KEF;
    public int Speed;
    public float CriticalRate;
    public float CriticalDamage;
    public int AetherRecorverPoint;
    public string CharacterAnimationPrefab;
}

[System.Serializable]
public class CharacterTableDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, LocalUserData의 ID
    public Dictionary<int, CharacterTableData> CharacterTable = new Dictionary<int, CharacterTableData>();


    public List<CharacterTableData> CharacterTableList = new List<CharacterTableData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in CharacterTableList)
        {
            CharacterTable.Add(data.ID, data);
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in CharacterTableList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
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
        CharacterTableList.Clear();
        CharacterTable.Clear();
    }
}