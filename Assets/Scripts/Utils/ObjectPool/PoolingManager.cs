using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class PoolingManager
{
    private const int DefaultPoolSize = 50;

    private List<GameObject> _pooledObjects;
    public List<GameObject> PooledObjects
    {
        get { return _pooledObjects; }
    }

    private int _curIndex = 0;

    public PoolingManager(string prefabPath, string parentName, int poolSize = DefaultPoolSize)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject parent = new GameObject(parentName);


        _pooledObjects = new List<GameObject>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent.transform);
            obj.SetActive(false);            
            _pooledObjects.Add(obj);
        }
    }

    public PoolingManager(GameObject prefab, string parentName, int poolSize = DefaultPoolSize)
    {        
        GameObject parent = new GameObject(parentName);


        _pooledObjects = new List<GameObject>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent.transform);
            obj.SetActive(false);
            _pooledObjects.Add(obj);
        }
    }

    public PoolingManager(string prefabPath, GameObject parent, int poolSize = DefaultPoolSize) //Ãß°¡ÇÔ
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);


        _pooledObjects = new List<GameObject>(poolSize);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Object.Instantiate(prefab, parent.transform);
            obj.SetActive(false);
            _pooledObjects.Add(obj);
        }
    }
    public void AddComponent<T>() where T : Component
    {
        foreach (GameObject obj in _pooledObjects)
        {
            if (obj.GetComponent<T>() == null)
            {
                obj.AddComponent<T>();
            }
        }
    }

    public GameObject Pop()
    {
        GameObject popObject = _pooledObjects[GetCurIndex()];

        _curIndex = ++_curIndex % _pooledObjects.Count;

        popObject.SetActive(true);

        return popObject;
    }
    public List<GameObject> GetAllToActiveTrue()
    {
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < _pooledObjects.Count; i++)
        {
            if(_pooledObjects[i].activeSelf)
                list.Add(_pooledObjects[i]);
        }
        return list;
    }

    public GameObject GetClosestObject(Vector2 pos)
    {
        GameObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (GameObject obj in _pooledObjects)
        {
            if (obj.activeSelf)
            {
                float distance = Vector2.Distance(obj.transform.position, pos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = obj;
                }
            }
        }

        return closestObject;
    }

    private int GetCurIndex()
    {
        if (_pooledObjects[_curIndex].activeSelf == false)
        {
            return _curIndex;
        }

        _curIndex = ++_curIndex % _pooledObjects.Count;

        return GetCurIndex();
    }    
}
