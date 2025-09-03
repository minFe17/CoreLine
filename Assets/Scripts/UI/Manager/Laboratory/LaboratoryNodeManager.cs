using UnityEngine;
using System.Collections.Generic;
using Utils;
using System.Linq;
using NUnit.Framework.Constraints;

public class LaboratoryNodeManager : MonoBehaviour 
{
    private Vector3 _startPosition = new Vector3(0, 0,0);
    private Dictionary<LaboratoryType, PoolingManager> _poolingNodes = new Dictionary<LaboratoryType, PoolingManager>();
    private Dictionary<LaboratoryType, PoolingManager> _poolingLines = new Dictionary<LaboratoryType, PoolingManager>();
    private Dictionary<LaboratoryType, List<LaboratoryNode>> _nodes = new();
    private Dictionary<LaboratoryType,TopologicalSorter<LaboratoryNode>> _sorter = new();
    private List<LineController> _lines = new List<LineController>();

    private void Start()
    {
        CreateNodes();
        SpawnNodes();
        SettingPosition();
        UpdateLine();
    }
    private void UpdateLine()
    {
        foreach(var line in _lines)
        {
            line.UpdateLine();
        }
    }
    private void CreateNodes()
    {
        string prefabsPath = "UI/Prefabs/Image/Line";
        string parentPath = "UI/LaboratoryPanel/NodePanel/Viewport/";

        _poolingLines[LaboratoryType.Attack] = new PoolingManager(prefabsPath, parentPath + "Attack", 20);
        _poolingLines[LaboratoryType.Defense] = new PoolingManager(prefabsPath, parentPath + "Defense", 20);
        _poolingLines[LaboratoryType.Utility] = new PoolingManager(prefabsPath, parentPath + "Utility", 20);


        prefabsPath = "UI/Prefabs/Button/Laboratory/LaboratoryNode";

        _poolingNodes[LaboratoryType.Attack] = new PoolingManager(prefabsPath, parentPath + "Attack", 20);
        _poolingNodes[LaboratoryType.Defense] = new PoolingManager(prefabsPath, parentPath + "Defense", 20);
        _poolingNodes[LaboratoryType.Utility] = new PoolingManager(prefabsPath, parentPath + "Utility", 20);

        _nodes[LaboratoryType.Attack] = new List<LaboratoryNode>();
        _nodes[LaboratoryType.Defense] = new List<LaboratoryNode>();
        _nodes[LaboratoryType.Utility] = new List<LaboratoryNode>();
    }
    private void SpawnNodes()
    {
        List<LaboratoryData> attackData = LaboratoryManager.Instance.GetData(LaboratoryType.Attack);
        List<LaboratoryData> defenseData = LaboratoryManager.Instance.GetData(LaboratoryType.Defense);
        List<LaboratoryData> utilityData = LaboratoryManager.Instance.GetData(LaboratoryType.Utility);

        SettingData(ref attackData, LaboratoryType.Attack);
        SettingData(ref defenseData, LaboratoryType.Defense);
        SettingData(ref utilityData, LaboratoryType.Utility);
    }
    private void SettingData(ref List<LaboratoryData> data, LaboratoryType type)
    {
        Dictionary<string, LaboratoryNode> parent = new Dictionary<string, LaboratoryNode>();
        for(int i=0;i<data.Count; i++)
        {
            LaboratoryNode node = _poolingNodes[type].Pop().GetComponent<LaboratoryNode>();
            node.Data = data[i];
            parent.Add(node.Data.Id, node);
        }
        MatchParent(ref parent);
        SortedNode(ref parent, type);
    }
    private void MatchParent(ref Dictionary<string, LaboratoryNode> nodes)
    {
        foreach (var node in nodes.Values)
        {
            foreach (var parentId in node.Data.ParentsId)
            {
                if (nodes.TryGetValue(parentId, out var parentNode))
                {
                    node.AddParent(parentNode);
                    LineController line = _poolingLines[node.Data.LaboratoryType].Pop().GetComponent<LineController>();
                    line.SetTargets(parentNode, node);
                    _lines.Add(line);
                }
            }
        }
    }
    private void SortedNode(ref Dictionary<string, LaboratoryNode> nodes, LaboratoryType type)
    {
        if (!_sorter.ContainsKey(type))
            _sorter[type] = new TopologicalSorter<LaboratoryNode>();
        foreach (var node in nodes)
        {
            _sorter[type].AddNode(node.Value);
            foreach(var parent in node.Value.Data.ParentsId)
            {
                _sorter[type].AddEdge(nodes[parent], node.Value);
            }
        }
        _nodes[type] = _sorter[type].Sort(); 
    }
    //private void SettingPosition()
    //{
    //    float xSpacing = 100f;
    //    float ySpacing = 150f;
    //
    //
    //    foreach (var node in _nodes)
    //    {
    //        Dictionary<string, int> levels = CalculateLevels(_nodes[node.Key]);
    //        foreach (var nd in node.Value)
    //        {
    //            RectTransform rt = nd.GetComponent<RectTransform>();
    //            rt.anchoredPosition = _startPosition;
    //        }
    //    }
    //}
    private void SettingPosition()
    {
        float xSpacing = 350f;
        float ySpacing = 350f;

        foreach (var kvp in _nodes) 
        {
            List<LaboratoryNode> nodeList = kvp.Value;

            Dictionary<string, int> levels = CalculateLevels(nodeList);

            // 레벨별 노드 그룹화
            Dictionary<int, List<LaboratoryNode>> levelGroups = new();
            Dictionary<string, RectTransform> rects = new();

            foreach (var node in nodeList)
            {
                int level = levels[node.Data.Id];

                if (!levelGroups.ContainsKey(level))
                    levelGroups[level] = new List<LaboratoryNode>();

                levelGroups[level].Add(node);
                rects[node.Data.Id] = node.GetComponent<RectTransform>();
            }
            Dictionary<string, Vector2> finalPositions = new();

            foreach (var level in levelGroups.Keys.OrderBy(l => l))
            {
                List<LaboratoryNode> nodesInLevel = levelGroups[level];

                Dictionary<LaboratoryNode, float> preferredY = new();

                foreach (var node in nodesInLevel)
                {
                    float totalY = 0f;
                    int count = 0;

                    foreach (var parentId in node.Data.ParentsId)
                    {
                        if (finalPositions.TryGetValue(parentId, out Vector2 parentPos))
                        {
                            totalY += parentPos.y;
                            count++;
                        }
                    }

                    float avgY = count > 0 ? totalY / count : 0f;
                    preferredY[node] = avgY;
                }

                var sortedNodes = nodesInLevel.OrderBy(n => preferredY[n]).ToList();

                int mid = sortedNodes.Count / 2;
                for (int i = 0; i < sortedNodes.Count; i++)
                {
                    int offsetIndex = i - mid;
                    if (sortedNodes.Count % 2 == 0 && i >= mid) offsetIndex += 1; // 짝수일 때 균형 맞추기

                    float x = _startPosition.x + level * xSpacing;
                    float y = _startPosition.y + -offsetIndex * ySpacing;

                    RectTransform rt = rects[sortedNodes[i].Data.Id];
                    rt.anchoredPosition = new Vector2(x, y);
                    finalPositions[sortedNodes[i].Data.Id] = new Vector2(x, y);
                }
            }
        }
    }

    private Dictionary<string, int> CalculateLevels(List<LaboratoryNode> sorted)
    {
        Dictionary<string, int> levels = new Dictionary<string, int>();

        foreach (var node in sorted)
        {
            int maxParentLevel = -1;

            foreach (var parent in node.Data.ParentsId)
            {
                if (levels.TryGetValue(parent, out int parentLevel))
                {
                    if (parentLevel > maxParentLevel)
                        maxParentLevel = parentLevel;
                }
            }

            levels[node.Data.Id] = maxParentLevel + 1; // 본인의 레벨은 부모 최대 레벨 + 1
        }

        return levels;
    }
}
