using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : BaseManager
{
    private List<UnitData> m_character;
    private UnitData m_currentTurnCharacter;

    private int m_partyNumber;
    public UnitData CurrentTurnCharacter
    {
        get { return m_currentTurnCharacter; }
    }

    public List<UnitData> Character
    {
        get { return m_character; }
    }

    public override void Initialize(MasterManager masterManager, TurnManager turnManager)
    {
        m_masterManager = masterManager;
    }

    /// <summary>
    /// ���� �ڵ�
    /// 1. DontDestroyOnLoadManager�� Party ������ ������
    /// 2. party�� ���� key �� {UserID, PrototypeCharacterID, InstanceID}�� ��� ������
    /// 3. ������ key ������ DontDestroyOnLoadManager���� ã�Ƴ� ���� m_character�� ��� enqueue
    /// 4. �ӵ��� ���� ��ȹ�� �߰��Ǿ��ٸ� �ӵ� ���� �°� ���� �� enqueue
    /// </summary>
    public override void DataInitialize(TurnManager turnManager, CharacterManager charcterManager)
    {
        m_partyNumber = 4;
        m_character = new List<UnitData>(m_partyNumber);
        // �ӽ� ��Ƽ ������ : 0,0,0 4��
        for (int i =0;i<m_partyNumber;i++)
        {
            var key = (0, 0, 0);
            m_character.Add(DontDestroyOnLoadManager.Instance.Unit.Unit[key]);
        }

        // ���� ���� ĳ���� ����
        m_currentTurnCharacter = m_character[0];
    }

    public override void SetTurn(TurnManager turnManager, CharacterManager characterManager, CardManager cardManager)
    {
        m_currentTurnCharacter = m_character[(turnManager.TurnCount - 1) % m_partyNumber];
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
