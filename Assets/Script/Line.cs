using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public GameObject startNode;
    public GameObject endNode;
    public LineRenderer lineRenderer;

    public Line(GameObject start, GameObject end, LineRenderer renderer)
    {
        startNode = start;
        endNode = end;
        lineRenderer = renderer;
    }

    void Update()
    {
        if (startNode == null || endNode == null)
        {
            // ����� ��尡 �����Ǹ� ������ �ı�
            Debug.Log("One of the nodes is null. Destroying the line.");
            Destroy(gameObject); // ���� ���� ������Ʈ �ı�
        }
    }

    public void DrawLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startNode.transform.position);
            lineRenderer.SetPosition(1, endNode.transform.position);
        }
    }

    public void DestroyIfInvalidLine()
    {
        // ���� ��峪 �� ��尡 null�̸� ���ἱ�� �ı�
        if (startNode == null || endNode == null)
        {
            if (lineRenderer != null)
            {
                GameObject.Destroy(lineRenderer.gameObject); // ���� ����
            }
        }
    }
}
