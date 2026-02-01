using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class GameVolumeControl : MonoBehaviour
{
    [Header("Links")]
    public GameObject inventoryBarObject; 
    public Transform siblingBar;          
    public Transform siblingHandle;       
    public AudioMixer masterMixer;

    [Header("Settings")]
    public float minHeight = 0.5f;
    public float maxHeight = 5.0f;
    public float gameThresholdY = -3f;
    public float handleVisualOffset = 0.0f; 

    [Header("Collision Check")]
    public LayerMask wallLayer;   
    private bool isDragging = false;
    private bool isResizing = false;
    private Rigidbody2D rb;
    private BoxCollider2D barCollider; 
    private SpriteRenderer barSprite;

    private Vector3 dragOffset;       
    private Vector3 lastValidPosition; 
    private float initialMouseY;       
    private float initialHeight;       
    private float currentHeight = 1.0f;

    void OnEnable()
    {
        isDragging = false;
        isResizing = false;

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (siblingBar != null)
        {
            if (barCollider == null) barCollider = siblingBar.GetComponent<BoxCollider2D>();
            barSprite = siblingBar.GetComponent<SpriteRenderer>();

            if (barSprite != null && barSprite.drawMode == SpriteDrawMode.Sliced)
                currentHeight = barSprite.size.y;
            else
                currentHeight = siblingBar.localScale.y;

            if (barCollider != null) barCollider.enabled = true;
        }

        if (siblingHandle != null)
        {
            var col = siblingHandle.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;
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
        if (transform.position.z != 0) 
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool foundHandle = false;
            bool foundBar = false;

            foreach (var hit in hits)
            {
                if (hit.collider.transform == siblingHandle) foundHandle = true;
                if (hit.collider.transform == siblingBar || hit.collider.gameObject == gameObject) foundBar = true;
            }

            if (foundHandle) 
            {
                isResizing = true;
                initialMouseY = mousePos.y;
                initialHeight = currentHeight;
            }
            else if (foundBar)
            {
                isDragging = true;
                dragOffset = transform.position - mousePos;
                lastValidPosition = transform.position; 
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                if (barCollider != null && barCollider.IsTouchingLayers(wallLayer))
                {
                    Debug.Log("🚫 Inside Wall! Snapping back.");
                    transform.position = lastValidPosition; 
                }
                else
                {
                    if (transform.position.y < gameThresholdY) 
                    {
                        SwapToInventoryMode();
                    }
                }
            }

            isDragging = false;
            isResizing = false;
        }
        if (isDragging)
        {
            if (rb != null)
            {
                rb.MovePosition(mousePos + dragOffset);
            }
            else
            {
                transform.position = mousePos + dragOffset;
            }
            
            UpdateVisuals(currentHeight);
        }
        else if (isResizing)
        {
            float mouseDelta = mousePos.y - initialMouseY;
            float newH = initialHeight + mouseDelta;

            currentHeight = Mathf.Clamp(newH, minHeight, maxHeight);
            UpdateVisuals(currentHeight);
            ApplyAudio(currentHeight);
        }
    }

      void UpdateVisuals(float h)
    {
        if (siblingBar == null || siblingHandle == null) return;

        // Scale
        if (barSprite != null && barSprite.drawMode == SpriteDrawMode.Sliced)
        {
            barSprite.size = new Vector2(barSprite.size.x, h);
            siblingBar.localScale = Vector3.one; 
        }
        else
        {
            siblingBar.localScale = new Vector3(0.5f, h, 1f);
        }

        Physics2D.SyncTransforms(); 

        float topY = 0f;
        if(barCollider != null) topY = barCollider.bounds.max.y;
        else topY = siblingBar.position.y + h; 

        siblingHandle.position = new Vector3(siblingBar.position.x, topY + handleVisualOffset, 0);
    }

   void ApplyAudio(float h)
{
    if (GlobalSoundManager.Instance != null)
    {
        GlobalSoundManager.Instance.SetVolumeFromLength(h, minHeight, maxHeight);
    }
}

    void SwapToInventoryMode()
    {
        if (inventoryBarObject != null)
        {
            inventoryBarObject.transform.position = transform.position;
            inventoryBarObject.GetComponent<InventoryVolumeControl>().SyncState(currentHeight);
            inventoryBarObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}