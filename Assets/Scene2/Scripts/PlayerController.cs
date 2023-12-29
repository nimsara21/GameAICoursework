using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{

    public static float currentSpeed = 15f;
    private NavMeshAgent navMeshAgent;
    private float turnSmoothVelocity;
    public float turnSmoothTime = 0.1f;

    public Transform cameraTransform;
    public float cameraFollowSpeed = 5f;

    public static int maxHealth = 100;
    public static int currentHealth;
    public float rotationSpeed = 5f;

    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    
    private Collider playerCollider;
    void Start()
    {
        currentHealth = maxHealth;
        
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("NavMeshAgent component not found on the player GameObject.");
        }

        SetPlayerOnNavMesh();
        cameraTransform = Camera.main?.transform;
        if (cameraTransform == null)
    {
        Debug.LogWarning("Main Camera not found. Make sure you have a camera tagged as 'MainCamera' in your scene.");
    }
        
        playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.isTrigger = false;
        }else{
            Debug.LogError("Collider component not found on the player GameObject.");

        }
        
        //  Rigidbody playerRigidbody = gameObject.AddComponent<Rigidbody>();
        //     playerRigidbody.isKinematic = false;
    }
    

    void Update()
    {
        HandlePlayerMovement();
        HandleCameraFollow();
        //HandleMouseRotation();

        if (Input.GetMouseButtonDown(0)) // 0 = LMB
        {
            Shoot();
        }
    }

    void HandlePlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;

        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            transform.Translate(movement * currentSpeed * Time.deltaTime, Space.World);
        }
    }

void Shoot()
{
    if (bulletPrefab != null && firePoint != null)
    {
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            bullet.layer = LayerMask.NameToLayer("PlayerBullet");
            

       
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

        if (bulletRb != null)
        {
            bulletRb.useGravity = false;
           
            float increasedSpeed = bulletSpeed * 2f; 
            bulletRb.AddForce(firePoint.forward * increasedSpeed, ForceMode.Impulse);

            
            float bulletLifespan = 5f; 
            Destroy(bullet, bulletLifespan);
        }
        else
        {
            Debug.LogError("Bullet prefab does not have a Rigidbody component.");
        }
    }
    else
    {
        Debug.LogError("Bullet prefab or fire point not assigned in the inspector.");
    }
}

    public void ApplySpeedPowerUp(float speedMultiplier, float duration)
    {
        StartCoroutine(SpeedPowerUpCoroutine(speedMultiplier, duration));
    }

    private IEnumerator SpeedPowerUpCoroutine(float speedMultiplier, float duration)
    {
        currentSpeed *= speedMultiplier;

        yield return new WaitForSeconds(duration);

        
        currentSpeed /= speedMultiplier;
    }

        public void HitTrap(float speedMultiplier, float duration)
    {
        StartCoroutine(SpeedPowerDownCoroutine(speedMultiplier, duration));
    }

    private IEnumerator SpeedPowerDownCoroutine(float speedMultiplier, float duration)
    {
        currentSpeed /= speedMultiplier;

        yield return new WaitForSeconds(duration);

        
        currentSpeed *= speedMultiplier;
    }


    public void TakeDamage(int damage)
    {
        Grid.playerHealth -= damage;
        

       
        if (currentHealth <= 0)
        {
            Debug.Log("Player is defeated!");
           
        }
    }

void HandleCameraFollow()
{
    if (cameraTransform != null)
    {
        
        Vector3 offset = new Vector3(5f, 8f, -10f); 
        Vector3 targetPosition = transform.position + offset;

       
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);

       
        cameraTransform.LookAt(transform.position + Vector3.up * 2f); 
    }
}
void HandleMouseRotation()
    {
        
        float mouseX = Input.GetAxis("Mouse X");

       
        transform.Rotate(Vector3.up * mouseX * rotationSpeed);

       
        if (cameraTransform != null)
        {
           
            Quaternion newRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);

            
            cameraTransform.rotation = newRotation;
        }
    }

    void SetPlayerOnNavMesh()
    {
        NavMeshHit hit;
        Vector3 randomPosition = new Vector3(transform.position.x, 0f, transform.position.z);

        if (NavMesh.SamplePosition(randomPosition, out hit, 10f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogError("Could not find a valid position for the player on the NavMesh.");
        }
    }
}