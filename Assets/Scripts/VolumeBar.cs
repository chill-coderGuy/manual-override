using UnityEngine;
using UnityEngine.Audio;

public class TelescopicVolumeTool : MonoBehaviour
{
    [Header("References")]
    public Transform barTransform;   // Drag the PARENT (Square) here!
    public AudioMixer masterMixer;   // Drag MainMixer here

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float interfaceYThreshold = -3f; // Adjust this to your floor level

    private bool isDraggingWholeTool = false;
    private bool isResizing = false;
    private Vector3 dragOffset;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // --- INPUT ---
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                // CASE 1: Clicked the Handle (This script)
                if (hit.collider.gameObject == gameObject)
                {
                    // IN GAME -> Resize Mode
                    if (barTransform.position.y > interfaceYThreshold)
                    {
                        isResizing = true;
                    }
                    // IN INVENTORY -> Move Mode
                    else
                    {
                        StartMove(mousePos);
                    }
                }
                // CASE 2: Clicked the Bar (Parent)
                else if (hit.collider.transform == barTransform)
                {
                    StartMove(mousePos);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingWholeTool = false;
            isResizing = false;
        }

        // --- EXECUTION ---
        if (isDraggingWholeTool)
        {
            // FIX: Move the PARENT, not 'transform.position'
            barTransform.position = mousePos + dragOffset; 
        }
        else if (isResizing)
        {
            ResizeTool(mousePos);
        }
    }

    void StartMove(Vector3 mPos)
    {
        isDraggingWholeTool = true;
        // FIX: Calculate offset relative to the PARENT
        dragOffset = barTransform.position - mPos; 
    }

    void ResizeTool(Vector3 mPos)
    {
        // 1. Calculate new height based on mouse Y
        float newHeight = mPos.y - barTransform.position.y;
        newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

        // 2. Scale the PARENT (The Bar)
        barTransform.localScale = new Vector3(barTransform.localScale.x, newHeight, 1);
        transform.localScale = new Vector3(1f, 1f / newHeight, 1f);
        
        // Note: Since the Handle is a child, it moves up automatically!
        // We don't need to touch transform.position of the handle.

        ApplyAudioEffects(newHeight);
    }

    void ApplyAudioEffects(float currentHeight)
{
    if (masterMixer == null) 
    {
        Debug.LogWarning("Master Mixer is missing! Drag it into the Inspector slot.");
        return;
    }

    // 1. NORMALIZE: Map height to a 0.0 - 1.0 range
    // If height is 0.5 (min) -> t = 0.0
    // If height is 5.0 (max) -> t = 1.0
    float t = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);

    // 2. PITCH: Map 0-1 to 0.6-1.0
    float pitch = Mathf.Lerp(0.6f, 1.0f, t);
    bool pitchSuccess = masterMixer.SetFloat("MusicPitch", pitch);

    // 3. VOLUME: Map 0-1 to -40dB (Quiet) and 0dB (Loud)
    // We use -40 instead of -80 to ensure you can hear the difference clearly
    float vol = Mathf.Lerp(-40f, 0f, t);
    bool volSuccess = masterMixer.SetFloat("MusicVol", vol);

    // DEBUG: Look at your Console while dragging!
    // If these say 'False', your Exposed Parameter name is wrong.
    // If 't' stays 0 or 1, your min/max height settings are wrong.
    Debug.Log($"t: {t:F2} | Vol: {vol:F2} (Success: {volSuccess}) | Pitch: {pitch:F2}");
}
}