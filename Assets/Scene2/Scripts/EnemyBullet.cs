using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float Speed = 10f;
    public delegate void BulletHitAction();
    public event BulletHitAction OnBulletHit;
    
void Start()
    {
        
        Collider bulletCollider = GetComponent<Collider>();

        // Check if the Collider component exists
        if (bulletCollider != null)
        {
            // Enable "Is Trigger" during runtime
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
        
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

            if (playerController != null)
            {
                // Inflict damage to the player
                playerController.TakeDamage(10);
            }

            
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Environment"))
        {
           
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