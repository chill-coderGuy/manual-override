using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlankDispenser : MonoBehaviour
{
    [Header("1. References")]
    public GameObject plankPrefab;    // The object to spawn (Prefab)
    public Transform spawnParent;     // Optional: An empty object to keep hierarchy clean
    public RectTransform clickZone;   // The Transparent UI Image you click
    public RectTransform progressBar; // The Green Bar Image
    public TextMeshProUGUI ammoText;  // The Text showing "3"
    
    [Header("2. Settings")]
    public int startingAmmo = 3;
    public LayerMask invalidLayers;   // Set to "Ground" or "Walls"
    public float minWidth = 1f;       // Plank size at start of level
    public float maxWidth = 10f;      // Plank size at end of level

    [Header("3. Level Progress")]
    public Transform levelStart;
    public Transform levelEnd;
    public Transform player;

    // Internal State
    private int currentAmmo;
    private float currentProgress;
    private GameObject activePlank;   // The plank we are currently dragging
    private bool isDragging = false;
    private Canvas rootCanvas;

    void Start()
    {
        currentAmmo = startingAmmo;
        UpdateUI();
        
        rootCanvas = GetComponentInParent<Canvas>();
        
        // Auto-Find Player
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        // A. Calculate Level Progress (0% to 100%)
        CalculateProgress();

        // B. Input Handling
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }
        
        if (isDragging && activePlank != null)
        {
            UpdateDragPosition();
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            TryPlacePlank();
        }
    }

    // --- LOGIC ---

    void TryStartDrag()
    {
        // 1. Check Ammo
        if (currentAmmo <= 0) return;

        // 2. Check if Mouse is inside the UI Click Zone
        if (IsMouseInZone(clickZone))
        {
            StartSpawning();
        }
    }

    void StartSpawning()
    {
        isDragging = true;

        // 1. Determine Spawn Position (World Space)
        Vector3 spawnPos = GetMouseWorldPos();

        // 2. Instantiate the "Child" Copy
        activePlank = Instantiate(plankPrefab, spawnPos, Quaternion.identity, spawnParent);

        // 3. Set Size based on Progress
        float width = Mathf.Lerp(minWidth, maxWidth, currentProgress);
        activePlank.transform.localScale = new Vector3(width, 0.5f, 1f);

        // 4. Disable Physics temporarily (so it doesn't collide while dragging)
        Collider2D col = activePlank.GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }

    void UpdateDragPosition()
    {
        // Snap plank to mouse
        activePlank.transform.position = GetMouseWorldPos();
    }

    void TryPlacePlank()
    {
        isDragging = false;
        if (activePlank == null) return;

        // 1. Re-enable Collider to check for overlaps
        Collider2D col = activePlank.GetComponent<Collider2D>();
        if (col) col.enabled = true;

        // 2. Check for invalid placement (Walls)
        // We use OverlapBox to see if the plank is hitting the 'invalidLayers'
        Collider2D hit = Physics2D.OverlapBox(activePlank.transform.position, activePlank.transform.localScale, 0f, invalidLayers);

        if (hit != null)
        {
            Debug.Log("🚫 Invalid Placement! (Inside Wall)");
            Destroy(activePlank); // Refund (Don't decrease ammo)
        }
        else
        {
            Debug.Log("✅ Plank Placed!");
            currentAmmo--; // Decrease Ammo
            UpdateUI();
        }

        activePlank = null; // Forget about this plank, it's part of the world now
    }

    // --- HELPERS ---

    void CalculateProgress()
    {
        if (player == null || levelStart == null || levelEnd == null) return;

        float totalDist = levelEnd.position.x - levelStart.position.x;
        float currentDist = player.position.x - levelStart.position.x;
        currentProgress = Mathf.Clamp01(currentDist / totalDist);

        // Update the Green Bar Fill
        if (progressBar != null)
        {
            // Important: Ensure Pivot X is 0 on the Image in Inspector
            progressBar.localScale = new Vector3(currentProgress, 1, 1);
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0; // Lock to 2D plane
        return mPos;
    }

    bool IsMouseInZone(RectTransform zone)
    {
        if (zone == null) return false;
        
        // Supports both Overlay and Camera Space UI
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return RectTransformUtility.RectangleContainsScreenPoint(zone, Input.mousePosition, null);
        else
            return RectTransformUtility.RectangleContainsScreenPoint(zone, Input.mousePosition, Camera.main);
    }

    void UpdateUI()
    {
        if (ammoText != null) ammoText.text = currentAmmo.ToString();
    }
}