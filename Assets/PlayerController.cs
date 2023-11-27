using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Ensure the agent is on the NavMesh
        StartCoroutine(EnableAgent());
    }

    IEnumerator EnableAgent()
    {
        yield return new WaitForEndOfFrame();
        agent.enabled = true;
    }

    void Update()
    {
        // Check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
            }
        }

        // Check for WASD input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Update the agent's destination based on WASD input
        if (movement.magnitude > 0.1f)
        {
            Vector3 moveDirection = Camera.main.transform.TransformDirection(movement);
            moveDirection.y = 0.0f; // Ensure the movement is horizontal
            moveDirection.Normalize();
            Vector3 moveDestination = transform.position + moveDirection * 5.0f; // Adjust the multiplier as needed
            agent.SetDestination(moveDestination);
        }
    }
}
