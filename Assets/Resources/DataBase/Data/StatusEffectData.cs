using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class StatusEffectData
{
    public int ID;
    public string StatusName;
    public int DurationTurn;
    public string EffectPerAction;
    public ETickTrigger TickTrigger;
}

[System.Serializable]
public class StatusEffectDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, CardData의 ID
    public Dictionary<int, StatusEffectData> StatusEffect = new Dictionary<int, StatusEffectData>();

    public List<StatusEffectData> StatusEffectList = new List<StatusEffectData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in StatusEffectList)
        {
            if (!StatusEffect.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in StatusEffectList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
        }
    }


    public override void ClearContainer()
    {
        StatusEffectList.Clear();
        StatusEffect.Clear();
    }
}