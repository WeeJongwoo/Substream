using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : BaseManager
{
    private LinkedList<Unit> m_monster;

    public LinkedList<Unit> Monster
    {
        get { return m_monster; }
    }
    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;

        m_monster = new LinkedList<Unit>();
    }

    public override void DataInitialize(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager)
    {
        for (int i = 0; i < 1; i++)
        {
            MonsterData monster = new MonsterData();
            monster.UserID = 0;
            monster.PrototypeUnitID = i;
            monster.InstanceID = 0;
            monster.Speed = 9;

            
            m_monster.AddLast(monster);
        }
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
       
    }

    public void SetHealthPoint(int position, float damage)
    {

    }
}
