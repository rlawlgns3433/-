using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("�Ѿ��� ���� ����");
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Environment"))
        {
            Debug.Log("�Ѿ��� ȯ���� ����");
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        Invoke("DestroyBullet", 10f);
    }
    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
