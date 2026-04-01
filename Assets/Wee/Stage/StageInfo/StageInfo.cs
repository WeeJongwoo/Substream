using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject stageInfo;          // StageInfo (루트 오브젝트)
    public Image stageImage;              // Image (초상화/아이콘 자리, 추후 사용)
    public TextMeshProUGUI stageTypeText; // StageType
    public Button enterButton;            // EnterButton
    public Button exitButton;             // ExitButton

    private StageNode currentNode;

    void Awake()
    {
        enterButton.onClick.AddListener(OnEnterClicked);
        exitButton.onClick.AddListener(OnExitClicked);
        stageInfo.SetActive(false);
    }

    public void Show(StageNode node)
    {
        currentNode = node;
        stageTypeText.text = GetStageTypeName(node.stageType);
        stageInfo.SetActive(true);
    }

    public void Hide()
    {
        stageInfo.SetActive(false);
        currentNode = null;
    }

    void OnEnterClicked()
    {
        if (currentNode == null) return;

        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager != null)
        {
            StageEventData stageData = new StageEventData
            {
                stageID = currentNode.stageID, // 예시로 현재 레벨을 최대 레벨로 설정
                stageType = currentNode.stageType,
                level = currentNode.levelIndex
            };

            stageManager.EnterStage(stageData);
        }
    }

    void OnExitClicked()
    {
        Hide();
    }

    string GetStageTypeName(StageType type)
    {
        switch (type)
        {
            case StageType.Battle: return "전투";
            case StageType.Shop: return "상점";
            case StageType.RandomEvent: return "랜덤 이벤트";
            case StageType.Boss: return "보스";
            default: return "알 수 없음";
        }
    }
}
