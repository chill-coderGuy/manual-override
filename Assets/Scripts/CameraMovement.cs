using UnityEngine;

public class CameraFollowX : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           
    public Vector3 offset;             
    public bool useInitialOffset = true; 

    [Header("Movement Settings")]
    [Range(0f, 1f)]
    public float smoothTime = 0.2f;    
    
    [Header("Level Boundaries")]
    public bool enableLimits = true;   
    public float minX = -10f;          
    public float maxX = 50f;           
   
    private Vector3 velocity = Vector3.zero; 
    void Start()
    { if (target != null && useInitialOffset)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;
        float targetX = target.position.x + offset.x;
        if (enableLimits)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }
        Vector3 destination = new Vector3(targetX, transform.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
    }
    void OnDrawGizmos()
    {
        if (enableLimits)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(minX, -10, 0), new Vector3(minX, 10, 0));
            Gizmos.DrawLine(new Vector3(maxX, -10, 0), new Vector3(maxX, 10, 0));
        }
    }
}