using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{

    public float Speed = 10f;

    
    void Start()
    {
        // Destroy the bullet after 5 seconds
        Destroy(gameObject, 5f);
    }

   
    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Obstacle"))
        {
            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(10);
            }
            Destroy(gameObject);
        }
    }
}
