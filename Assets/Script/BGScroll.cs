using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGScroll : MonoBehaviour
{
    public float scrollSpeed;  // ����� ��ũ�ѵǴ� �ӵ�
    public float resetPosition = -15f;  // ����� �������� ����� ��ġ
    public float startPosition = 15f;  // ����� �����ʿ��� �ٽ� �����ϴ� ��ġ

    private Vector3 initialPosition;

    void Start()
    {
        scrollSpeed = Random.Range(1f, 3f);
        initialPosition = transform.position;
    }

    void Update()
    {
        // ����� �������� �̵�
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // ����� ȭ�� ���� ���� ����� ��ġ�� ���������� ���ġ
        if (transform.position.x < resetPosition)
        {
            Vector3 newPosition = new Vector3(startPosition, transform.position.y, transform.position.z);
            transform.position = newPosition;
        }
    }
}
