using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageNodeUI : MonoBehaviour
{
    public TextMeshProUGUI stageIDText;
    public Button button;
    public StageNode linkedNode;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialize(StageNode node)
    {
        linkedNode = node;
        stageIDText.text = linkedNode.StageID;
        //button.onClick.AddListener(OnClick);
        Debug.Log("Init NodeUI" + linkedNode.isActive);
    }

    public void OnClick()
    {
        //Debug.Log("Enter Stage: " + linkedNode.StageID);
        if (linkedNode.isActive)
        {
            //스테이지 진입 처리 로직
            Debug.Log("Enter Stage: " + linkedNode.StageID);

            //임시 테스트
            StageManager.Instance.StageClear(linkedNode);
        }
    }

    public void UpdateState()
    {
        button.interactable = linkedNode.isActive;
        Debug.Log("UI Stage Active: " + linkedNode.StageID + " " + linkedNode.isActive);
    }
}
