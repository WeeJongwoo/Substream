using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageData
{
    public List<List<StageNode>> stages;
    public int currentLevel;
    public StageNode currentStage;
    public int maxLevel;
    public bool isInitialized;

}
