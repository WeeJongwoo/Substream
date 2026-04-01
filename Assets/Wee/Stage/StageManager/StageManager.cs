using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    //public StageData stageData;
    public int maxLevel;
    public GameObject stageNodeUIPrefab;
    public RectTransform stageUIContainer;
    public ScrollRect scrollRect;
    private Dictionary<string, StageNodeUI> nodeUIMap = new Dictionary<string, StageNodeUI>();

    public float xSpacing = 250f;
    public float ySpacing = 150f;

    public float leftPadding = 0.0f;
    public float rightPadding = 200f;

    public Color lineColor = new Color(1f, 1f, 1f, 0.5f);
    public float lineWidth = 4f;
    private Dictionary<string, Vector2> nodePositionMap = new Dictionary<string, Vector2>();
    private List<GameObject> lineObjects = new List<GameObject>();


    void Start()
    {
        GameManager.Instance.stageData.maxLevel = maxLevel;
        GameManager.Instance.InitializeStageManager(this);
    }

    [Header("Stage Type Settings")]
    [Tooltip("상점 등장 후 재등장까지 최소 레벨 간격")]
    public int shopCooldownMin = 6;
    [Tooltip("상점 등장 후 재등장까지 최대 레벨 간격")]
    public int shopCooldownMax = 8;
    [Tooltip("이벤트 등장 후 재등장까지 최소 레벨 간격")]
    public int eventCooldownMin = 2;
    [Tooltip("이벤트 등장 후 재등장까지 최대 레벨 간격")]
    public int eventCooldownMax = 4;

    public void GenerateNewStages(StageData stageData)
    {
        stageData.stages = new List<StageLevel>();

        // 쿨다운 추적: 해당 타입이 다음에 등장 가능한 최소 레벨
        int shopAvailableAt = Random.Range(shopCooldownMin, shopCooldownMax + 1);
        int eventAvailableAt = Random.Range(eventCooldownMin, eventCooldownMax + 1);

        for (int i = 0; i < maxLevel; i++)
        {
            StageLevel level = new StageLevel();
            bool isFirstLevel = (i == 0);
            bool isLastLevel = (i == maxLevel - 1);

            // 첫/마지막 레벨은 노드 1개
            int numOfNodes = (isFirstLevel || isLastLevel) ? 1 : Random.Range(1, 4);

            // 이 레벨에서 등장 가능한 특수 타입 결정 (레벨당 1종만)
            StageType? forcedSpecialType = null;

            if (isLastLevel)
            {
                forcedSpecialType = StageType.Boss;
            }
            else if (!isFirstLevel)
            {
                // 우선순위: 상점 > 이벤트 (둘 다 가능하면 상점 우선)
                bool shopReady = (i >= shopAvailableAt);
                bool eventReady = (i >= eventAvailableAt);

                if (shopReady && eventReady)
                {
                    // 둘 다 가능 → 상점 우선
                    forcedSpecialType = StageType.Shop;
                }
                else if (shopReady)
                {
                    forcedSpecialType = StageType.Shop;
                }
                else if (eventReady)
                {
                    forcedSpecialType = StageType.RandomEvent;
                }
            }

            // 특수 타입이 배정될 노드 인덱스 (레벨 내 랜덤 1개)
            int specialNodeIndex = -1;
            if (forcedSpecialType.HasValue && forcedSpecialType.Value != StageType.Boss)
            {
                specialNodeIndex = Random.Range(0, numOfNodes);
            }

            for (int j = 0; j < numOfNodes; j++)
            {
                StageNode node = new StageNode();
                node.Initialize(i, j);

                if (isLastLevel)
                {
                    node.stageType = StageType.Boss;
                }
                else if (j == specialNodeIndex && forcedSpecialType.HasValue)
                {
                    node.stageType = forcedSpecialType.Value;
                }
                else
                {
                    node.stageType = StageType.Battle;
                }

                level.nodes.Add(node);
            }

            // 쿨다운 갱신: 해당 타입이 이 레벨에 배치되었으면 다음 쿨다운 설정
            if (forcedSpecialType == StageType.Shop)
            {
                shopAvailableAt = i + Random.Range(shopCooldownMin, shopCooldownMax + 1);
                // 이벤트는 건드리지 않음 - 다음 레벨부터 다시 체크
            }
            else if (forcedSpecialType == StageType.RandomEvent)
            {
                eventAvailableAt = i + Random.Range(eventCooldownMin, eventCooldownMax + 1);
            }

            stageData.stages.Add(level);
        }

        // 노드 간 연결 (ID 기반)
        BuildConnections(stageData);

        // 첫 레벨 활성화
        foreach (var node in stageData.stages[0].nodes)
        {
            node.SetActive(true);
        }

        stageData.currentLevel = 0;
        stageData.currentStageID = null;
        stageData.maxLevel = maxLevel;
        stageData.isInitialized = true;

        DebugPrintStageMap(stageData);
    }

    void DebugPrintStageMap(StageData stageData)
    {
        for (int i = 0; i < stageData.stages.Count; i++)
        {
            var level = stageData.stages[i];
            string info = "Level " + i + ": ";
            foreach (var node in level.nodes)
            {
                info += "[" + node.stageID + " " + node.stageType + "] ";
            }
            Debug.Log(info);
        }
    }

    void BuildConnections(StageData stageData)
    {
        for (int i = 0; i < maxLevel - 1; i++)
        {
            var currentNodes = stageData.stages[i].nodes;
            var nextNodes = stageData.stages[i + 1].nodes;

            if (nextNodes.Count == 1)
            {
                // 다음이 1개면 현재 모든 노드가 연결
                foreach (var node in currentNodes)
                {
                    AddConnection(node, nextNodes[0]);
                }
            }
            else if (currentNodes.Count == 1)
            {
                // 현재가 1개면 다음 모든 노드로 연결
                foreach (var next in nextNodes)
                {
                    AddConnection(currentNodes[0], next);
                }
            }
            else if (currentNodes.Count == nextNodes.Count)
            {
                // 같은 수: 1:1 매핑 + 랜덤 추가 연결 1개
                for (int n = 0; n < currentNodes.Count; n++)
                {
                    AddConnection(currentNodes[n], nextNodes[n]);
                }

                int extraCurrent = Random.Range(0, currentNodes.Count);
                int extraNext = Random.Range(0, nextNodes.Count);
                AddConnection(currentNodes[extraCurrent], nextNodes[extraNext]);
            }
            else if (currentNodes.Count == 3 && nextNodes.Count == 2)
            {
                AddConnection(currentNodes[0], nextNodes[0]);
                AddConnection(currentNodes[1], nextNodes[0]);
                AddConnection(currentNodes[1], nextNodes[1]);
                AddConnection(currentNodes[2], nextNodes[1]);
            }
            else if (currentNodes.Count == 2 && nextNodes.Count == 3)
            {
                AddConnection(currentNodes[0], nextNodes[0]);
                AddConnection(currentNodes[1], nextNodes[2]);

                int randomIndex = Random.Range(0, 2);
                AddConnection(currentNodes[randomIndex], nextNodes[1]);
            }
        }
    }

    /// <summary>
    /// 중복 연결 방지 헬퍼
    /// </summary>
    void AddConnection(StageNode from, StageNode to)
    {
        if (!from.nextNodeIDs.Contains(to.stageID))
        {
            from.nextNodeIDs.Add(to.stageID);
        }
    }

    // ─────────────────────────────────────────────
    //  UI 로드 (씬 진입 시마다, GameManager가 호출)
    // ─────────────────────────────────────────────
    public void LoadStagesFromData(StageData stageData)
    {
        nodeUIMap.Clear();
        nodePositionMap.Clear();

        // 기존 연결선 제거
        foreach (var line in lineObjects)
        {
            if (line != null) Destroy(line);
        }
        lineObjects.Clear();

        int totalLevels = stageData.stages.Count;

        // ── Content RectTransform 강제 설정 ──
        // Anchor: 좌측 세로 스트레치, Pivot: 좌측 중앙
        stageUIContainer.anchorMin = new Vector2(0, 0);
        stageUIContainer.anchorMax = new Vector2(0, 1);
        stageUIContainer.pivot = new Vector2(0, 0.5f);

        // Content 너비를 전체 맵 크기에 맞게 설정 → 가로 스크롤 가능
        float contentWidth = leftPadding + (totalLevels - 1) * xSpacing + rightPadding;
        stageUIContainer.sizeDelta = new Vector2(contentWidth, stageUIContainer.sizeDelta.y);

        float startX = leftPadding;

        // ── 노드 배치 ──
        for (int level = 0; level < totalLevels; level++)
        {
            var nodeList = stageData.stages[level].nodes;
            int nodeCount = nodeList.Count;

            float totalHeight = (nodeCount - 1) * ySpacing;

            for (int i = 0; i < nodeCount; i++)
            {
                StageNode node = nodeList[i];
                GameObject uiObj = Instantiate(stageNodeUIPrefab, stageUIContainer);
                RectTransform rt = uiObj.GetComponent<RectTransform>();

                // 기본 위치 (격자) + 저장된 랜덤 오프셋
                float baseX = startX + level * xSpacing;
                float baseY = (totalHeight / 2f) - (i * ySpacing);

                float x = baseX + node.offsetX;
                float y = baseY + node.offsetY;

                rt.anchoredPosition = new Vector2(x, y);

                StageNodeUI ui = uiObj.GetComponent<StageNodeUI>();
                ui.Initialize(node);

                nodeUIMap[node.stageID] = ui;
                nodePositionMap[node.stageID] = new Vector2(x, y);
            }
        }

        // ── 연결선 그리기 (노드보다 뒤에) ──
        DrawAllConnections(stageData);

        if (scrollRect != null)
        {
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }


    // ─────────────────────────────────────────────
    //  연결선
    // ─────────────────────────────────────────────
    void DrawAllConnections(StageData stageData)
    {
        foreach (var level in stageData.stages)
        {
            foreach (var node in level.nodes)
            {
                if (!nodePositionMap.ContainsKey(node.stageID)) continue;
                Vector2 fromPos = nodePositionMap[node.stageID];

                foreach (string nextID in node.nextNodeIDs)
                {
                    if (!nodePositionMap.ContainsKey(nextID)) continue;
                    Vector2 toPos = nodePositionMap[nextID];
                    DrawLine(fromPos, toPos);
                }
            }
        }
    }

    void DrawLine(Vector2 from, Vector2 to)
    {
        // Image 컴포넌트를 가진 오브젝트를 동적 생성
        GameObject lineObj = new GameObject("ConnectionLine");
        lineObj.transform.SetParent(stageUIContainer, false);

        // 노드보다 뒤에 그려지도록 맨 앞(하위)으로 이동
        lineObj.transform.SetAsFirstSibling();

        Image img = lineObj.AddComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false; // 클릭 방해 방지

        RectTransform rt = lineObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);  // 추가
        rt.anchorMax = new Vector2(0, 0.5f);  // 추가
        rt.pivot = new Vector2(0.5f, 0.5f);

        Vector2 direction = to - from;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rt.sizeDelta = new Vector2(distance, lineWidth);
        rt.anchoredPosition = from + direction * 0.5f;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        lineObjects.Add(lineObj);
    }

    // ─────────────────────────────────────────────
    //  UI 갱신
    // ─────────────────────────────────────────────
    public void RefreshAllUI()
    {
        foreach (var pair in nodeUIMap)
        {
            pair.Value.UpdateState();
        }
    }

    public void RefreshNodeUI(string nodeID)
    {
        if (nodeUIMap.TryGetValue(nodeID, out StageNodeUI ui))
        {
            ui.UpdateState();
        }
    }

    public StageInfo stageInfoUI;

    public void ShowStageInfo(StageNode node)
    {
        if (stageInfoUI != null)
        {
            stageInfoUI.Show(node);
        }
    }

    public void HideStageInfo()
    {
        if (stageInfoUI != null)
        {
            stageInfoUI.Hide();
        }
    }

    public void EnterStage(StageEventData data)
    {
        Debug.Log("Enter Stage: " + data.stageID + " Type: " + data.stageType);

        StageEvents.RaiseStageEnter(data);

        // GameManager를 통해 클리어 처리 (임시 - 추후 실제 스테이지 씬 로드로 교체)
        GameManager.Instance.OnStageClear(data.stageID);
        RefreshAllUI();
        HideStageInfo();
    }

}
