// StageBinder.cs (붙여넣고 StageRoot에 추가)
//using UnityEngine;
//public class StageBinder : MonoBehaviour
//{
//    void Awake()
//    {
//        if (MapManager.Instance != null)
//            MapManager.Instance.BindStageRoot(transform);

//        if (!MapManager.Instance.IsReady)
//            Debug.LogError("[StageBinder] Bind 실패: IsReady=false");
//        else
//            Debug.Log("[StageBinder] Bind OK → " + transform.name);
//    }
//}

using System.Collections;
using UnityEngine;

public class StageBinder : MonoBehaviour
{
    private IEnumerator Start()
    {
        // MapManager가 씬에 생성될 때까지 1프레임씩 대기
        MapManager mm = null;
        while ((mm = MapManager.Instance) == null) yield return null;

        // 이 오브젝트(=Grid)를 스테이지로 바인딩
        mm.BindStageRoot(transform);

        if (!mm.IsReady)
            Debug.LogError("[StageBinder] Bind 실패: IsReady=false");
        else
            Debug.Log("[StageBinder] Bind OK → " + transform.name);
    }
}