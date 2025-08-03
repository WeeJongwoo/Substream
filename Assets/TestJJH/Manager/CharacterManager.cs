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
    /// 1. DontDestroyOnLoadManager의 Party 정보를 가져옴
    /// 2. party에 속한 key 값 {UserID, PrototypeCharacterID, InstanceID}를 모두 가져옴
    /// 3. 가져온 key 값으로 DontDestroyOnLoadManager에서 찾아낸 다음 m_character로 모두 enqueue
    /// 4. 속도에 관한 기획이 추가되었다면 속도 값에 맞게 정렬 후 enqueue
    /// </summary>
    public override void DataInitialize(TurnManager turnManager, CharacterManager charcterManager, MonsterManager monsterManager)
    {
        m_partyNumber = 4;
        // 임시 파티 데이터 : 0,0,0,0 4명
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
        Debug.Log($"Character[{position}] 가 {damage}의 데미지를 입음");
        //실제 대미지 적용
        /*m_character[position].HealthPoint -= damage;
        if (m_character[position].HealthPoint < 0)
        {
            m_character.RemoveAt(position);
        }*/
    }
}
