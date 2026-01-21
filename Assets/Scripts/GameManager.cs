using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

// 게임의 전반적인 로직을 관리하는 매니저 클래스
// 포션 제조, 오브젝트 드래그, 인벤토리 연동 등을 처리합니다.
public class GameManager : MonoBehaviour
{
    // 레시피 데이터 구조
    // 하나의 결과 아이템(result)에 대해 필요한 재료 목록(ingredients)을 가집니다.
    [System.Serializable]
    public class Recipe
    {
        // 레시피 결과물
        public FinishedProduct result;

        // 이 결과물을 만들기 위해 필요한 재료 목록
        public List<Item> ingredients = new List<Item>();
    }

    // 포션을 만드는 Pot 오브젝트
    [Header("Pot Object")]
    public GameObject specificObject;
    
    // 재료를 섞는 Mix 버튼
    [Header("Mix Button")]
    public GameObject mixButton;
    
    // 인벤토리 UI 아이콘
    //[Header("Inventory Button")]
    //public GameObject InventoryIcon;

    // 현재 드래그 중인 월드 오브젝트
    private GameObject selectedObject = null;
    
    // 드래그 시작 시 오브젝트의 원래 위치
    private Vector3 originalPosition;
    
    // 메인 카메라 참조
    private Camera mainCam;

    // New Input System actions
    private InputAction clickAction;
    private InputAction pointerPositionAction;

    // 각 오브젝트의 이동 허용 여부를 저장하는 딕셔너리
    private Dictionary<GameObject, bool> motionAllowed = new Dictionary<GameObject, bool>();
    
    // Pot에 놓인 재료 오브젝트 리스트
    private List<GameObject> overlappedIngredients = new List<GameObject>();

    // 인벤토리에서 추가된 재료의 개수 (통계/디버그용)
    private int inventoryIngredientCount = 0;

    // 현재 Pot 안에 들어간 모든 재료 아이템 리스트 (월드+인벤토리)
    private List<Item> potItems = new List<Item>();

    // 레시피 목록 (대분류: 결과물, 소분류: 재료들)
    [Header("레시피 리스트")]
    public List<Recipe> recipes = new List<Recipe>();

    // 결과물 생성 관련 설정
    [Header("결과물 생성 설정")]
    [Tooltip("완성 아이템이 생성될 위치 (빈 오브젝트를 여기에 할당)")]
    public Transform resultSpawnPosition;
    
    [Tooltip("결과물이 이동할 거리 (Y축 양수 방향)")]
    public float resultMoveDistance = 1f;


    // 게임 시작 시 초기화
    void Start()
    {
        mainCam = Camera.main;
        //if (InventoryIcon != null)
        //{
        //    InventoryIcon.SetActive(false);
        //}
    }

    private void OnEnable()
    {
        SetupInputActions();
    }

    private void OnDisable()
    {
        clickAction?.Disable();
        pointerPositionAction?.Disable();
    }

    // Input System 액션 초기화 및 활성화
    private void SetupInputActions()
    {
        if (clickAction == null)
        {
            clickAction = new InputAction("Click", binding: "<Mouse>/leftButton");
        }
        if (pointerPositionAction == null)
        {
            pointerPositionAction = new InputAction("PointerPosition", binding: "<Pointer>/position");
        }

        clickAction.Enable();
        pointerPositionAction.Enable();
    }

    // 매 프레임마다 호출

    void Update()
    {
        UpdateMixButtonState();
        HandleDragInput();
    }

    // 월드에 있는 재료 오브젝트의 드래그 입력을 처리합니다.

