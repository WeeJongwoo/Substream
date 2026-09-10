using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageNode
{
    public string StageID;
    public int levelIndex;
    public List<StageNode> nextNodes;
    private int stageDateType;

    public bool isCleared;
    public bool isActive;

    public void Initialize(int inLevelInedex, int IDNum)
    {
        levelIndex = inLevelInedex;
        isCleared = false;
        isActive = false;
        nextNodes = new List<StageNode>();
        StageID = inLevelInedex.ToString() + "_" + IDNum.ToString();
    }

    public void ClearStage()
    {
        isCleared = true;
        isActive = false;

        for (int i = 0; i < nextNodes.Count; i++)
        {
            if (nextNodes[i] != null)
            {
                nextNodes[i].SetActive(true);
            }
        }
    }

    public void SetActive(bool state)
    {
        isActive = state;
        Debug.Log("Node Set Active: " + StageID + " " + isActive);
    }
}
