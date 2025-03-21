using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private AsyncOperation asyncLoader = null;
    [HideInInspector] public CoroutineHandle coroutineHandle = null;
    [HideInInspector] public CoroutineManager coroutineManager;

    private void Awake()
    {
        if (coroutineManager == null)
        {
            coroutineManager = GetComponent<CoroutineManager>();
        }

        if (coroutineManager == null)
        {
            Debug.LogError("CoroutineManager가 할당되지 않았습니다");
        }
    }

    /// <summary>
    /// 다른 씬으로의 비동기 전환을 시작함
    /// </summary>
    /// <param name="name">전환되는 씬 이름</param>
    public void StartLoadingScene(string name)
    {
        coroutineHandle = coroutineManager.StartManagedCoroutine(name, StartLoading(name));
    }

    private IEnumerator StartLoading(string name)
    {
        asyncLoader = SceneManager.LoadSceneAsync(name);
        if (asyncLoader == null) yield break;

        while (asyncLoader.isDone == false)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 로딩되는 씬의 진행도를 로더로부터 전달
    /// </summary>
    /// <returns>>> 진행도</returns>
    public float GetLoadingProgress()
    {
        return asyncLoader.progress;
    }
}
