using UnityEngine;
using Utils;

public class Temp : MonoBehaviour
{
    async void Start()
    {
        if(!SimpleSingleton<PrefabManager>.Instance.CheckLoadPrefab())
            await SimpleSingleton<PrefabManager>.Instance.LoadPrefab();
        SimpleSingleton<FactoryManager>.Instance.Init();
        Debug.Log(1);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(mousePosition);
            if (hit != null && hit.CompareTag("Unit"))
                return;

            int randomIndex = Random.Range(0, (int)EUnitType.Max);
            GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull((EUnitType)randomIndex);
            temp.transform.position = mousePosition;
        }
    }
}