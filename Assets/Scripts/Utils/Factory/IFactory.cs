using UnityEngine;

namespace Utils
{
    public interface IFactory
    {
        GameObject Create();
        void Register();
    }
}