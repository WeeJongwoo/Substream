using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillScheduler
{
    private LinkedList<List<ActionContext>> m_skillList;
    public void RegistSkill(List<ActionContext> skills)
    {
        m_skillList.AddLast(skills);
    }

    public void RegistSkill(ActionContext skill)
    {
        List<ActionContext> SkillList = new List<ActionContext>
        {
            skill
        };
        m_skillList.AddLast(SkillList);
    }

    public List<ActionContext> GetSkill()
    {
        List<ActionContext> skills = m_skillList.First.Value;
        m_skillList.RemoveFirst();
        return skills;
    }

    public bool SkillQueueIsEmpty()
    {
        if (m_skillList.Count == 0) return true;
        return false;
    }

    public SkillScheduler()
    {
        m_skillList = new LinkedList<List<ActionContext>>();
    }

    public void UnitDying(Unit unit)
    {
        var node = m_skillList.First;

        while (node != null)
        {
            var next = node.Next;

            if (node.Value.First().CasterUnit.Equals(unit))
            {
                m_skillList.Remove(node);
            }

            node = next;
        }
    }
}