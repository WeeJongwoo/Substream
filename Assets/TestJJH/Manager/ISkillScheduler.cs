using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class FlowScheduler
{
    private LinkedList<Flow> m_flow;
    public void RegistFlow(Flow flow)
    {
        m_flow.AddLast(flow);
    }

    public Flow GetFlow()
    {
        Flow Flow = m_flow.First.Value;
        m_flow.RemoveFirst();
        return Flow;
    }

    public Flow GetFirstFlow()
    {
        return m_flow.First.Value;
    }

    public bool SkillQueueIsEmpty()
    {
        if (m_flow.Count == 0) return true;
        return false;
    }

    public FlowScheduler()
    {
        m_flow = new LinkedList<Flow>();
    }

    public void UnitDying(Unit unit)
    {
        var node = m_flow.First;

        while (node != null)
        {
            var next = node.Next;

            switch(node.Value.Input)
            {
                case CardAbilityFlowInput Input:

                    if (Input.CasterUnit.Equals(unit))
                    {
                        m_flow.Remove(node);
                    }

                    break;
                case UnitDyingFlowInput Input:

                    if (Input.Victim.Equals(unit))
                    {
                        m_flow.Remove(node);
                    }

                    break;
                case TurnEndFlowInput Input:
                    break;

                case SystemDrawCardFlowInput Input:
                    if (Input.CasterUnit.Equals(unit))
                    {
                        m_flow.Remove(node);
                    }
                    break;
            }
            node = next;
        }
    }
}