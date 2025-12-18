using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 드래그 중인 아이템을 마우스를 따라 표시하는 UI 클래스
/// </summary>
public class DragItemUI : MonoBehaviour
{
    /// <summary>UI가 속한 Canvas</summary>
    [SerializeField] private Canvas canvas;
    
    /// <summary>드래그 중인 아이템 이미지를 표시하는 Image 컴포넌트</summary>
    [SerializeField] private Image image;

    /// <summary>현재 드래그 중인지 여부</summary>
    private bool isDragging = false;

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Awake()
    {
        Hide();
        
        // Canvas가 할당되지 않았으면 자동으로 찾기
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
        }
    }

    /// <summary>
    /// 매 프레임마다 호출
    /// 드래그 중일 때 마우스 위치를 따라가도록 합니다.
    /// </summary>
    void Update()
    {
        // 드래그 중이 아니면 업데이트하지 않음
        if (!isDragging) return;

        // 마우스 위치로 UI 업데이트
        UpdatePosition();
    }

    /// <summary>
    /// 드래그 UI를 표시하고 마우스를 따라가기 시작합니다.
    /// </summary>
    /// <param name="sprite">표시할 아이템 스프라이트</param>
    public void Show(Sprite sprite)
    {
        Debug.Log($"DragItemUI.Show 호출됨 - Sprite: {(sprite != null ? sprite.name : "null")}");
        
        if (image == null)
        {
            Debug.LogError("DragItemUI: image가 null입니다!");
            return;
        }
        
        // Canvas가 없으면 찾기
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }
        }
        
        if (canvas == null)
        {
            Debug.LogError("DragItemUI: Canvas를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"DragItemUI: Canvas 찾음 - {canvas.name}");
        
        image.sprite = sprite;
        image.color = Color.white;
        image.enabled = true;
        isDragging = true;
        
        // 마우스 위치로 즉시 이동
        UpdatePosition();
        
        Debug.Log($"DragItemUI: 드래그 시작됨, isDragging={isDragging}, Canvas RenderMode={canvas.renderMode}");
    }
    
    /// <summary>
    /// UI 위치를 마우스 위치로 업데이트합니다.
    /// </summary>
    private void UpdatePosition()
    {
        if (canvas == null) return;
        
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform == null) return;
        
        Vector2 pos;
        bool success = false;
        
        // Canvas의 Render Mode에 따라 다른 방식으로 처리
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Screen Space - Overlay 모드: worldCamera가 null
            success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null, // Overlay 모드에서는 카메라가 필요 없음
                out pos
            );
        }
        else
        {
            // Screen Space - Camera 또는 World Space 모드: worldCamera 필요
            Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
            success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                cam,
                out pos
            );
        }
        
        if (success)
        {
            rectTransform.anchoredPosition = pos;
        }
    }

    /// <summary>
    /// 드래그 UI를 숨깁니다.
    /// </summary>
    public void Hide()
    {
        if (image == null) return;
        
        image.enabled = false;
        isDragging = false;
    }
}
