// Assets/Scripts/PADDownloadUI.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TMPro;

public class PADDownloadUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;
    public Slider progressBar;
    public TMP_Text progressText;
    public Button cancelBtn;

    [Header("Options")]
    [Tooltip("모바일 데이터에서는 받지 않고 Wi-Fi에서만 다운로드")]
    public bool requireWifi = false;

    [Tooltip("다운로드 예상치 외에 추가로 확보할 여유 공간(MB)")]
    public int reserveSpaceMB = 100;

    [Tooltip("UI 갱신 간격(초). 너무 자주 갱신하면 성능이 아까움")]
    public float uiUpdateInterval = 0.1f;

    bool _cancelRequested;

    // --- 네트워크/저장공간 유틸 ---
    bool Online => Application.internetReachability != NetworkReachability.NotReachable;
    bool OnWifi => Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

#if UNITY_ANDROID && !UNITY_EDITOR
    long GetAvailableBytes()
    {
        using (var statFs = new AndroidJavaObject("android.os.StatFs", Application.persistentDataPath))
        {
            return statFs.Call<long>("getAvailableBytes");
        }
    }
#else
    long GetAvailableBytes() => long.MaxValue; // 에디터/타 플랫폼: 체크 스킵
#endif

    void Awake()
    {
        if (cancelBtn) cancelBtn.onClick.AddListener(() => _cancelRequested = true);
        if (progressBar) progressBar.interactable = false;
        if (root) root.SetActive(false);
    }

    public IEnumerator EnsureReadyByLabels(IEnumerable<object> labels)
    {
        // 0) 네트워크 선체크
        if (!Online)
        {
            Show("네트워크 연결을 확인해 주세요.");
            yield return new WaitForSeconds(1.0f);
            Hide();
            yield break;
        }
        if (requireWifi && !OnWifi)
        {
            Show("Wi-Fi에서만 다운로드할 수 있어요.");
            yield return new WaitForSeconds(1.0f);
            Hide();
            yield break;
        }

        // 1) 예상 다운로드 용량
        var sizeOp = Addressables.GetDownloadSizeAsync(labels);
        yield return sizeOp;
        long bytesNeeded = sizeOp.Status == AsyncOperationStatus.Succeeded ? sizeOp.Result : 0;

        if (bytesNeeded <= 0) yield break; // 이미 설치/캐시됨

        // 2) 저장공간 체크(+여유)
        long reserve = (long)(bytesNeeded * 0.10f) + (long)reserveSpaceMB * 1024L * 1024L; // 10% + 옵션 MB
        long avail = GetAvailableBytes();
        if (avail < bytesNeeded + reserve)
        {
            Show($"저장공간 부족\n필요: {FmtMB(bytesNeeded + reserve)} / 여유: {FmtMB(avail)}");
            yield return new WaitForSeconds(1.2f);
            Hide();
            yield break;
        }

        // 3) UI 준비
        _cancelRequested = false;
        root.SetActive(true);
        if (progressBar) progressBar.value = 0f;
        if (progressText) progressText.text = "Preparing...";

        // 4) 다운로드 시작
        var dl = Addressables.DownloadDependenciesAsync(labels, Addressables.MergeMode.Union, false);

        double lastBytes = 0;
        float lastT = Time.realtimeSinceStartup;
        float nextUiT = 0f;

        while (!dl.IsDone)
        {
            var st = dl.GetDownloadStatus(); // TotalBytes / DownloadedBytes
            float pct = st.TotalBytes > 0 ? (float)st.DownloadedBytes / st.TotalBytes : 0f;

            // 대략 속도(B/s)
            float nowT = Time.realtimeSinceStartup;
            double deltaB = st.DownloadedBytes - lastBytes;
            float deltaT = Mathf.Max(0.0001f, nowT - lastT);
            double speed = deltaB / deltaT;

            // UI는 일정 주기로만 갱신
            if (Time.realtimeSinceStartup >= nextUiT)
            {
                if (progressBar) progressBar.value = pct;
                if (progressText)
                    progressText.text = $"{FmtMB(st.DownloadedBytes)} / {FmtMB(st.TotalBytes)}  ({pct * 100f:0.#}%)  {FmtMB(speed)}/s";
                nextUiT = Time.realtimeSinceStartup + uiUpdateInterval;
            }

            lastBytes = st.DownloadedBytes;
            lastT = nowT;

            if (_cancelRequested)
            {
                Addressables.Release(dl);
                Hide();
                yield break;
            }

            
            if (!Online)
            {
                Show("네트워크가 끊겼어요. 다시 시도해 주세요.");
                yield return new WaitForSeconds(1.0f);
                Addressables.Release(dl);
                Hide();
                yield break;
            }

            yield return null;
        }

        if (dl.Status != AsyncOperationStatus.Succeeded)
        {
            Show("다운로드 실패. 다시 시도해 주세요.");
            yield return new WaitForSeconds(1.0f);
            Hide();
            yield break;
        }

        // 완료
        if (progressBar) progressBar.value = 1f;
        if (progressText) progressText.text = "Completed";
        yield return new WaitForSeconds(0.15f);
        Hide();
    }

    void Show(string msg)
    {
        if (root && !root.activeSelf) root.SetActive(true);
        if (progressText) progressText.text = msg;
    }

    void Hide()
    {
        if (root && root.activeSelf) root.SetActive(false);
    }

    static string FmtMB(double bytes) => $"{bytes / 1024f / 1024f:0.0} MB";
}
