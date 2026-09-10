using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FlowRecorder
{
    private List<UnitRecordInfo> m_initialUnits;

    // 모든 기록
    private List<List<List<FlowRecord>>> m_allResultRecorder;

    public FlowRecorder()
    {
        m_initialUnits = new List<UnitRecordInfo>();
        m_allResultRecorder = new List<List<List<FlowRecord>>>();
        m_allResultRecorder.Add(new List<List<FlowRecord>>());
        m_allResultRecorder[0].Add(new List<FlowRecord>());
    }

    public void Initialize(CharacterManager characters, MonsterManager monsters)
    {
        foreach (var unit in characters.Units) 
        {
            var NewUnitRecordInfo = new UnitRecordInfo();
            NewUnitRecordInfo.UnitID = unit.IngameUnitID();
            NewUnitRecordInfo.Position = unit.Position;
            NewUnitRecordInfo.IsCharacter = unit.IsCharacter;
            m_initialUnits.Add(NewUnitRecordInfo);
        }
        foreach (var unit in monsters.Units)
        {
            var NewUnitRecordInfo = new UnitRecordInfo();
            NewUnitRecordInfo.UnitID = unit.IngameUnitID();
            NewUnitRecordInfo.Position = unit.Position;
            NewUnitRecordInfo.IsCharacter = unit.IsCharacter;
            m_initialUnits.Add(NewUnitRecordInfo);
        }
    }

    public void SaveRecord(Flow flow)
    {
        m_allResultRecorder[^1][^1].Add(CreateRecord(flow));
    }

    public void SetTurn()
    {
        var newTurn = new List<FlowRecord>();
        m_allResultRecorder[^1].Add(newTurn);
    }

    public void SetRound()
    {
        var newRound = new List<List<FlowRecord>>();
        newRound.Add(new List<FlowRecord>());
        m_allResultRecorder.Add(newRound);
    }

    private FlowRecord CreateRecord(Flow flow)
    {
        FlowRecord record = new FlowRecord();

        record.FlowID = flow.ID;
        record.Results = flow.Collector;

        switch (flow.Input)
        {
            case CardAbilityFlowInput input:
                record.Source.SourceType = EFlowSourceType.E_UNIT;

                record.Source.Unit = new UnitRecordInfo
                {
                    UnitID = input.CasterUnit.IngameUnitID(),
                    Position = input.CasterUnit.Position,
                    IsCharacter = input.CasterUnit.IsCharacter
                };

                record.Source.CardID = input.CasterCard.CardData.ID;
                break;

            case SkillAbilityFlowInput input:
                record.Source.SourceType = EFlowSourceType.E_UNIT;

                record.Source.Unit = new UnitRecordInfo
                {
                    UnitID = input.CasterUnit.IngameUnitID(),
                    Position = input.CasterUnit.Position,
                    IsCharacter = input.CasterUnit.IsCharacter
                };

                record.Source.CardID = input.CasterCard.CardData.ID;
                break;

            case SystemDrawCardFlowInput input:
                record.Source.SourceType = EFlowSourceType.E_SYSTEM;

                record.Source.Unit = new UnitRecordInfo
                {
                    UnitID = input.CasterUnit.IngameUnitID(),
                    Position = input.CasterUnit.Position,
                    IsCharacter = input.CasterUnit.IsCharacter
                };
                break;

            case UnitDyingFlowInput input:
                record.Source.SourceType = EFlowSourceType.E_SYSTEM;
                break;

            case TurnEndFlowInput:
                record.Source.SourceType = EFlowSourceType.E_SYSTEM;
                break;

            case RoundEndFlowInput:
                record.Source.SourceType = EFlowSourceType.E_SYSTEM;
                break;
        }

        return record;
    }
}
