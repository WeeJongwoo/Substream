using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ContextGenerator
{
    private DontDestroyOnLoadManager m_dbLoader;


    private ContextWriterFactory m_writerFactory;
    public ContextGenerator()
    {
        m_writerFactory = new ContextWriterFactory();
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
            case CardAbilityFlowInput Input:
                foreach (var id in Input.CasterCard.CardData.SkillID)
                {
                    SkillTableData SkillData = m_dbLoader.SkillTable(id);
                    BattleContext bc = new BattleContext();
                    bc.SkillData = SkillData;
                    contexts.Add(bc);
                    m_writerFactory.Write(flow, bc);
                }
                break;
            case UnitDyingFlowInput Input:
                UnitDyingActionContext udac = new UnitDyingActionContext();
                contexts.Add(udac);
                m_writerFactory.Write(flow, udac);
                break;
            case TurnEndFlowInput Input:
                TurnEndActionContext teac = new TurnEndActionContext();
                contexts.Add(teac);
                m_writerFactory.Write(flow, teac);
                break;
            case RoundEndFlowInput Input:
                RoundEndActionContext reac = new RoundEndActionContext();
                contexts.Add(reac);
                m_writerFactory.Write(flow, reac);
                break;
            case SystemDrawCardFlowInput Input:
                DrawCardActionContext dcac = new DrawCardActionContext();
                dcac.Amount = Input.Amount;
                contexts.Add(dcac);
                m_writerFactory.Write(flow, dcac);
                break;
        }
        return contexts;
    }
}
