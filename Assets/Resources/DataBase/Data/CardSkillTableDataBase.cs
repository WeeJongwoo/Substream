using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CardSkillTableData
{
    public int ID;
    public ECardSkillType SkillType;
    public ECardSkillSource SkillSource;
    public float EffectValue;
    public float UpgradeEffectValue;
    public ECardSkillStatusType StatusType;
    public ECardSkillTargetType TargetType;
    public int TargetCount;
    public int HitCount;
    public string CardText;
    public int NextSkillID;
    public string Sound;
}

[System.Serializable]
public class CardSkillTableDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, CardData의 ID
    public Dictionary<int, CardSkillTableData> CardSkillTable = new Dictionary<int, CardSkillTableData>();

    public List<CardSkillTableData> CardSkillTableList = new List<CardSkillTableData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in CardSkillTableList)
        {
            if (!CardSkillTable.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in CardSkillTableList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
        }
    }

    public override void ClearContainer()
    {
        CardSkillTableList.Clear();
        CardSkillTable.Clear();
    }
}