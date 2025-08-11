using UnityEngine;

namespace Utils
{
    public class MonoSingleton<T> where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    GameObject temp = new GameObject(typeof(T).Name);
                    _instance = temp.AddComponent<T>();
                    Object.DontDestroyOnLoad(temp);
                }
                return _instance;
            }
        }
    }
}