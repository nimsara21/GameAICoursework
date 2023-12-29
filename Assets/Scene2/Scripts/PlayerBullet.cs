using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float Speed = 10f;
    public delegate void BulletHitAction();
    public event BulletHitAction OnBulletHit;
    
void Start()
    {
        
        Collider bulletCollider = GetComponent<Collider>();

        
        if (bulletCollider != null)
        {
           
            bulletCollider.isTrigger = false;
        }
        else
        {
            Debug.LogWarning("Collider component not found on the EnemyBullet GameObject.");
        }
        //  Rigidbody playerRigidbody = gameObject.AddComponent<Rigidbody>();
        //     playerRigidbody.isKinematic = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            
            EnemyController enemyController = collision.gameObject.GetComponent<EnemyController>();

            if (enemyController != null)
            {
               
                enemyController.TakeDamage(25);
            }

            
            Destroy(gameObject);
        }
    }
    public float speed = 10.0f;
    private Vector3 direction;

    public void Shoot(Vector3 shootDirection)
    {
        direction = shootDirection.normalized;
    }

    void Update()
    {
       
        transform.Translate(direction * speed * Time.deltaTime);
    }
    public int GetDamage()
    {
        
        return 10; 
    }
    

    
}