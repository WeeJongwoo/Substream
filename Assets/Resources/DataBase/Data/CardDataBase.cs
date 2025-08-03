using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public int ID;
    public string Name;
    public ECardType Type;
    public int Rank;
    public int Cost;
    public int TargetCount;
    public string Explanation;
}

[System.Serializable]
public class CardDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, CardData의 ID
    public Dictionary<int, CardData> Card = new Dictionary<int, CardData>();

    public List<CardData> CardList = new List<CardData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in CardList)
        {
            if(!Card.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in CardList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
        }
    }
}