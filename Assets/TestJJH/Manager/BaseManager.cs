using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseManager : MonoBehaviour
{
    protected MasterManager m_masterManager;
    
    // ���� �ʱ�ȭ�� ȣ��
    public abstract void Initialize(MasterManager masterManager, TurnManager turnManager);

    // ������ �ʱ�ȭ�� ȣ��
    public virtual void DataInitialize(TurnManager turnManager, CharacterManager characterManager , MonsterManager monsterManager)
    {
    }

    // �� ����� ȣ��
    public virtual void SetTurn(TurnManager turnManager, CharacterManager characterManager, MonsterManager monsterManager, CardManager cardManager)
    {
    }
}

interface IsynchronizeUI
{
    void Synchronization(BaseManager baseManager);
}


interface IUpdatableManager
{
    void Execute();
}