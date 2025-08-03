using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class TurnUIManager : BaseManager, IsynchronizeUI
{
    public struct UnitPortraitPair
    {
        public Image s_portrait;
        public Unit s_unit;
    }

    [SerializeField]
    private List<Image> OrderByTurnSpeedImage;

    [SerializeField]
    private Button m_turnEndButton;
    [SerializeField]
    private Text m_turnText;
    private StringBuilder m_stringBuilder = new StringBuilder(32);

    private List<UnitPortraitPair> m_unitPortraitPair;

    // Start is called before the first frame update
    public override void Initialize(MasterManager masterManager, TurnManager turnManager) 
    {
        m_unitPortraitPair = new List<UnitPortraitPair>();
        m_masterManager = masterManager;
        SetTurnEtherInfo(turnManager);
        m_turnEndButton.onClick.AddListener(() => {
            m_masterManager.SetTurn();
        });  
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        /*
        여기는 데이터 베이스와 연동하여 초상화 이미지 초기화가 들어가야함
        */
        m_unitPortraitPair.Clear();
        int i = 0;
        foreach (var character in characterManager.Character)
        {
            UnitPortraitPair pair = new UnitPortraitPair();
            pair.s_portrait = OrderByTurnSpeedImage[i];
            pair.s_unit = character;
            m_unitPortraitPair.Add(pair);
            i++;
        }
        foreach (var monster in monsterManager.Monster)
        {
            UnitPortraitPair pair = new UnitPortraitPair();
            pair.s_portrait = OrderByTurnSpeedImage[i];
            pair.s_unit = monster;
            m_unitPortraitPair.Add(pair);
            i++;
        }
    }

    public void Synchronization(BaseManager baseManager)
    {
        if (Input.GetKeyDown(KeyCode.Escape)) { }

        if (baseManager is TurnManager turnManager)
        {
            for (int i = turnManager.Units.Count + 1; i < OrderByTurnSpeedImage.Count; i++)
            {
                OrderByTurnSpeedImage[i].gameObject.SetActive(false);
            }
            int j = 0;
            foreach(var unitPortraitPair in m_unitPortraitPair)
            {
                if(turnManager.CurrentTurnUnit.Equals(unitPortraitPair.s_unit))
                {
                    Debug.Log(unitPortraitPair.s_portrait.transform.parent.name);
                    unitPortraitPair.s_portrait.transform.SetAsFirstSibling();
                    j++;
                    Debug.Log("Front Unit :" + unitPortraitPair.s_portrait.name);
                    break;
                }
            }
            foreach (var unitdata in turnManager.Units)
            {
                foreach (var unitPortraitPair in m_unitPortraitPair)
                {
                    if (unitdata.Equals(unitPortraitPair.s_unit))
                    {
                        unitPortraitPair.s_portrait.transform.SetSiblingIndex(j);
                        j++;
                        Debug.Log(j + ":" + unitPortraitPair.s_portrait.name);
                    }
                }
            }
            SetTurnEtherInfo(turnManager);
        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    { 
        
    }

    public void SetTurnEtherInfo(TurnManager turnManager)
    {
        m_stringBuilder.Clear();
        m_stringBuilder
            .Append("Turn")
            .Append(turnManager.TurnCount)
            .Append("\n")
            .Append(turnManager.EtherCount)
            .Append(" / 15");

        m_turnText.text = m_stringBuilder.ToString();
    }
}
