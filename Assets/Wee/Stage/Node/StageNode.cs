using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Battle,
    Shop,
    RandomEvent,
    Boss
}

public class StageNode
{
    public string StageID;
    public int levelIndex;
    public int stageIndex;

    public StageType stageType;
    public List<string> nextNodeIDs = new List<string>();

    public bool isCleared;
    public bool isActive;

    // 배치용 랜덤 오프셋 (생성 시 결정, 데이터에 저장되어 씬 재로드 시에도 유지)
    public float offsetX;
    public float offsetY;

    public void Initialize(int inLevelInedex, int IDNum)
    {
        levelIndex = inLevelInedex;
        stageIndex = IDNum;
        isCleared = false;
        isActive = false;
        nextNodeIDs = new List<string>();
        StageID = inLevelInedex + "_" + IDNum;

        // 랜덤 오프셋 생성
        offsetX = Random.Range(-40f, 40f);
        offsetY = Random.Range(-30f, 30f);
    }

    //public void ClearStage()
    //{
    //    isCleared = true;
    //    isActive = false;

    //    for (int i = 0; i < nextNodes.Count; i++)
    //    {
    //        if (nextNodes[i] != null)
    //        {
    //            nextNodes[i].SetActive(true);
    //        }
    //    }
    //}

    public void SetActive(bool state)
    {
        isActive = state;
        Debug.Log("Node Set Active: " + StageID + " " + isActive);
    }
}
