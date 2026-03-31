using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public StageData stageData = new StageData();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// StageManager가 씬에 배치되어 Start()에서 호출하거나,
    /// GameManager가 직접 FindObjectOfType으로 호출할 수 있음
    /// </summary>
    public void InitializeStageManager(StageManager stageManager)
    {
        if (!stageData.isInitialized)
        {
            stageManager.GenerateNewStages(stageData);
        }

        stageManager.LoadStagesFromData(stageData);
    }

    /// <summary>
    /// 스테이지 클리어 처리 - 데이터 레벨에서 상태 변경
    /// </summary>
    public void OnStageClear(string clearedNodeID)
    {
        StageNode clearedNode = stageData.FindNodeByID(clearedNodeID);
        if (clearedNode == null) return;

        clearedNode.isCleared = true;
        clearedNode.isActive = false;

        // 같은 레벨의 다른 노드 비활성화
        List<StageNode> sameLevelNodes = stageData.stages[clearedNode.levelIndex].nodes;
        foreach (var node in sameLevelNodes)
        {
            if (!node.isCleared)
            {
                node.isActive = false;
            }
        }

        // 다음 레벨 노드 활성화
        foreach (string nextID in clearedNode.nextNodeIDs)
        {
            StageNode nextNode = stageData.FindNodeByID(nextID);
            if (nextNode != null && !nextNode.isCleared)
            {
                nextNode.isActive = true;
            }
        }

        stageData.currentLevel = clearedNode.levelIndex + 1;
        stageData.currentStageID = clearedNodeID;
    }
}
