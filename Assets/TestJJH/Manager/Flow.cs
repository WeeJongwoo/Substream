using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public static class FlowIdGenerator
{
    private static int _id = 0;

    /// <summary>
    /// 전역 고유 ID를 발행한다. (Thread-Safe)
    /// </summary>
    public static int Next()
    {
        return Interlocked.Increment(ref _id);
    }

    /// <summary>
    /// 필요 시 초기화 (테스트/리플레이 용)
    /// </summary>
    public static void Reset(int start = 0)
    {
        Interlocked.Exchange(ref _id, start);
    }
}

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

    public int ResultCount => m_contextResults.Count;

    public void SetSourceSkillID(int startIndex, int skillID)
    {
        for (int i = startIndex; i < m_contextResults.Count; i++)
        {
            m_contextResults[i].SourceSkillID = skillID;
        }
    }
}

public class Flow
{
    public readonly int ID;
    private FlowScheduleManager m_manager;

    public bool CanTriggerCounter;
    public bool CanTriggerPassive;

    public FlowInput Input;

    private readonly FlowResultCollector m_resultCollector;

    public FlowResultCollector Collector => m_resultCollector;

    public float TotalDamage
    {
        get { return m_resultCollector.GetResults<ChangeHPResult>().Sum(r => r.IsDamage?r.Amount:0); }
    }

    /*
    public int ConsumeStatusEffect
    {
        get { return m_resultCollector.GetResults<ChangeStackResult>().Sum(r => r.Stack < 0 ? r.Amount : 0); }
    }*/

    public Flow(FlowInput flowInput)
    {
        Input = flowInput;

        m_resultCollector = new FlowResultCollector();
        ID = FlowIdGenerator.Next();
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