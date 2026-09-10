using System.Collections.Generic;
using UnityEngine;

public interface IEffectTask
{
    /// <summary>
    /// 매 프레임 스케줄러에 의해 호출.
    /// 로직 및 연출이 완전히 종료되었을 때 true를 반환
    /// </summary>
    bool ExecuteAndCheckCompletion();
}

/// <summary>
/// 다양한 효과의 적용을 스케쥴링 함
/// </summary>
public class EffectScheduler : MonoBehaviour
{
    public static EffectScheduler Instance { get; private set; }

    /// <summary>
    /// 효과들이 대기하는 큐
    /// </summary>
    private Queue<IEffectTask> _taskQueue = new Queue<IEffectTask>();

    /// <summary>
    /// 현재 실행 중인 태스크
    /// </summary>
    private IEffectTask _currentTask = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 새로운 효과를 큐의 맨 뒤에 예약
    /// </summary>
    public void Schedule(IEffectTask task)
    {
        _taskQueue.Enqueue(task);
    }

    private void Update()
    {
        // 현재 실행 중인 태스크가 없다면 대기열에서 하나 꺼냄
        if (_currentTask == null && _taskQueue.Count > 0) {
            _currentTask = _taskQueue.Dequeue();
        }

        // 현재 태스크가 있다면 실행하고 완료 여부를 체크
        // 종료된 상태라면 실행 Task를 비워줌
        if (_currentTask != null) {
            bool isFinished = _currentTask.ExecuteAndCheckCompletion();

            if (isFinished) {
                _currentTask = null;
            }
        }
    }
}