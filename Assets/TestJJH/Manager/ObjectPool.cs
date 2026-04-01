using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool
{
    private int _maxCount;

    private GameObject _prefab;

    private Queue<GameObject> _queue;

    private GameObject _caching;
    public int Count { get { return _queue.Count; } }

    private Transform _parent;

    public ObjectPool(GameObject obj, int maxCount, Transform parent)
    {
        _prefab = obj;
        _queue = new Queue<GameObject>(maxCount);
        _maxCount = maxCount;
        _parent = parent;

        for(int i = 0; i < _maxCount; i++)
        {
            ReleaseObject(Object.Instantiate(obj));
        }
    }

    public ObjectPool(GameObject obj)
    {
        _prefab = obj;
        _queue = new Queue<GameObject>();
        _maxCount = int.MaxValue;
    }

    public GameObject GetObject()
    {
        if (_queue.Count == 0)
        {
            _caching = Object.Instantiate(_prefab);
            return _caching;
        }

        _caching = _queue.Dequeue();
        _caching.gameObject.SetActive(true);
        return _caching;
    }

    public bool ReleaseObject(GameObject obj)
    {
        if (_maxCount <= _queue.Count) return false;

        obj.gameObject.SetActive(false);
        _queue.Enqueue(obj);
        obj.transform.parent = _parent;
        return true;
    }
}

public class ObjectPool<T> where T : UIObject
{
    private int _maxCount;

    private T _prefab;

    private Queue<T> _queue;

    private T _caching;
    public int Count { get { return _queue.Count; } }

    private Transform _parent;

    public ObjectPool(T obj, int maxCount, Transform parent)
    {
        _prefab = obj;
        _queue = new Queue<T>(maxCount);
        _maxCount = maxCount;
        _parent = parent;

        for (int i = 0; i < _maxCount; i++)
        {
            ReleaseObject(Object.Instantiate(obj));
        }
    }

    public ObjectPool(T obj, int maxCount)
    {
        _prefab = obj;
        _queue = new Queue<T>(maxCount);
        _maxCount = maxCount;
    }

    public ObjectPool(T obj)
    {
        _prefab = obj;
        _queue = new Queue<T>();
        _maxCount = int.MaxValue;
    }

    public T GetObject()
    {
        if (_queue.Count == 0)
        {
            _caching = Object.Instantiate(_prefab);
            return _caching;
        }

        _caching = _queue.Dequeue();
        _caching.gameObject.SetActive(true);
        return _caching;
    }

    public bool ReleaseObject(T obj) 
    {
        if (_maxCount <= _queue.Count)
        {
            Debug.LogError("오브젝트 풀 오류 발생");
            return false;
        }

        obj.gameObject.SetActive(false);
        _queue.Enqueue(obj);
        obj.transform.parent = _parent;

        return true;
    }
}
