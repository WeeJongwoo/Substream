using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CardTableData
{
    public int ID;
    public string Name;
    public ECardType CardType;
    public ECardRarity CardRarity;
    public int Cost;
    public string CardText;
    public string Texture;
    public int SkillID;
}

[System.Serializable]
public class CardTableDataBase : DataScriptableObjects
{
    [Serialize]
    //key 는 int 형, CardData의 ID
    public Dictionary<int, CardTableData> CardTable = new Dictionary<int, CardTableData>();

    public List<CardTableData> CardTableList = new List<CardTableData>();
    public override bool TranslateListToDic(int SelectUserID)
    {
        bool result = true;
        foreach (var data in CardTableList)
        {
            if(!CardTable.TryAdd(data.ID, data))
            {
                result = false;
            }
        }
        return result;
    }

    public override void TranslateDicToListAtSaveDatas(int SelectUserID)
    {
        foreach (var data in CardTableList)
        {
            //딕셔너리 데이터를 리스트로 재저장하여 수정
            //수정할 데이터는 key값이 아닌 모든 값
        }
    }

    public override void ClearContainer()
    {
        CardTableList.Clear();
        CardTable.Clear();
    }
}