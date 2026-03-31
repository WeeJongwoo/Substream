using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageLevel
{
    public List<StageNode> nodes = new List<StageNode>();
}

[System.Serializable]
public class StageData
{
    public List<StageLevel> stages = new List<StageLevel>();
    public int currentLevel;
    public string currentStageID;
    public int maxLevel;
    public bool isInitialized;

    /// <summary>
    /// ID로 노드 검색
    /// </summary>
    public StageNode FindNodeByID(string id)
    {
        foreach (var level in stages)
        {
            foreach (var node in level.nodes)
            {
                if (node.StageID == id)
                    return node;
            }
        }
        return null;
    }

    /// <summary>
    /// 특정 레벨의 노드 리스트 반환
    /// </summary>
    public List<StageNode> GetNodesAtLevel(int level)
    {
        if (level >= 0 && level < stages.Count)
            return stages[level].nodes;
        return null;
    }
}
