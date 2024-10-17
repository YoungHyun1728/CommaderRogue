using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum NodeType
{
    Combat,  // ���� ����Ȯ���� MapGenerator.cs
    Rest,    
    Boss,    // ����    10������ ����,���� 10�������� ����
    Event,   
    Trade,   
}

public class MapNode : MonoBehaviour
{
    public NodeType Type { get; private set; }
    public int Level { get; private set; }  // Ʈ�� ���� (����)
    public List<MapNode> Connections { get; private set; }  // ����� ����
    public GameObject NodeObject { get; private set; }
    public List<Line> lines = new List<Line>();
    public List<GameObject> prevNodePrefab = new List<GameObject>();

    private void Update()
    {
        if (Level == 0)  // ���� 0�� �ı����� ����
        {
            return;
        }

        if (prevNodePrefab == null || prevNodePrefab.Count == 0)
        {
            Destroy(gameObject); // ����Ʈ�� null�̰ų� ������� �� �ı�
        }
        else
        {
            bool allNoneOrMissing = true;  // ��� ��尡 None(�Ǵ� Missing)���� Ȯ���ϴ� ����

            foreach (var prevNode in prevNodePrefab)
            {
                if (prevNode != null)  // null(��, None ����)�� �ƴ� ��尡 ������
                {
                    allNoneOrMissing = false;
                    break;  // �� �̻� üũ�� �ʿ� ����
                }
            }

            if (allNoneOrMissing)
            {
                Destroy(gameObject);  // ��� ��尡 None(�Ǵ� Missing)�� ��� �ı�
            }
        }
    }

    public void Initialize(NodeType type, int level, GameObject nodeObject)
    {
        Type = type;
        Level = level;
        NodeObject = nodeObject;
        Connections = new List<MapNode>();
    }

    public void ConnectTo(MapNode otherNode, Line line)
    {
        Connections.Add(otherNode);
        lines.Add(line);
    }
}
