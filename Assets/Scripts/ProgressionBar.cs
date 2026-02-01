using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class PlankDispenser : MonoBehaviour
{
    [Header("1. References")]
    public GameObject plankPrefab;    
    public Transform spawnParent;     
    public RectTransform clickZone;   
    public RectTransform progressBar;
    public TextMeshProUGUI ammoText;  
    
    [Header("2. Settings")]
    public int startingAmmo = 3;
    public LayerMask invalidLayers;   
    public float minWidth = 1f;      
    public float maxWidth = 10f;     

    [Header("3. Level Progress")]
    public Transform levelStart;
    public Transform levelEnd;
    public Transform player;

   
    private int currentAmmo;
    private float currentProgress;
    private GameObject activePlank;   
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
        CalculateProgress();
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
    void TryStartDrag()
    {
        if (currentAmmo <= 0) return;
        if (IsMouseInZone(clickZone))
        {
            StartSpawning();
        }
    }

    void StartSpawning()
    {
        isDragging = true;
        Vector3 spawnPos = GetMouseWorldPos();
        activePlank = Instantiate(plankPrefab, spawnPos, Quaternion.identity, spawnParent);
        float width = Mathf.Lerp(minWidth, maxWidth, currentProgress);
        activePlank.transform.localScale = new Vector3(width, 0.5f, 1f);
        Collider2D col = activePlank.GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }

    void UpdateDragPosition()
    {
                activePlank.transform.position = GetMouseWorldPos();
    }

    void TryPlacePlank()
    {
        isDragging = false;
        if (activePlank == null) return;
        Collider2D col = activePlank.GetComponent<Collider2D>();
        if (col) col.enabled = true;
        Collider2D hit = Physics2D.OverlapBox(activePlank.transform.position, activePlank.transform.localScale, 0f, invalidLayers);

        if (hit != null)
        {
            
            Destroy(activePlank); 
        }
        else
        {
            Debug.Log("✅ Plank Placed!");
            currentAmmo--; 
            UpdateUI();
        }

        activePlank = null; 
    }

   

    void CalculateProgress()
    {
        if (player == null || levelStart == null || levelEnd == null) return;

        float totalDist = levelEnd.position.x - levelStart.position.x;
        float currentDist = player.position.x - levelStart.position.x;
        currentProgress = Mathf.Clamp01(currentDist / totalDist);

        if (progressBar != null)
        {
            progressBar.localScale = new Vector3(currentProgress, 1, 1);
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mPos.z = 0; 
        return mPos;
    }

    bool IsMouseInZone(RectTransform zone)
    {
        if (zone == null) return false;
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