using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseManager : AccessableDataBase
{
    protected MasterManager m_masterManager;
    public abstract void Initialize();
    public virtual void InitializeReference(MasterManager masterManager)
    {
        m_masterManager = masterManager;
    }
    public virtual void DataInitialize()
    {
    }
    public virtual void Synchronization()
    {
    }
    public virtual void SetTurn()
    {
    }
    public bool ConnectsDataBase()
    {
        if (!ConnectDataBase())
        {
            return false;
        }
        return true;
    }
    public virtual void UseCard(Card card)
    {
    }
    public abstract void UnitDying(Unit unit);
}

interface IUpdatableManager
{
    void Execute();
}