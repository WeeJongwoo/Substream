using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public abstract class BaseUI : BaseManager
{
}

public abstract class BaseUI<TSystem> 
    : BaseUI where TSystem : BaseSystem
{
    protected TSystem m_model;
    public void Bind(TSystem targetSystem)
    {
        m_model = targetSystem;
        m_model.OnSynchronization += () => Synchronization();
    }
}
