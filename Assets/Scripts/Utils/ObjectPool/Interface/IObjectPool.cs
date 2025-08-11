using System;
using UnityEngine;

namespace Utils
{
    public interface IObjectPool
    {
        void Init(Transform parent);
        GameObject Pull(Enum type);
        void Push(Enum type, GameObject obj);
    }
}