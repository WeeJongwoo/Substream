using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Monster
{
    private MonsterData m_monsterData;

    public void Initialize(MonsterData monsterData)
    {
        m_monsterData = monsterData;
    }

    public MonsterData MonsterData
    {
        get { return m_monsterData; }
        set { m_monsterData = value; }
    }

    public void Execute()
    {
        Debug.Log("Monster Turn: " + m_monsterData.PrototypeUnitID + " " + m_monsterData.InstanceID + " " + Random.Range(1, 4) + "에게" + m_monsterData.Speed + "만큼 데미지를 입힘");
    }
}