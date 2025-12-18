using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리의 개별 슬롯을 관리하는 클래스
/// 아이템 표시 및 클릭 이벤트를 처리합니다.
/// </summary>
public class Slot : MonoBehaviour, IPointerDownHandler
{
    /// <summary>아이템 이미지를 표시하는 Image 컴포넌트</summary>
    [SerializeField] Image image;
    
    /// <summary>이 슬롯에 보관된 아이템</summary>
    private Item _item;

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Awake()
    {
        // Image가 할당되지 않았으면 자동으로 찾거나 추가
        if (image == null)
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                // 자식 오브젝트에서 Image 찾기
                image = GetComponentInChildren<Image>();
            }
            if (image == null)
            {
                // Image 컴포넌트가 없으면 추가 (UI 이벤트를 받기 위해 필요)
                image = gameObject.AddComponent<Image>();
                image.color = new Color(1, 1, 1, 0); // 투명하게 시작
            }
        }
        
        // Raycast Target이 꺼져있으면 켜기 (UI 이벤트를 받기 위해 필요)
        if (image != null)
        {
            image.raycastTarget = true;
        }
    }

    /// <summary>
    /// 슬롯에 보관된 아이템을 가져오거나 설정합니다.
    /// 아이템이 설정되면 자동으로 UI가 업데이트됩니다.
    /// </summary>
    public Item item
    {
        get { return _item; }
        set
        {
            _item = value;
            if (image == null) return;
            
            if (_item != null)
            {
                // 아이템이 있으면 이미지 표시
                image.sprite = _item.itemImage;
                image.color = new Color(1, 1, 1, 1); // 완전 불투명
            }
            else
            {
                // 아이템이 없으면 투명하게 처리
                image.color = new Color(1, 1, 1, 0); // 완전 투명
            }
        }
    }

    /// <summary>
    /// 슬롯에 마우스를 누를 때 호출됩니다.
    /// 인벤토리 매니저에 드래그 시작 이벤트를 전달합니다.
    /// </summary>
    /// <param name="eventData">포인터 이벤트 데이터</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"Slot OnPointerDown 호출됨 - 아이템: {(_item != null ? _item.itemName : "null")}");
        
        // 아이템이 없으면 무시
        if (_item == null)
        {
            Debug.Log("Slot: 아이템이 없어서 무시됨");
            return;
        }
        
        // 부모 오브젝트에서 Inventory 컴포넌트 찾기
        Inventory inventory = GetComponentInParent<Inventory>();
        if (inventory != null)
        {
            Debug.Log($"Slot: Inventory 찾음, OnSlotClicked 호출");
            // 인벤토리에 슬롯 클릭 이벤트 전달 (드래그 시작)
            inventory.OnSlotClicked(this);
        }
        else
        {
            Debug.LogError("Slot: Inventory 컴포넌트를 찾을 수 없습니다!");
        }
    }
}
