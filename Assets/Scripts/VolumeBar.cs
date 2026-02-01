using UnityEngine;
using UnityEngine.Audio;

public class TelescopicVolumeTool : MonoBehaviour
{
    [Header("References")]
    public Transform barTransform;   
    public AudioMixer masterMixer;   

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float interfaceYThreshold = -3f; 

    private bool isDraggingWholeTool = false;
    private bool isResizing = false;
    private Vector3 dragOffset;

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (barTransform.position.y > interfaceYThreshold)
                    {
                        isResizing = true;
                    }
                    else
                    {
                        StartMove(mousePos);
                    }
                }
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
        if (isDraggingWholeTool)
        {
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
        dragOffset = barTransform.position - mPos; 
    }

    void ResizeTool(Vector3 mPos)
    {
        float newHeight = mPos.y - barTransform.position.y;
        newHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
        barTransform.localScale = new Vector3(barTransform.localScale.x, newHeight, 1);
        transform.localScale = new Vector3(1f, 1f / newHeight, 1f);
        ApplyAudioEffects(newHeight);
    }

    void ApplyAudioEffects(float currentHeight)
{
    if (masterMixer == null) 
    {
        Debug.LogWarning("Master Mixer is missing! Drag it into the Inspector slot.");
        return;
    }
    float t = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);
    float pitch = Mathf.Lerp(0.6f, 1.0f, t);
    bool pitchSuccess = masterMixer.SetFloat("MusicPitch", pitch);
    float vol = Mathf.Lerp(-40f, 0f, t);
    bool volSuccess = masterMixer.SetFloat("MusicVol", vol);
    Debug.Log($"t: {t:F2} | Vol: {vol:F2} (Success: {volSuccess}) | Pitch: {pitch:F2}");
}
}