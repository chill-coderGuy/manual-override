using UnityEngine;
using UnityEngine.Audio;

public class TelescopicVolumeTool : MonoBehaviour
{
    [Header("References")]
    public Transform barTransform; // The Rectangle Bar (Parent)
    public AudioMixer masterMixer;

    [Header("State Settings")]
    public float interfaceYThreshold = -5f; // Boundary between inventory and game
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private float originalVol;

    void Start()
    {
        // Store the default engine volume
        masterMixer.GetFloat("MusicVol", out originalVol);
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        if (Input.GetMouseButtonDown(0)) StartInteraction(mousePos);
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging) ExecuteInteraction(mousePos);
    }

    void StartInteraction(Vector3 mPos)
    {
        // We only respond if the HANDLE is clicked
        RaycastHit2D hit = Physics2D.Raycast(mPos, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            isDragging = true;
            dragOffset = transform.position - mPos;
        }
    }

    void ExecuteInteraction(Vector3 mPos)
    {
        // 1. Check State: Are we in the Inventory or the Game?
        bool isInGame = transform.position.y > interfaceYThreshold;

        if (!isInGame)
        {
            // IN INVENTORY: Left click just moves the whole tool
            barTransform.position = mPos + dragOffset;
        }
        else
        {
            // IN GAME: The base stays put, and we SCALE the bar
            float newHeight = mPos.y - barTransform.position.y;
            newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

            // Physically scale the bar upwards
            barTransform.localScale = new Vector3(barTransform.localScale.x, newHeight, 1);
            
            // Move the handle to stay at the new top of the bar
            transform.localPosition = new Vector3(0, 1f, 0); // Stays at top relative to scale

            ApplyAudioLeak(newHeight);
        }
    }

    void ApplyAudioLeak(float height)
    {
        // Map height to 0-1 for volume/pitch changes
        float t = Mathf.InverseLerp(minHeight, maxHeight, height);
        
        float pitch = Mathf.Lerp(0.7f, 1.0f, t);
        float volume = Mathf.Lerp(-30f, originalVol, t);

        masterMixer.SetFloat("MusicPitch", pitch);
        masterMixer.SetFloat("MusicVol", volume);
    }
}