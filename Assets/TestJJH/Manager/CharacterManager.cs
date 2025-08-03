using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : BaseManager
{
    private LinkedList<CharacterData> m_character;

    private int m_partyNumber;

    public LinkedList<CharacterData> Character
    {
        get { return m_character; }
    }

    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;

        m_character = new LinkedList<CharacterData>();
    }

    /// <summary>
    /// 1. DontDestroyOnLoadManager�� Party ������ ������
    /// 2. party�� ���� key �� {UserID, PrototypeCharacterID, InstanceID}�� ��� ������
    /// 3. ������ key ������ DontDestroyOnLoadManager���� ã�Ƴ� ���� m_character�� ��� enqueue
    /// 4. �ӵ��� ���� ��ȹ�� �߰��Ǿ��ٸ� �ӵ� ���� �°� ���� �� enqueue
    /// </summary>
    public override void DataInitialize(TurnManager turnManager, CharacterManager charcterManager, MonsterManager monsterManager)
    {
        m_partyNumber = 4;
        // �ӽ� ��Ƽ ������ : 0,0,0,0 4��
        var key = (0, 0, 0);
        m_character.AddLast(DontDestroyOnLoadManager.Instance.Character.Character[key]);
        key = (0, 0, 1);
        m_character.AddLast(DontDestroyOnLoadManager.Instance.Character.Character[key]);
        key = (0, 0, 2);
        m_character.AddLast(DontDestroyOnLoadManager.Instance.Character.Character[key]);
        key = (0, 0, 3);
        m_character.AddLast(DontDestroyOnLoadManager.Instance.Character.Character[key]);
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {

    }

    public void SetHealthPoint(int position, float damage)
    {
        Debug.Log($"Character[{position}] �� {damage}�� �������� ����");
        //���� ����� ����
        /*m_character[position].HealthPoint -= damage;
        if (m_character[position].HealthPoint < 0)
        {
            m_character.RemoveAt(position);
        }*/
    }
}
