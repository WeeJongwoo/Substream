using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 자신 삭제
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동에도 유지하고 싶다면
    }

    public StageData stageData;
    public int maxLevel;

    public GameObject stageNodeUIPrefab;
    public RectTransform stageUIContainer;
    private Dictionary<StageNode, StageNodeUI> nodeUIMap = new Dictionary<StageNode, StageNodeUI>();

    public void Initialize()
    {
        stageData = GameManager.Instance.stageData;

        if (!stageData.isInitialized)
        {
            InitializeStages();
        }

        LoadStagesFromData();
    }

    void UIActive(StageNode node)
    {
        nodeUIMap[node].UpdateState();
    }

    //public void StageClear(StageNode node)
    //{
    //    node.ClearStage();
    //    UIActive(node);

    //    foreach (var sameLevelNode in stages[node.levelIndex])
    //    {
    //        if (!sameLevelNode.isCleared)
    //        {
    //            sameLevelNode.SetActive(false);
    //            UIActive(sameLevelNode);
    //        }
    //    }

    //    foreach (var nextNode in node.nextNodes)
    //    {
    //        if (nextNode != null && !nextNode.isCleared)
    //        {
    //            UIActive(nextNode);
    //        }
    //    }
    //}

    void InitializeStages()
    {
        int currentLevel = 0;

        List<List<StageNode>> stages = new List<List<StageNode>>();

        for (int i = 0; i < maxLevel - 1; i++)
        {
            List<StageNode> stageLevel = new List<StageNode>();
            int numOfNodes = Random.Range(1, 4);
            for (int j = 0; j < numOfNodes; j++)
            {
                StageNode stageNode = new StageNode();
                stageNode.Initialize(i, j);
                stageLevel.Add(stageNode);
                //스테이지 데이터 세팅
            }
            stages.Add(stageLevel);
        }
        List<StageNode> fivalStageLevel = new List<StageNode>();
        StageNode finalStageNode = new StageNode();
        finalStageNode.Initialize(maxLevel - 1, 0);
        fivalStageLevel.Add(finalStageNode);
        stages.Add(fivalStageLevel);

        //for (int i = 0; i < stages.Count; i++)
        //{
        //    Debug.Log("Level " + i + " has " + stages[i].Count + " nodes.");
        //}

        for (int i = 0; i < maxLevel - 1; i++)
        {
            var nextNodes = stages[i + 1];
            var currentNodes = stages[i];

            if (nextNodes.Count == 1)
            {
                foreach (var node in currentNodes)
                {
                    node.nextNodes.Add(nextNodes[0]);
                }
            }
            if (currentNodes.Count == 1)
            {
                foreach (var node in nextNodes)
                {
                    currentNodes[0].nextNodes.Add(node);
                }
            }

            if (currentNodes.Count == nextNodes.Count)
            {
                for (int n = 0; n < currentNodes.Count; n++)
                {
                    currentNodes[n].nextNodes.Add(nextNodes[n]);
                }

                int extraCurrentIndex = Random.Range(0, currentNodes.Count);
                int extraNextIndex = Random.Range(0, nextNodes.Count);

                currentNodes[extraCurrentIndex].nextNodes.Add(nextNodes[extraNextIndex]);
            }

            if (currentNodes.Count == 3 && nextNodes.Count == 2)
            {
                currentNodes[0].nextNodes.Add(nextNodes[0]);

                currentNodes[1].nextNodes.Add(nextNodes[0]);
                currentNodes[1].nextNodes.Add(nextNodes[1]);

                currentNodes[2].nextNodes.Add(nextNodes[1]);
            }

            if (currentNodes.Count == 2 && nextNodes.Count == 3)
            {
                currentNodes[0].nextNodes.Add(nextNodes[0]);
                currentNodes[1].nextNodes.Add(nextNodes[2]);

                int randomIndex = Random.Range(0, 2);
                currentNodes[randomIndex].nextNodes.Add(nextNodes[1]);
            }

            //foreach (var nextNode in nextNodes)
            //{
            //    int numberOfConnections = Random.Range(1, currentNodes.Count + 1);

            //    List<int> currentNodeIndex = Enumerable.Range(0, currentNodes.Count).ToList();
            //    currentNodeIndex = currentNodeIndex.OrderBy(_ => Random.value).ToList();
            //    currentNodeIndex = currentNodeIndex.Take(numberOfConnections).ToList();

            //    foreach (var index in currentNodeIndex)
            //    {
            //        currentNodes[index].nextNodes.Add(nextNode);
            //        numberOfConnections--;
            //    }
            //}

            //foreach (var node in currentNodes)
            //{
            //    if (node.nextNodes.Count == 0)
            //    {
            //        int randomIndex = Random.Range(0, nextNodes.Count);
            //        node.nextNodes.Add(nextNodes[randomIndex]);
            //    }
            //}
        }

        for (int i = 0; i < stages.Count; i++)
        {
            for (int j = 0; j < stages[i].Count; j++)
            {
                for (int k = 0; k < stages[i][j].nextNodes.Count; k++)
                {
                    //Debug.Log("Node " + i + "-" + j +  " has " + stages[i][j].nextNodes[k].StageID + " next nodes.");
                }
            }
        }

        for (int i = 0; i < stages[0].Count; i++)
        {
            stages[0][i].SetActive(true);
        }

        stageData.currentLevel = currentLevel;
        stageData.stages = stages;
        stageData.maxLevel = maxLevel;
        stageData.currentStage = null;
        stageData.isInitialized = true;
    }

    void LoadStagesFromData()
    {
        // ----------------------------------UI 생성-------------------------------------
        float xSpacing = 250f; // 레벨 간 가로 간격
        float ySpacing = 150f; // 노드 간 세로 간격

        int totalLevels = stageData.stages.Count;

        // 전체 너비 기준으로 X축 중앙 정렬
        float totalWidth = (totalLevels - 1) * xSpacing;
        float startX = -totalWidth / 2f;

        for (int level = 0; level < totalLevels; level++)
        {
            var nodeList = stageData.stages[level];
            int nodeCount = nodeList.Count;

            // 레벨 내 노드들을 세로로 정렬 → 중앙 기준
            float totalHeight = (nodeCount - 1) * ySpacing;
            float startY = totalHeight / 2f;

            for (int i = 0; i < nodeCount; i++)
            {
                StageNode node = nodeList[i];
                GameObject uiObj = Instantiate(stageNodeUIPrefab, stageUIContainer);
                RectTransform rt = uiObj.GetComponent<RectTransform>();

                float x = startX + level * xSpacing;
                float y = startY - i * ySpacing;

                rt.anchoredPosition = new Vector2(x, y);

                StageNodeUI ui = uiObj.GetComponent<StageNodeUI>();
                ui.Initialize(node);
                nodeUIMap[node] = ui;
                nodeUIMap[node].UpdateState();
            }
        }
    }

    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
