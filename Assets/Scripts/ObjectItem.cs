using UnityEngine;

/// <summary>
/// 월드에 배치된 아이템 오브젝트 클래스
/// 플레이어가 클릭하면 인벤토리에 추가할 수 있는 아이템을 제공합니다.
/// </summary>
public class ObjectItem : MonoBehaviour, IObjectItem
{
    /// <summary>이 오브젝트가 제공하는 아이템</summary>
    [Header("아이템")]
    public Item item;
    
    /// <summary>월드에 표시될 아이템 이미지</summary>
    [Header("아이템 이미지")]
    public SpriteRenderer itemImage;

    /// <summary>
    /// 게임 시작 시 아이템 이미지를 설정합니다.
    /// </summary>
    void Start()
    {
        if (item != null && itemImage != null)
        {
            itemImage.sprite = item.itemImage;
        }
    }
    
    /// <summary>
    /// 오브젝트를 클릭했을 때 호출됩니다.
    /// 이 오브젝트가 가지고 있는 아이템을 반환합니다.
    /// </summary>
    /// <returns>이 오브젝트의 아이템</returns>
    public Item ClickItem()
    {
        return this.item;
    }
}
