using UnityEngine;
using UnityEngine.Audio;

public class InventoryVolumeControl : MonoBehaviour
{
    [Header("Links")]
    public GameObject gameBarObject; 
    public Transform siblingBar;     
    public Transform siblingHandle;  
    public AudioMixer masterMixer;

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float sensitivity = 0.2f; 
    public float gameThresholdY = 3.0f; 
    public float handleVisualOffset = 0.0f; 

    private bool isDragging = false;
    private bool isResizing = false;
    private Vector3 dragOffset;
    private float currentHeight = 1.0f;
    private BoxCollider2D barCollider; 
    private Vector3 initialUiPosition;

    void Awake()
    {
       
        initialUiPosition = transform.position;
    }

    void OnEnable()
    {
        isDragging = false;
        isResizing = false;
        transform.position = initialUiPosition;

        if (siblingBar != null)
        {
            if (barCollider == null) barCollider = siblingBar.GetComponent<BoxCollider2D>();
            currentHeight = siblingBar.localScale.y;
        }

        UpdateVisuals(currentHeight);
    }

    public void SyncState(float syncedHeight)
    {
        currentHeight = syncedHeight;
        UpdateVisuals(currentHeight);
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        if (Input.GetMouseButtonDown(1)) 
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == siblingHandle) isResizing = true;
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null && hit.collider.transform == siblingHandle)
            {
                isDragging = true;
                dragOffset = transform.position - mousePos;
            }
        }
        if (Input.GetMouseButtonUp(0)) isDragging = false;
        if (Input.GetMouseButtonUp(1)) isResizing = false;
        if (!isDragging && !isResizing)
        {
            if (transform.position.y > gameThresholdY)
            {
                SwapToGameMode();
            }
            else if (transform.position != initialUiPosition)
            {
                transform.position = initialUiPosition;
                UpdateVisuals(currentHeight);
            }
        }
        if (isResizing)
        {
            float delta = Input.GetAxis("Mouse Y") * sensitivity;
            currentHeight = Mathf.Clamp(currentHeight + delta, minHeight, maxHeight);
            UpdateVisuals(currentHeight);
            ApplyAudio(currentHeight);
        }
        else if (isDragging)
        {
            transform.position = mousePos + dragOffset;
            UpdateVisuals(currentHeight); 
        }
    }

    void UpdateVisuals(float h)
    {
        if (siblingBar == null || siblingHandle == null) return;

        siblingBar.localScale = new Vector3(siblingBar.localScale.x, h, 1);
        Physics2D.SyncTransforms(); 

        float topY = (barCollider != null) ? barCollider.bounds.max.y : siblingBar.position.y + (h / 2);
        siblingHandle.position = new Vector3(siblingBar.position.x, topY + handleVisualOffset, 0);
    }

   void ApplyAudio(float h)
{
    if (GlobalSoundManager.Instance != null)
    {
        GlobalSoundManager.Instance.SetVolumeFromLength(h, minHeight, maxHeight);
    }
}
    void SwapToGameMode()
    {
        if(gameBarObject != null)
        {
             gameBarObject.transform.position = transform.position;
             gameBarObject.GetComponent<GameVolumeControl>().SyncState(currentHeight);
             gameBarObject.SetActive(true);
             gameObject.SetActive(false);
        }
    }
}