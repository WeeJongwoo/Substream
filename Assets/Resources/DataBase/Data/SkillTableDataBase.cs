using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SkillTableData
{
    public int ID;
    public int CardID;
    public ESkillType SkillType;
    public ESkillSource SkillSource;
    public float EffectValue;
    public float UpgradeEffectValue;
    public int HitCount;
    public ESkillStatusType StatusType;
    public ESkillTrigger Trigger;
    public int TriggerConditionValue;
    public ESkillTargetType TargetType;
    public int TargetCount;
    public ESkillSource TargetSource;
    public string Sound;
}

[System.Serializable]
public class SkillTableDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, CardData의 ID
    public Dictionary<int, SkillTableData> SkillTable = new Dictionary<int, SkillTableData>();

    public List<SkillTableData> SkillTableList = new List<SkillTableData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in SkillTableList)
        {
            if (!SkillTable.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in SkillTableList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
        }
    }

    public override void ClearContainer()
    {
        SkillTable.Clear();
        SkillTableList.Clear();
    }
}