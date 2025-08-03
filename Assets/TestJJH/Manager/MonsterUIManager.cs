using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MonsterUIManager : BaseManager, IsynchronizeUI
{
    [SerializeField]
    private Slider[] m_characterHealthPoint;
    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {

    }
    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {

    }

    public void Synchronization(BaseManager baseManager)
    {
        if (baseManager is CharacterManager MonsterManager)
        {

        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {

    }

    public void SetHealthPoint(int position, CharacterManager characterManager)
    {

    }
}
