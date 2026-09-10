using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageNodeUI : MonoBehaviour
{
    public TextMeshProUGUI stageIDText;
    public Button button;
    public Image buttonImage;

    public StageNode linkedNode;

    public void Initialize(StageNode node)
    {
        linkedNode = node;
        stageIDText.text = linkedNode.stageID + "\n" + GetStageTypeLabel(linkedNode.stageType);

        if (buttonImage == null)
            buttonImage = button.GetComponent<Image>();

        button.onClick.AddListener(OnClick);
        UpdateState();
        Debug.Log("Init NodeUI" + linkedNode.isActive);
    }

    public void OnClick()
    {
        if (!linkedNode.isActive || linkedNode.isCleared) return;

        Debug.Log("Enter Stage: " + linkedNode.stageID);

        // 팝업 표시
        StageManager stageManager = FindObjectOfType<StageManager>();
        if (stageManager != null)
        {
            stageManager.ShowStageInfo(linkedNode);
        }
    }

    public void UpdateState()
    {
        button.interactable = linkedNode.isActive && !linkedNode.isCleared;

        // 클리어된 노드 시각적 표시 (필요 시 확장)
        if (linkedNode.isCleared)
        {
            buttonImage.color = Color.gray;
            stageIDText.color = Color.white;
        }
        else
        {
            buttonImage.color = GetStageTypeColor(linkedNode.stageType);
        }
    }

    Color GetStageTypeColor(StageType type)
    {
        switch (type)
        {
            case StageType.Battle: return new Color(0.9f, 0.3f, 0.3f);  // 빨강
            case StageType.Shop: return new Color(0.3f, 0.8f, 0.3f);  // 초록
            case StageType.RandomEvent: return new Color(0.3f, 0.5f, 0.9f);  // 파랑
            case StageType.Boss: return new Color(0.7f, 0.2f, 0.8f);  // 보라
            default: return Color.white;
        }
    }

    string GetStageTypeLabel(StageType type)
    {
        switch (type)
        {
            case StageType.Battle: return "전투";
            case StageType.Shop: return "상점";
            case StageType.RandomEvent: return "이벤트";
            case StageType.Boss: return "보스";
            default: return "???";
        }
    }
}
