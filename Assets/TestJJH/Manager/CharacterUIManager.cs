using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterUIManager : BaseManager, IsynchronizeUI
{
    [SerializeField]
    private Slider[] m_characterHealthPoint;
    private int s_AttackPosition;
    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
        s_AttackPosition = 50;
    }
    /// <summary>
    /// 
    /// </summary>
    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        for (int i = 0; i < characterManager.Character.Count; i++)
        {
            m_characterHealthPoint[i].maxValue = i+1;
            m_characterHealthPoint[i].value = m_characterHealthPoint[i].maxValue;
        }
        for (int i = characterManager.Character.Count; i < 4; i++)
        {
            m_characterHealthPoint[i].gameObject.SetActive(false);
        }
    }

    public void Synchronization(BaseManager baseManager)
    {
        if (baseManager is CharacterManager characterManager)
        {
            int a = 0;
            foreach (var character in characterManager.Character)
            {
                m_characterHealthPoint[a].value -= character.UserID;
                a++;
            }
        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager,  CardManager cardManager)
    {

    }

    public void SetHealthPoint(int position, CharacterManager characterManager)
    {
        m_characterHealthPoint[position].value--;
        //실제 데미지 UI 적용
        //m_characterHealthPoint[position].value = characterManager.Character[position].HealthPoint
    }
}
    