using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EnemyController : MonoBehaviour
{
    public enum NPCStates
    {
        Patrol,
        Chase,
        Attack,
        GroupAttack,
        StopChase
    }

    [SerializeField] private float chaseRange = 15f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private EnemyBullet BulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;

    private readonly float fireRate = 2f;
    private NPCStates currentState = NPCStates.Patrol;
    private NavMeshAgent navMeshAgent;
    private float nextShootTime = 0f;
    private Transform playerPosition;
    private float originalChaseRange;
    private int nextPatrolPoint = 0;
    [SerializeField] private GameObject floatingTextPrefab;
    private GameObject floatingTextObject;

    private float enemyMaxHealth = 100f;
    private float currentHealth;

    
    private List<Transform> patrolPoints = new List<Transform>();

    // Public variable
    public int numberOfPatrolPoints = 5;

    
    public int enemyID;




    void Start()
    {
        currentHealth = enemyMaxHealth;
        InitializeComponents();
        GeneratePatrolPoints();
        SetFloatingTextActive(true);

        enemyID = GetInstanceID();
        floatingTextObject = Instantiate(floatingTextPrefab, transform.position, Quaternion.identity, transform);
        float yOffset = 5f; 
        floatingTextObject.transform.Rotate(Vector3.up, 180f);
        Vector3 textPosition = floatingTextObject.transform.localPosition;
        textPosition.y = yOffset;
        floatingTextObject.transform.localPosition = textPosition;
    }

    void Update()
    {
        SwitchState();
    }

    void UpdateFloatingText()
    {
        if (floatingTextObject != null)
        {
            
            TextMeshPro textMesh = floatingTextObject.GetComponentInChildren<TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = "State: " + currentState.ToString();
            }
        }
    }
    void SetFloatingTextActive(bool isActive)
{
    if (floatingTextObject != null)
    {
        floatingTextObject.SetActive(isActive);
    }
}

        void OnDestroy()
    {
        
        if (floatingTextObject != null)
        {
            Destroy(floatingTextObject);
        }
    }


    void InitializeComponents()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerPosition = playerObject.transform;
        }
        else
        {
            Debug.Log("Player object not found");
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        CreateBulletSpawnPoint();
    }

    void CreateBulletSpawnPoint()
    {
        GameObject spawnPointObject = new GameObject("BulletSpawnPoint");
        spawnPointObject.transform.parent = transform;
        spawnPointObject.transform.localPosition = new Vector3(0f, 1.5f, 2f);
        bulletSpawnPoint = spawnPointObject.transform;
    }

    void SwitchState()
    {
        switch (currentState)
        {
            case NPCStates.Patrol:
                Patrol();
                UpdateFloatingText();
                break;
            case NPCStates.Chase:
                Chase();
                UpdateFloatingText();
                break;
            case NPCStates.Attack:
                Attack();
                UpdateFloatingText();
                break;
            case NPCStates.GroupAttack:
                GroupAttack();
                Debug.Log("Group Attack");
                break;
            case NPCStates.StopChase:
                currentState = NPCStates.Patrol;
                break;
            default:
                Patrol();
                break;
        }
    }

    void GeneratePatrolPoints()
    {
        for (int i = 0; i < numberOfPatrolPoints; i++)
        {
            Transform newPatrolPoint = InstantiatePatrolPoint();
            patrolPoints.Add(newPatrolPoint);
        }
    }

    Transform InstantiatePatrolPoint()
    {
        GameObject patrolPointObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        patrolPointObject.name = "PatrolPoint";
        patrolPointObject.transform.position = GetRandomGridPosition();
        return patrolPointObject.transform;
    }

    Vector3 GetRandomGridPosition()
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 0, z);
    }

    void GroupAttack()
    {
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition.position);

        if (distanceToPlayer > chaseRange)
        {
            Patrol();
            return;
        }

        if (distanceToPlayer <= chaseRange && distanceToPlayer > attackRange)
        {
            Chase();
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
            Attack();
            return;
        }
    }

    void Chase()
    {
        //Debug.Log("Chase");
        navMeshAgent.SetDestination(playerPosition.position);
        if (Vector3.Distance(transform.position, playerPosition.position) > chaseRange)
        {
            currentState = NPCStates.Patrol;
            return;
        }

        if (Vector3.Distance(transform.position, playerPosition.position) <= attackRange)
        {
            currentState = NPCStates.Attack;
            return;
        }
    }

void Attack()
{
    navMeshAgent.isStopped = true;
    transform.LookAt(playerPosition);

    if (Time.time >= nextShootTime)
    {
        // Debug.Log("Attack");
        nextShootTime = Time.time + 1f / fireRate;

        //Direction from bulletSpawnPoint to playerPosition
        Vector3 direction = (playerPosition.position - bulletSpawnPoint.position).normalized;

       
        EnemyBullet bullet = InstantiateBullet();
        bullet.tag = "Normal Bullet";
        

        
        bullet.GetComponent<Rigidbody>().velocity = direction * bullet.Speed;
    }

    if (Vector3.Distance(transform.position, playerPosition.position) > attackRange)
    {
        navMeshAgent.isStopped = false;
        currentState = NPCStates.Chase;
        return;
    }
}

EnemyBullet InstantiateBullet()
{
    
    GameObject bulletObject = Instantiate(BulletPrefab, bulletSpawnPoint.position, Quaternion.identity).gameObject;
    EnemyBullet bullet = bulletObject.GetComponent<EnemyBullet>(); 
    bulletObject.layer = LayerMask.NameToLayer("EnemyBullet");

    
    Rigidbody bulletRigidbody = bulletObject.GetComponent<Rigidbody>();
    if (bulletRigidbody == null)
    {
        bulletRigidbody = bulletObject.AddComponent<Rigidbody>();
    }

    
    bulletRigidbody.velocity = Vector3.zero;

    return bullet;
}

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        
        if (currentHealth <= 0)
        {
            Debug.Log("Enemy is defeated!");
          
        }
    }
void Patrol()
{
    //Debug.Log("Patrol");

    
    if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
    {
        // Toggle the patrol direction
        if (patrolDirection == PatrolDirection.Forward)
        {
            patrolDirection = PatrolDirection.Backward;
        }
        else
        {
            patrolDirection = PatrolDirection.Forward;
        }

       
        Vector3 patrolDestination = transform.position + transform.forward * 10;
        navMeshAgent.SetDestination(patrolDestination);
    }

    
    if (Vector3.Distance(transform.position, playerPosition.position) <= chaseRange)
    {
        currentState = NPCStates.Chase;
    }
}

enum PatrolDirection
{
    Forward,
    Backward
}

private PatrolDirection patrolDirection = PatrolDirection.Forward;
IEnumerator WaitAtPatrolPoint(float duration)
{
    
    navMeshAgent.isStopped = true;

   
    yield return new WaitForSeconds(duration);

    
    navMeshAgent.isStopped = false;
}
}