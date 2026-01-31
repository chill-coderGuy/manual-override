using UnityEngine;

public class CameraFollowX : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;           // Drag your Player object here
    public Vector3 offset;             // Manual offset (how far form player to sit)
    public bool useInitialOffset = true; // If true, auto-calculates offset on Start

    [Header("Movement Settings")]
    [Range(0f, 1f)]
    public float smoothTime = 0.2f;    // 0 = Instant, 1 = Very Slow (0.1-0.3 is standard)
    
    [Header("Level Boundaries")]
    public bool enableLimits = true;   // Turn this on to stop camera at edges
    public float minX = -10f;          // Far left boundary
    public float maxX = 50f;           // Far right boundary

    // Private variables for the math
    private Vector3 velocity = Vector3.zero; // Used internally by SmoothDamp

    void Start()
    {
        // 1. Auto-Setup Offset
        // This calculates the initial distance so you don't have to guess the numbers
        if (target != null && useInitialOffset)
        {
            offset = transform.position - target.position;
            // Since we only care about X, we ensure Y and Z offsets are kept 
            // but effectively ignored by the movement logic below
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 2. Calculate the Goal Position
        // We take the Player's X, add the offset X.
        // We KEEP the Camera's current Y and Z (Locking vertical movement)
        float targetX = target.position.x + offset.x;

        // 3. Apply Limits (Clamping)
        if (enableLimits)
        {
            targetX = Mathf.Clamp(targetX, minX, maxX);
        }

        // Create the final destination vector
        Vector3 destination = new Vector3(targetX, transform.position.y, transform.position.z);

        // 4. Smoothly Move
        // SmoothDamp moves from 'current' to 'destination' over 'smoothTime'
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
    }

    // 5. Visual Debugging
    // Draws lines in the Scene view so you can see the Min/Max limits
    void OnDrawGizmos()
    {
        if (enableLimits)
        {
            Gizmos.color = Color.green;
            // Draw Left Boundary
            Gizmos.DrawLine(new Vector3(minX, -10, 0), new Vector3(minX, 10, 0));
            // Draw Right Boundary
            Gizmos.DrawLine(new Vector3(maxX, -10, 0), new Vector3(maxX, 10, 0));
        }
    }
}