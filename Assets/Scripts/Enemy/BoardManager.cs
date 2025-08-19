using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private GameObject[] monsterPrefabs;
    [SerializeField] private int boardWidth = 5;
    [SerializeField] private int boardHeight = 5;
    [SerializeField] private float cellSize = 2.5f;
    [SerializeField] private Vector3 boardOrigin = new Vector3(-5f, -4.2f, 0f);

    [SerializeField] private float resolveDelay = 0.05f; 

    private Monster[,] grid;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SpawnMonsters();
    }

    private void SpawnMonsters()
    {
        grid = new Monster[boardHeight, boardWidth];

        for (int y = 0; y < boardHeight; y++)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Vector3 pos = boardOrigin + new Vector3(x * cellSize, y * cellSize, 0f);
                int idx = Random.Range(0, monsterPrefabs.Length);
                GameObject go = Instantiate(monsterPrefabs[idx], pos, Quaternion.identity, transform);
                Monster m = go.GetComponent<Monster>();
                m.SetCoords(y, x);
                grid[y, x] = m;
            }
        }
    }

    public void OnMonsterOutlineChanged(int r, int c)
    {
        if (grid == null) return;

        var toKill = new HashSet<Monster>();

        if (IsRowComplete(r)) AddRow(toKill, r);
        if (IsColComplete(c)) AddCol(toKill, c);

        if (r == c && IsMainDiagComplete()) AddMainDiag(toKill);
        if (r + c == boardWidth - 1 && IsAntiDiagComplete()) AddAntiDiag(toKill);

        if (toKill.Count > 0) StartCoroutine(ResolveLines(toKill));
    }

    private IEnumerator ResolveLines(HashSet<Monster> set)
    {
        yield return new WaitForSeconds(resolveDelay);
        foreach (var m in set)
            if (m) m.PlayDeathAndDisable();
    }

    private bool IsRowComplete(int r)
    {
        for (int x = 0; x < boardWidth; x++)
            if (grid[r, x] == null || !grid[r, x].IsCountedForBingo) return false;
        return true;
    }

    private bool IsColComplete(int c)
    {
        for (int y = 0; y < boardHeight; y++)
            if (grid[y, c] == null || !grid[y, c].IsCountedForBingo) return false;
        return true;
    }

    private bool IsMainDiagComplete()
    {
        int n = Mathf.Min(boardWidth, boardHeight);
        for (int i = 0; i < n; i++)
            if (grid[i, i] == null || !grid[i, i].IsCountedForBingo) return false;
        return true;
    }

    private bool IsAntiDiagComplete()
    {
        int n = Mathf.Min(boardWidth, boardHeight);
        for (int i = 0; i < n; i++)
            if (grid[i, n - 1 - i] == null || !grid[i, n - 1 - i].IsCountedForBingo) return false;
        return true;
    }

    
    private void AddRow(HashSet<Monster> set, int r)
    {
        for (int x = 0; x < boardWidth; x++) if (grid[r, x]) set.Add(grid[r, x]);
    }
    private void AddCol(HashSet<Monster> set, int c)
    {
        for (int y = 0; y < boardHeight; y++) if (grid[y, c]) set.Add(grid[y, c]);
    }
    private void AddMainDiag(HashSet<Monster> set)
    {
        int n = Mathf.Min(boardWidth, boardHeight);
        for (int i = 0; i < n; i++) if (grid[i, i]) set.Add(grid[i, i]);
    }
    private void AddAntiDiag(HashSet<Monster> set)
    {
        int n = Mathf.Min(boardWidth, boardHeight);
        for (int i = 0; i < n; i++) if (grid[i, n - 1 - i]) set.Add(grid[i, n - 1 - i]);
    }
}
