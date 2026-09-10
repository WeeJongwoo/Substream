using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseManager : AccessableDataBase
{
    protected MasterManager m_masterManager;

    public abstract void Initialize(MasterManager masterManager, TurnManager turnManager);

    // 데이터 초기화시 호출
    public virtual void DataInitialize(TurnManager turnManager, CharacterManager characterManager , MonsterManager monsterManager)
    {
    }

    // 턴 종료시 호출
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