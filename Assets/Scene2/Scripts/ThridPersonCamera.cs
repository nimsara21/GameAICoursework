using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField]
    private Transform player; 

    public Vector3 offset = new Vector3(0f, 2f, -5f);
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        if (player == null)
        {
            Debug.LogError("Player reference not set in ThirdPersonCamera script!");
            return;
        }

        Vector3 desiredPosition = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * rotationSpeed);

        Vector3 lookDirection = player.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
    }
}
