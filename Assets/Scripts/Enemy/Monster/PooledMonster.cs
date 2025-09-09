using UnityEngine;

[DisallowMultipleComponent]
public sealed class PooledMonster : MonoBehaviour
{
    public MonsterManager Manager { get; set; }
    public MonsterMover PrefabKey { get; set; }

    private bool _suppressNextDisable = false;
    private bool _isQuitting = false;

    public void SuppressReturnOnce()
    {
        _suppressNextDisable = true;
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }

    private void OnDisable()
    {
        if (_isQuitting) { return; }

        // 프리웜/수동 디스폰에서 호출되는 OnDisable 재진입 차단
        if (_suppressNextDisable)
        {
            _suppressNextDisable = false; // 1회성 억제
            return;
        }

        if (Manager != null && PrefabKey != null)
        {
            // 비활성화 "중"에는 SetParent/SetActive 등을 만지지 않고,
            // 다음 프레임에 MonsterManager가 안전하게 반환 처리하도록 위임
            Manager.EnqueueReturn(this);
        }
    }
}