using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FlowResultCollector
{
    private List<ContextResult> m_contextResults = new List<ContextResult>();

    public void Collect(ContextResult contextResult)
    {
        m_contextResults.Add(contextResult);
    }

    public IEnumerable<T> GetResults<T>() where T : ContextResult
    {
        return m_contextResults.OfType<T>();
    }

    public List<ContextResult> Results
    {
        get { return m_contextResults; }
    }
}

public class Flow
{
    private FlowScheduleManager m_manager;

    public bool CanTriggerCounter;
    public bool CanTriggerPassive;

    public FlowInput Input;

    public int FlowID;

    private readonly FlowResultCollector m_resultCollector;

    public FlowResultCollector Results => m_resultCollector;

    public List<ContextResult> ContextResults
    {
        get
        {
            return m_resultCollector.Results;
        }
    }

    public float TotalDamage
    {
        get { return m_resultCollector.GetResults<ChangeHPResult>().Sum(r => r.IsDamage?r.Amount:0); }
    }

    public Flow(FlowInput flowInput)
    {
        Input = flowInput;

        m_resultCollector = new FlowResultCollector();
    }

    public void Record(ContextResult contextResult)
    {
        m_resultCollector.Collect(contextResult);
    }

    public void CreateCounterFlow()
    {

    }

    public void CreatePassiveFlow()
    {

    }
}