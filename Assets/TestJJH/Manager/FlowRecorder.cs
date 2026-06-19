using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowRecorder
{
    private List<FlowResultCollector> m_records;

    public FlowRecorder()
    {
        m_records = new List<FlowResultCollector>();
    }

}
