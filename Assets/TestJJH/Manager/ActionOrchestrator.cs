using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ActionOrchestrator
{
    public ActionOrchestrator()
    {

    }

    public List<ActionContext> AnalyzeFlow(Flow flow)
    {
        List<ActionContext> contexts = new List<ActionContext>();
        switch(flow.Input)
        {
            case AbilityFlowInput Input:
                CreateSkillContext(Input.CasterUnit, Input.CasterCard, contexts);
                break;
            case UnitDyingFlowInput Input:
                contexts.Add(new ActionContext());
                break;
            case TurnEndFlowInput Input:
                contexts.Add(new ActionContext());
                break;
        }
        return contexts;
    }

    private void CreateSkillContext(Unit unit, Card card, List<ActionContext> contexts)
    {
#if UNITY_EDITOR
        Debug.Log("###########액션 해석 시작###########");
#endif
        contexts = new List<ActionContext>();

        foreach (var id in card.CardData.SkillID)
        {
            ActionContext context = new ActionContext();
            context.SkillData = DontDestroyOnLoadManager.Instance.SkillTable(id);
            contexts.Add(context);
        }
    }
}
