using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 인벤토리 시스템을 관리하는 클래스
/// 아이템 추가, 제거, 드래그 앤 드롭 기능을 제공합니다.
/// </summary>
public class Inventory : MonoBehaviour
{
    /// <summary>인벤토리에 보관된 아이템 리스트</summary>
    public List<Item> items;

    /// <summary>슬롯들의 부모 Transform</summary>
    [SerializeField] private Transform slotParent;
    
    /// <summary>인벤토리 슬롯 배열</summary>
    [SerializeField] private Slot[] slots;

    /// <summary>드래그 중인 아이템을 표시하는 UI</summary>
    [Header("드래그 UI")]
    public DragItemUI dragItemUI;

    /// <summary>현재 드래그 중인 아이템</summary>
    private Item currentDragItem;
    
    /// <summary>현재 드래그 중인 슬롯의 인덱스 (아이템 제거를 위해 필요)</summary>
    private int currentDragSlotIndex = -1;

#if UNITY_EDITOR
    /// <summary>
    /// 에디터에서 인스펙터 값이 변경될 때 자동으로 호출
    /// 슬롯 배열을 자동으로 갱신합니다.
    /// </summary>
    private void OnValidate()
    {
        if (slotParent != null)
        {
            slots = slotParent.GetComponentsInChildren<Slot>();
        }
    }
#endif

    /// <summary>
    /// 게임 시작 시 초기화
    /// </summary>
    void Awake()
    {
        FreshSlot();
        if (dragItemUI != null)
            dragItemUI.Hide();
    }

    /// <summary>
    /// 매 프레임마다 호출
    /// 드래그 종료를 감지합니다.
    /// </summary>
    void Update()
    {
        // 마우스 버튼을 놓았을 때 드래그 종료 처리
        if (currentDragItem != null && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            EndDrag();
        }
    }

    /// <summary>
    /// 슬롯들을 현재 아이템 리스트 상태에 맞게 갱신합니다.
    /// </summary>
    public void FreshSlot()
    {
        int i = 0;
        // 아이템이 있는 슬롯들에 아이템 할당
        for (; i < items.Count && i < slots.Length; i++)
        {
            slots[i].item = items[i];
        }
        // 나머지 빈 슬롯들은 null로 설정
        for (; i < slots.Length; i++)
        {
            slots[i].item = null;
        }
    }

    /// <summary>
    /// 인벤토리에 아이템을 추가합니다.
    /// </summary>
    /// <param name="_item">추가할 아이템</param>
    public void AddItem(Item _item)
    {
        if (items.Count < slots.Length)
        {
            items.Add(_item);
            FreshSlot();
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다.");
        }
    }

    /// <summary>
    /// 슬롯이 클릭되었을 때 호출됩니다.
    /// 아이템 드래그를 시작합니다.
    /// </summary>
    /// <param name="slot">클릭된 슬롯</param>
    public void OnSlotClicked(Slot slot)
    {
        Debug.Log($"Inventory OnSlotClicked 호출됨");
        
        if (slot.item == null)
        {
            Debug.Log("Inventory: 슬롯에 아이템이 없음");
            return;
        }

        Debug.Log($"Inventory: 드래그 시작 - {slot.item.itemName}");

        // 드래그할 아이템과 슬롯 인덱스 저장
        currentDragItem = slot.item;
        
        // 슬롯 인덱스 찾기
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == slot)
            {
                currentDragSlotIndex = i;
                Debug.Log($"Inventory: 슬롯 인덱스 {i} 찾음");
                break;
            }
        }

        // 드래그 UI 표시
        if (dragItemUI != null)
        {
            Debug.Log("Inventory: DragItemUI.Show 호출");
            dragItemUI.Show(currentDragItem.itemImage);
        }
        else
        {
            Debug.LogError("Inventory: dragItemUI가 null입니다!");
        }
    }

    /// <summary>
    /// 드래그가 종료되었을 때 호출됩니다.
    /// Pot에 드롭되었는지 확인하고 아이템을 추가합니다.
    /// 아이템은 인벤토리에서 제거되지 않습니다 (무한정 사용 가능).
    /// </summary>
    private void EndDrag()
    {
        // Pot 위에 마우스가 있는지 확인
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && currentDragItem != null && gm.IsMouseOverPot())
        {
            // Pot에 아이템을 추가 (인벤토리에서는 제거하지 않음)
            gm.AddIngredientFromInventory(currentDragItem);
            Debug.Log($"Pot에 {currentDragItem.itemName} 추가됨 (인벤토리에는 그대로 유지)");
        }

        // 드래그 UI 숨기기
        if (dragItemUI != null)
            dragItemUI.Hide();

        // 드래그 상태 초기화
        currentDragItem = null;
        currentDragSlotIndex = -1;
    }

    /// <summary>
    /// 현재 드래그 중인 아이템을 반환합니다.
    /// 외부에서 드래그 상태를 확인할 때 사용합니다.
    /// </summary>
    /// <returns>현재 드래그 중인 아이템, 없으면 null</returns>
    public Item TakeCurrentDragItem()
    {
        return currentDragItem;
    }
}
