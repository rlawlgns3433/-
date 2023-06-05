using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public PlayAgent playAgent;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("�Ѿ��� ���� ����");
            //playAgent.SetReward(+1.0f);
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Environment") || collision.gameObject.CompareTag("Wall")|| collision.gameObject.CompareTag("OtherObject"))
        {
            Debug.Log("�Ѿ��� ȯ���� ����");
            //playAgent.SetReward(-1.0f /playAgent.MaxStep);
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
