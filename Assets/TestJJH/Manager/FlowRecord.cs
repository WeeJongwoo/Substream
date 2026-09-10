using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EFlowSourceType
{
    E_NONE,
    E_UNIT,
    E_SYSTEM,

    // 추후 확장
    E_STATUS_EFFECT,
    E_RULE
}

public struct UnitRecordInfo
{
    public int UnitID;
    public int Position;
    public bool IsCharacter;
}

public struct FlowSourceRecord
{
    public EFlowSourceType SourceType;

    public UnitRecordInfo? Unit;

    public int CardID;
}


public class FlowRecord
{
    public int FlowID;

    public FlowSourceRecord Source;

    public FlowResultCollector Results;
}
