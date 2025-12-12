using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject specificObject;  // 솥 또는 타겟 오브젝트
    public GameObject mixButton;        // Mix 버튼 (Inspector에서 연결)
    public GameObject InventoryIcon;   // 인벤토리 아이콘 연결, 활성화 여부 확인용

    private GameObject selectedObject = null;
    private Vector3 originalPosition;
    private Camera mainCam;

    private Dictionary<GameObject, bool> motionAllowed = new Dictionary<GameObject, bool>();
    private List<GameObject> overlappedIngredients = new List<GameObject>();

    void Start()
    {
        mainCam = Camera.main;
        InventoryIcon.SetActive(false);
    }

    void Update()
    {
        UpdateMixButtonState();
        HandleDragInput();
    }

    private void HandleDragInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GameObject hitObj = hit.collider.gameObject;
                // 오브젝트의 태그가 Ingredient일 때만 움직임 허용
                if (hitObj.CompareTag("Ingredient"))
                {
                    bool allowed = true;
                    if (motionAllowed.TryGetValue(hitObj, out allowed) && !allowed) return;

                    selectedObject = hitObj;
                    originalPosition = selectedObject.transform.position;
                }
                else
                {
                    selectedObject = null;
                }
            }
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            selectedObject.transform.position = mousePos;
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            SpriteRenderer sr = selectedObject.GetComponent<SpriteRenderer>();

            if (IsOverlappingWithSpecificObject(selectedObject))
            {
                Vector3 pos = specificObject.transform.position;
                pos.z = -1f;
                selectedObject.transform.position = pos;

                motionAllowed[selectedObject] = false;

                if (!overlappedIngredients.Contains(selectedObject))
                {
                    overlappedIngredients.Add(selectedObject);
                }

                // order in layer를 -1로 설정 (겹친 상태)
                if (sr != null)
                {
                    sr.sortingOrder = -1;
                }
            }
            else
            {
                selectedObject.transform.position = originalPosition;
                motionAllowed[selectedObject] = true;

                // order in layer를 1로 설정 (겹치지 않은 상태)
                if (sr != null)
                {
                    sr.sortingOrder = 1;
                }
            }

            selectedObject = null;
        }
    }

    private bool IsOverlappingWithSpecificObject(GameObject obj)
    {
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        Collider2D targetCollider = specificObject.GetComponent<Collider2D>();

        if (objCollider == null || targetCollider == null)
            return false;

        return objCollider.bounds.Intersects(targetCollider.bounds);
    }

    private void UpdateMixButtonState()
    {
        if (mixButton != null)
        {
            mixButton.SetActive(overlappedIngredients.Count > 1);
        }
    }

    public void Mix() // 섞기 버튼에서 호출
    {
        Debug.Log("Mix called. Objects to destroy: " + overlappedIngredients.Count);
        foreach (var obj in overlappedIngredients)
        {
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log("Destroyed: " + obj.name);
            }
        }
        overlappedIngredients.Clear();

        // 움직임 전체 허용 초기화 로직 등
        foreach (var key in new List<GameObject>(motionAllowed.Keys))
        {
            motionAllowed[key] = true;
            SpriteRenderer sr = key.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 1;
            }
        }
    }

    public void Inventorybtn()
    {
        Debug.Log("Inventory button clicked.");

        if(InventoryIcon.activeSelf) // 활성화 되어있는 상태에서 클릭 시 비활성화
        {
            InventoryIcon.SetActive(false);
        }
        else // 비활성화 되어있는 상태에서 클릭 시 활성화
        {
            InventoryIcon.SetActive(true);
        }        
    }
}
