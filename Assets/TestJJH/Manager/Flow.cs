using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FlowRecord
{

}

public class Flow
{
    public FlowInput Input;

    public int FlowID;

    public List<FlowRecord> Record;
    public List<ActionContext> ActionContext;

    public Flow(FlowInput flowInput)
    {
        Input = flowInput;
    
        ActionContext = new List<ActionContext>();
        Record = new List<FlowRecord>();
    }
}