using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StageEventData
{
    public string stageID;
    public StageType stageType;
    public int level;
}

public static class StageEvents
{
    // 스테이지 입장 시 (전투 씬 로드 전)
    public static event System.Action<StageEventData> OnStageEnter;

    // 스테이지 클리어 시
    public static event System.Action<StageEventData> OnStageClear;

    // 스테이지 맵 생성 완료 시
    public static event System.Action OnStageMapGenerated;

    public static void RaiseStageEnter(StageEventData data) => OnStageEnter?.Invoke(data);
    public static void RaiseStageClear(StageEventData data) => OnStageClear?.Invoke(data);
    public static void RaiseStageMapGenerated() => OnStageMapGenerated?.Invoke();
}
