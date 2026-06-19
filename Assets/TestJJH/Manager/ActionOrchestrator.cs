using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ActionOrchestrator
{
    private DontDestroyOnLoadManager m_dbLoader;

    public ActionOrchestrator()
    {
    }

    public void DataInitialize(DontDestroyOnLoadManager dbloader)
    {
        m_dbLoader = dbloader;
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
                contexts.Add(new UnitDyingActionContext());
                break;
            case TurnEndFlowInput Input:
                contexts.Add(new TurnEndActionContext());
                break;
            case SystemDrawCardFlowInput Input:
                CreateDrawCardContext(Input.Amount, contexts);
                break;
        }
        return contexts;
    }

    private void CreateSkillContext(Unit unit, Card card, List<ActionContext> contexts)
    {
        foreach (var id in card.CardData.SkillID)
        {
            BattleContext context = new BattleContext();

            SkillTableData SkillData = m_dbLoader.SkillTable(id);

            context.SkillData = SkillData;

            contexts.Add(context);
        }
    }

    private void CreateDrawCardContext(int amount, List<ActionContext> contexts)
    {
#if UNITY_EDITOR
        Debug.Log("###########액션 해석 시작###########");
#endif
        DrawCardActionContext context = new DrawCardActionContext();
        context.Amount = amount;
        contexts.Add(context);
    }
}
