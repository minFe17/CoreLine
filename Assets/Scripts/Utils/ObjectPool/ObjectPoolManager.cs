using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class ObjectPoolManager : MonoBehaviour
    {
        // 싱글턴
        Dictionary<Type, IObjectPool> _objectPools = new Dictionary<Type, IObjectPool>();

        ObjectPool<TEnum> GetOrCreatePool<TEnum>() where TEnum : Enum
        {
            Type key = typeof(TEnum);

            if (_objectPools.TryGetValue(key, out IObjectPool pool))
                return pool as ObjectPool<TEnum>;

            ObjectPool<TEnum> newPool = new ObjectPool<TEnum>();
            newPool.Init(transform);
            _objectPools[key] = newPool;
            return newPool;
        }

        public void RegisterFactory<TEnum>(TEnum type, IFactory factory) where TEnum : Enum
        {
            GetOrCreatePool<TEnum>().RegisterFactory(type, factory);
        }

        public GameObject Pull
            <TEnum>(TEnum type) where TEnum : Enum
        {
            return GetOrCreatePool<TEnum>().Pull(type);
        }

        public void Push<TEnum>(TEnum type, GameObject obj) where TEnum : Enum
        {
            GetOrCreatePool<TEnum>().Push(type, obj);
        }
    }
}
