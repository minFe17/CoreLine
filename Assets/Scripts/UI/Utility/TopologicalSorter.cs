using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public class TopologicalSorter<T>
{
    private Dictionary<T, List<T>> _graph = new();
    private Dictionary<T, int> _inDegree = new();

    // 노드 초기화: 그래프에 없는 노드라도 추가해야 함
    public void AddNode(T node)
    {
        if (!_graph.ContainsKey(node))
            _graph[node] = new List<T>();

        if (!_inDegree.ContainsKey(node))
            _inDegree[node] = 0;
    }

    // 간선 추가: from -> to
    public void AddEdge(T from, T to)
    {
        AddNode(from);
        AddNode(to);

        _graph[from].Add(to);
        _inDegree[to]++;
    }

    // 위상 정렬 수행
    public List<T> Sort()
    {
        var queue = new Queue<T>();

        // 진입 차수가 0인 노드를 큐에 넣기
        foreach (var kvp in _inDegree)
        {
            if (kvp.Value == 0)
                queue.Enqueue(kvp.Key);
        }

        var sortedList = new List<T>();

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            sortedList.Add(node);

            foreach (var neighbor in _graph[node])
            {
                _inDegree[neighbor]--;

                if (_inDegree[neighbor] == 0)
                    queue.Enqueue(neighbor);
            }
        }

        // 모든 노드를 방문하지 못했다면 사이클이 있음
        if (sortedList.Count != _graph.Count)
            throw new InvalidOperationException("그래프에 사이클이 존재합니다!");

        return sortedList;
    }
}