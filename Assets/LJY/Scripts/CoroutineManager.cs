using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각 코루틴의 실행 상태 및 관련 정보를 담는 핸들 클래스
/// </summary>
public class CoroutineHandle
{
    public Coroutine RunningCoroutine { get; set; }
    public IEnumerator Enumerator { get; set; }
    public bool IsRunning { get; set; }
}

/// <summary>
/// 코루틴을 시작하고 관리하는 매니저 클래스
/// </summary>
public class CoroutineManager : MonoBehaviour
{
    // 각 코루틴을 고유 키 값으로 관리
    private Dictionary<string, CoroutineHandle> coroutineHandles;

    private void Awake()
    {
        coroutineHandles = new Dictionary<string, CoroutineHandle>();
    }

    /// <summary>
    /// 코루틴을 시작하여 관리함
    /// </summary>
    /// <param name="id">코루틴을 식별할 고유 키</param>
    /// <param name="coroutine">실행할 IEnumerator</param>
    /// <returns>>> 코루틴의 상태를 담은 핸들</returns>
    public CoroutineHandle StartManagedCoroutine(string id, IEnumerator coroutine)
    {
        List<string> keysToRemove = new List<string>();
        foreach (var kvp in coroutineHandles)
        {
            if (!kvp.Value.IsRunning)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (string key in keysToRemove)
        {
            coroutineHandles.Remove(key);
        }

        // 이미 동일한 id로 코루틴이 실행 중이라면 정리합니다.
        if (coroutineHandles.ContainsKey(id))
        {
            StopManagedCoroutine(id);
        }

        CoroutineHandle handle = new CoroutineHandle
        {
            Enumerator = coroutine,
            IsRunning = true
        };

        // 내부 코루틴으로 감싸서 실행 후 상태 업데이트
        handle.RunningCoroutine = StartCoroutine(RunCoroutine(id, coroutine, handle));
        coroutineHandles[id] = handle;
        return handle;
    }

    // 코루틴 실행 후 완료되면 상태를 변경
    private IEnumerator RunCoroutine(string id, IEnumerator coroutine, CoroutineHandle handle)
    {
        yield return coroutine;
        handle.IsRunning = false;
    }

    /// <summary>
    /// id로 실행 중인 코루틴을 중단
    /// </summary>
    public void StopManagedCoroutine(string id)
    {
        if (coroutineHandles.TryGetValue(id, out CoroutineHandle handle))
        {
            if (handle.IsRunning)
            {
                StopCoroutine(handle.RunningCoroutine);
                handle.IsRunning = false;
            }
            coroutineHandles.Remove(id);
        }
    }

    /// <summary>
    /// id로 코루틴의 상태 정보를 반환
    /// </summary>
    public CoroutineHandle GetCoroutineHandle(string id)
    {
        if (coroutineHandles.TryGetValue(id, out CoroutineHandle handle))
        {
            return handle;
        }
        return null;
    }
}
