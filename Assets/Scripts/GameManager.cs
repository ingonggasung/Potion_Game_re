using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 게임의 전반적인 로직을 관리하는 매니저 클래스
/// 포션 제조, 오브젝트 드래그, 인벤토리 연동 등을 처리합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 레시피 데이터 구조
    /// 하나의 결과 아이템(result)에 대해 필요한 재료 목록(ingredients)을 가집니다.
    /// </summary>
    [System.Serializable]
    public class Recipe
    {
        /// <summary>레시피 결과물</summary>
        public Item result;

        /// <summary>이 결과물을 만들기 위해 필요한 재료 목록</summary>
        public List<Item> ingredients = new List<Item>();
    }

    /// <summary>포션을 만드는 Pot 오브젝트</summary>
    [Header("Pot Object")]
    public GameObject specificObject;
    
    /// <summary>재료를 섞는 Mix 버튼</summary>
    [Header("Mix Button")]
    public GameObject mixButton;
    
    /// <summary>인벤토리 UI 아이콘</summary>
    [Header("Inventory Button")]
    public GameObject InventoryIcon;

    /// <summary>현재 드래그 중인 월드 오브젝트</summary>
    private GameObject selectedObject = null;
    
    /// <summary>드래그 시작 시 오브젝트의 원래 위치</summary>
    private Vector3 originalPosition;
    
    /// <summary>메인 카메라 참조</summary>
    private Camera mainCam;

    /// <summary>각 오브젝트의 이동 허용 여부를 저장하는 딕셔너리</summary>
    private Dictionary<GameObject, bool> motionAllowed = new Dictionary<GameObject, bool>();
    
    /// <summary>Pot에 놓인 재료 오브젝트 리스트</summary>
    private List<GameObject> overlappedIngredients = new List<GameObject>();

    /// <summary>인벤토리에서 추가된 재료의 개수 (통계/디버그용)</summary>
    private int inventoryIngredientCount = 0;

    /// <summary>현재 Pot 안에 들어간 모든 재료 아이템 리스트 (월드+인벤토리)</summary>
    private List<Item> potItems = new List<Item>();

    /// <summary>레시피 목록 (대분류: 결과물, 소분류: 재료들)</summary>
    [Header("레시피 리스트")]
    public List<Recipe> recipes = new List<Recipe>();

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Start()
    {
        mainCam = Camera.main;
        if (InventoryIcon != null)
        {
            InventoryIcon.SetActive(false);
        }
    }

    /// <summary>
    /// 매 프레임마다 호출
    /// </summary>
    void Update()
    {
        UpdateMixButtonState();
        HandleDragInput();
    }

    /// <summary>
    /// 월드에 있는 재료 오브젝트의 드래그 입력을 처리합니다.
    /// </summary>
    private void HandleDragInput()
    {
        // UI 위에 마우스가 있으면 월드 오브젝트 드래그를 처리하지 않음
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // UI 위에 있으면 드래그 중인 오브젝트가 있어도 해제
            if (selectedObject != null)
            {
                selectedObject = null;
            }
            return;
        }

        // 마우스 버튼을 눌렀을 때
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            // 레이캐스트로 클릭한 오브젝트 확인
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                GameObject hitObj = hit.collider.gameObject;
                // "Ingredient" 태그를 가진 오브젝트만 드래그 가능
                if (hitObj.CompareTag("Ingredient"))
                {
                    bool allowed = true;
                    // 이미 Pot에 놓인 오브젝트는 드래그 불가
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

        // 마우스 버튼을 누르고 있는 동안 드래그 중인 오브젝트를 마우스 위치로 이동
        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;
            selectedObject.transform.position = mousePos;
        }

            // 마우스 버튼을 놓았을 때
            if (Input.GetMouseButtonUp(0) && selectedObject != null)
            {
                SpriteRenderer sr = selectedObject.GetComponent<SpriteRenderer>();

                // Pot 위에 놓였는지 확인
                if (IsOverlappingWithSpecificObject(selectedObject))
                {
                    // Pot 위치로 이동
                    Vector3 pos = specificObject.transform.position;
                    pos.z = -1f;
                    selectedObject.transform.position = pos;

                    // 더 이상 드래그 불가능하도록 설정
                    motionAllowed[selectedObject] = false;

                    // 재료 오브젝트 리스트에 추가
                    if (!overlappedIngredients.Contains(selectedObject))
                    {
                        overlappedIngredients.Add(selectedObject);
                    }

                    // 해당 오브젝트가 가지고 있는 아이템을 Pot 재료 리스트에 추가
                    IObjectItem objectItem = selectedObject.GetComponent<IObjectItem>();
                    if (objectItem != null)
                    {
                        Item item = objectItem.ClickItem();
                        if (item != null)
                        {
                            potItems.Add(item);
                        }
                    }

                    // 렌더링 순서 조정 (Pot 뒤에 표시)
                    if (sr != null)
                    {
                        sr.sortingOrder = -1;
                    }
                }
                else
                {
                    // 원래 위치로 돌아감
                    selectedObject.transform.position = originalPosition;
                    motionAllowed[selectedObject] = true;

                    // 렌더링 순서 복원
                    if (sr != null)
                    {
                        sr.sortingOrder = 1;
                    }
                }

                selectedObject = null;
            }
    }

    /// <summary>
    /// 오브젝트가 Pot과 겹치는지 확인합니다.
    /// </summary>
    /// <param name="obj">확인할 오브젝트</param>
    /// <returns>겹치면 true, 아니면 false</returns>
    private bool IsOverlappingWithSpecificObject(GameObject obj)
    {
        if (specificObject == null) return false;
        
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        Collider2D targetCollider = specificObject.GetComponent<Collider2D>();

        if (objCollider == null || targetCollider == null)
            return false;

        return objCollider.bounds.Intersects(targetCollider.bounds);
    }

    /// <summary>
    /// Mix 버튼의 활성화 상태를 업데이트합니다.
    /// 재료가 2개 이상일 때만 활성화됩니다.
    /// </summary>
    private void UpdateMixButtonState()
    {
        if (mixButton != null)
        {
            // Pot 안에 들어간 총 재료 개수 기준으로 Mix 버튼 활성화
            int totalCount = potItems.Count;
            mixButton.SetActive(totalCount > 1);
        }
    }

    /// <summary>
    /// Mix 버튼이 클릭되었을 때 호출됩니다.
    /// Pot에 있는 모든 재료를 제거하고 상태를 초기화합니다.
    /// </summary>
    public void Mix()
    {
        Debug.Log("Mix called. Objects to destroy: " + overlappedIngredients.Count);

        // 현재 Pot에 들어간 재료들로 레시피 검사
        Item resultItem = null;
        foreach (var recipe in recipes)
        {
            if (IsRecipeMatched(recipe))
            {
                resultItem = recipe.result;
                break;
            }
        }

        if (resultItem != null)
        {
            Debug.Log($"레시피 일치! 결과 아이템: {resultItem.itemName}");
            // TODO: 여기서 결과 아이템을 인벤토리에 추가하거나, 결과 오브젝트를 생성하는 등의 처리를 할 수 있습니다.
        }
        else
        {
            Debug.Log("레시피가 일치하지 않습니다. (실패 효과를 줄 수도 있음)");
        }

        // Pot에 놓인 모든 재료 오브젝트 제거
        foreach (var obj in overlappedIngredients)
        {
            if (obj != null)
            {
                Destroy(obj);
                Debug.Log("Destroyed: " + obj.name);
            }
        }
        overlappedIngredients.Clear();

        // 모든 오브젝트의 이동 허용 상태 초기화
        foreach (var key in new List<GameObject>(motionAllowed.Keys))
        {
            motionAllowed[key] = true;
            SpriteRenderer sr = key.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 1;
            }
        }

        // Pot 재료 리스트 및 카운트 초기화
        potItems.Clear();
        inventoryIngredientCount = 0;

        // Mix 버튼 상태 갱신
        UpdateMixButtonState();
    }

    /// <summary>
    /// 인벤토리 버튼이 클릭되었을 때 호출됩니다.
    /// 인벤토리 UI를 토글합니다.
    /// </summary>
    public void Inventorybtn()
    {
        Debug.Log("Inventory button clicked.");

        if (InventoryIcon != null)
        {
            if (InventoryIcon.activeSelf)
            {
                InventoryIcon.SetActive(false);
            }
            else
            {
                InventoryIcon.SetActive(true);
            }
        }
    }

    // ===== 인벤토리 드래그 앤 드롭을 위한 추가 메서드 =====

    /// <summary>
    /// 현재 마우스 위치가 Pot 위에 있는지 확인합니다.
    /// Unity 6의 FindFirstObjectByType을 사용합니다.
    /// </summary>
    /// <returns>Pot 위에 있으면 true, 아니면 false</returns>
    public bool IsMouseOverPot()
    {
        if (mainCam == null || specificObject == null) return false;
        
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Collider2D targetCollider = specificObject.GetComponent<Collider2D>();
        if (targetCollider == null) return false;

        return targetCollider.OverlapPoint(mousePos);
    }

    /// <summary>
    /// 인벤토리에서 드래그한 아이템을 Pot에 추가합니다.
    /// 재료 개수를 증가시키고 Mix 버튼 상태를 업데이트합니다.
    /// </summary>
    /// <param name="item">추가할 아이템</param>
    public void AddIngredientFromInventory(Item item)
    {
        if (item == null) return;

        // Pot 재료 리스트에 아이템 추가
        potItems.Add(item);
        inventoryIngredientCount++;

        Debug.Log($"인벤토리에서 재료 추가됨: {item.itemName} (총 {inventoryIngredientCount}개, Pot 재료 수: {potItems.Count})");
        
        // Mix 버튼 상태 즉시 업데이트
        UpdateMixButtonState();
    }

    /// <summary>
    /// 현재 Pot에 들어간 재료 목록이 주어진 레시피와 일치하는지 확인합니다.
    /// 개수와 구성(아이템 종류)이 모두 같아야 합니다. (순서는 상관 없음)
    /// </summary>
    /// <param name="recipe">비교할 레시피</param>
    /// <returns>일치하면 true, 아니면 false</returns>
    private bool IsRecipeMatched(Recipe recipe)
    {
        if (recipe == null || recipe.result == null || recipe.ingredients == null)
            return false;

        if (recipe.ingredients.Count != potItems.Count)
            return false;

        // Pot 재료 리스트를 복사해 하나씩 제거하는 방식으로 비교 (멀티셋 비교)
        List<Item> tempList = new List<Item>(potItems);

        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient == null) return false;

            if (!tempList.Remove(ingredient))
            {
                // 필요한 재료가 Pot 안에 충분히 없으면 실패
                return false;
            }
        }

        // 모든 재료를 성공적으로 제거했다면 완전히 일치
        return true;
    }
}
