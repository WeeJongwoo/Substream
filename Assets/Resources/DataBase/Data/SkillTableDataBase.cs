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
    public ESkillType SkillType;
    public EPresentationType PresentationType;

    public ESkillTrigger Trigger;
    public ETargetType TriggerTargetType;
    public int TriggerConditionValue;

    public EStatSource SkillSource;
    public ETargetType SkillSourceTargetType;
    public float EffectValue;
    public float UpgradeValue;
    public bool IsFixed;
    public int HitCount;

    public EScaleType ScaleType;
    public ETargetType ScaleTypeTargetType;
    public float ScaleFactor;
    public int ScaleLimit;

    public EStatusEffectType StatusType;
    public int StatusCount;
    public int RoundDuration;
    public int StatusDuration;

    public bool RefreshTarget;
    public ETargetType TargetType;
    public int TargetCount;
    public EStatSource TargetStatSource;
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