    private void HandleDragInput()
    {
        if (clickAction == null || pointerPositionAction == null) return;

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

        // 마우스/포인터 입력 상태
        bool clickDown = clickAction != null && clickAction.WasPressedThisFrame();
        bool clickHeld = clickAction != null && clickAction.IsPressed();
        bool clickUp = clickAction != null && clickAction.WasReleasedThisFrame();

        Vector3 mousePos = mainCam.ScreenToWorldPoint(pointerPositionAction.ReadValue<Vector2>());
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        // 마우스 버튼을 눌렀을 때
        if (clickDown)
        {

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
        if (clickHeld && selectedObject != null)
        {
            mousePos.z = 0f;
            selectedObject.transform.position = mousePos;
        }

            // 마우스 버튼을 놓았을 때
            if (clickUp && selectedObject != null)
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

    // 오브젝트가 Pot과 겹치는지 확인합니다.
    // <param name="obj">확인할 오브젝트</param>
    // <returns>겹치면 true, 아니면 false</returns>
    private bool IsOverlappingWithSpecificObject(GameObject obj)
    {
        if (specificObject == null) return false;
        
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        Collider2D targetCollider = specificObject.GetComponent<Collider2D>();

        if (objCollider == null || targetCollider == null)
            return false;

        return objCollider.bounds.Intersects(targetCollider.bounds);
    }

    // Mix 버튼의 활성화 상태를 업데이트합니다.
    // 재료가 2개 이상일 때만 활성화됩니다.
    private void UpdateMixButtonState()
    {
        if (mixButton != null)
        {
            // Pot 안에 들어간 총 재료 개수 기준으로 Mix 버튼 활성화
            int totalCount = potItems.Count;
            mixButton.SetActive(totalCount > 1);
        }
    }

  
    // Mix 버튼이 클릭되었을 때 호출됩니다.
    // Pot에 있는 모든 재료를 제거하고 상태를 초기화합니다.
    public void Mix()
    {
        Debug.Log("Mix called. Objects to destroy: " + overlappedIngredients.Count);

        // 현재 Pot에 들어간 재료들로 레시피 검사
        FinishedProduct resultItem = null;
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
            // 결과물을 Pot에서 생성하고 위로 이동시킴
            CreateAndLaunchResult(resultItem);
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

    // 인벤토리 버튼이 클릭되었을 때 호출됩니다.
    // 인벤토리 UI를 토글합니다.
    //public void Inventorybtn()
    //{
    //    Debug.Log("Inventory button clicked.");

    //    if (InventoryIcon != null)
    //    {
    //        if (InventoryIcon.activeSelf)
    //        {
    //            InventoryIcon.SetActive(false);
    //        }
    //        else
    //        {
    //            InventoryIcon.SetActive(true);
    //        }
    //    }
    //}

    // ===== 인벤토리 드래그 앤 드롭을 위한 추가 메서드 =====

    // 현재 마우스 위치가 Pot 위에 있는지 확인합니다.
    // Unity 6의 FindFirstObjectByType을 사용합니다.
    // <returns>Pot 위에 있으면 true, 아니면 false</returns>
    public bool IsMouseOverPot()
    {
        if (mainCam == null || specificObject == null) return false;
        
        // Input System을 사용하여 마우스 위치 가져오기
        Vector2 mouseScreenPos;
        if (pointerPositionAction != null)
        {
            mouseScreenPos = pointerPositionAction.ReadValue<Vector2>();
        }
        else if (Mouse.current != null)
        {
            mouseScreenPos = Mouse.current.position.ReadValue();
        }
        else
        {
            return false;
        }
        
        Vector3 mousePos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        mousePos.z = 0f;

        Collider2D targetCollider = specificObject.GetComponent<Collider2D>();
        if (targetCollider == null) return false;

        return targetCollider.OverlapPoint(mousePos);
    }


    // 인벤토리에서 드래그한 아이템을 Pot에 추가합니다.
    // 재료 개수를 증가시키고 Mix 버튼 상태를 업데이트합니다.
    // <param name="item">추가할 아이템</param>
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

    // 현재 Pot에 들어간 재료 목록이 주어진 레시피와 일치하는지 확인합니다.
    // 개수와 구성(아이템 종류)이 모두 같아야 합니다. (순서는 상관 없음)
    // <param name="recipe">비교할 레시피</param>
    // <returns>일치하면 true, 아니면 false</returns>
    private bool IsRecipeMatched(Recipe recipe)
    {
        if (recipe == null || recipe.result == null || recipe.ingredients == null)
            return false;

        if (recipe.ingredients.Count != potItems.Count)
            return false;

        // Pot 재료 리스트를 복사해 하나씩 제거하는 방식으로 비교 (멀티셋 비교)
        // 아이템 이름으로 비교하여 순서와 무관하게 매칭
        List<Item> tempList = new List<Item>(potItems);

        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient == null) return false;

            // 이름으로 일치하는 아이템 찾기
            Item foundItem = tempList.Find(item => item != null && item.itemName == ingredient.itemName);
            if (foundItem == null)
            {
                // 필요한 재료가 Pot 안에 충분히 없으면 실패
                return false;
            }
            
            // 찾은 아이템을 리스트에서 제거
            tempList.Remove(foundItem);
        }

        // 모든 재료를 성공적으로 제거했다면 완전히 일치
        return true;
    }

    // 결과물을 생성하고 지정된 위치에서 위로 이동시킵니다.
    // FinishedProduct의 정보만을 활용하여 완성품을 표시합니다.
    // <param name="resultItem">생성할 결과물 아이템 (FinishedProduct)</param>
    private void CreateAndLaunchResult(FinishedProduct resultItem)
    {
        if (resultItem == null) return;

        // 완성 아이템이 나올 위치 확인
        Vector3 startPos;
        if (resultSpawnPosition != null)
        {
            // 지정된 위치 사용
            startPos = resultSpawnPosition.position;
        }
        else if (specificObject != null)
        {
            // 위치가 지정되지 않았으면 Pot 위치 사용 (기본값)
            startPos = specificObject.transform.position;
        }
        else
        {
            Debug.LogWarning("완성 아이템 생성 위치가 지정되지 않았습니다.");
            return;
        }

        // FinishedProduct의 정보만으로 결과물 GameObject 생성
        // 완성품을 보여주는 역할만 수행
        GameObject resultObj = new GameObject("FinishedProduct_" + resultItem.itemName);
        SpriteRenderer sr = resultObj.AddComponent<SpriteRenderer>();
        resultObj.AddComponent<CircleCollider2D>();

        // 지정된 위치에서 시작
        startPos.z = 0f;
        resultObj.transform.position = startPos;

        // FinishedProduct의 itemImage를 SpriteRenderer에 설정
        if (resultItem.itemImage != null)
        {
            sr.sprite = resultItem.itemImage;
            sr.sortingOrder = 10; // 다른 오브젝트 위에 표시
        }

        // 결과물 이동 애니메이션 시작
        StartCoroutine(LaunchResultCoroutine(resultObj, startPos));
    }

    // 결과물을 위로 이동시키는 코루틴
    // 처음 빠른 속도로 시작해서 점차 감속합니다.
    // 가속(속도)이 0이 되면 완성품이 사라집니다.
    // <param name="resultObj">이동시킬 결과물 오브젝트</param>
    // <param name="startPos">시작 위치</param>
    private IEnumerator LaunchResultCoroutine(GameObject resultObj, Vector3 startPos)
    {
        if (resultObj == null) yield break;

        float elapsedTime = 0f;
        float duration = 0.5f; // 전체 애니메이션 시간
        float initialSpeed = resultMoveDistance * 6f; // 초기 속도 (거리의 6배)
        float currentSpeed = initialSpeed;
        float deceleration = initialSpeed / duration; // 감속량

        Vector3 currentPos = startPos;

        while (resultObj != null)
        {
            elapsedTime += Time.deltaTime;
            
            // 현재 속도 계산 (점차 감속)
            currentSpeed = Mathf.Max(0f, initialSpeed - (deceleration * elapsedTime));
            
            // 속도가 0이 되면 완성품 사라짐
            if (currentSpeed <= 0f)
            {
                if (resultObj != null)
                {
                    Destroy(resultObj);
                }
                yield break;
            }
            
            // 이동 거리 계산 (감속 곡선 적용)
            float moveDistance = currentSpeed * Time.deltaTime;
            currentPos += Vector3.up * moveDistance;

            resultObj.transform.position = currentPos;
            yield return null;
        }
    }
}
