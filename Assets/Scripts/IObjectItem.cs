using UnityEngine;

/// <summary>
/// 월드에 배치된 아이템 오브젝트가 구현해야 하는 인터페이스
/// 플레이어가 오브젝트를 클릭했을 때 아이템을 반환합니다.
/// </summary>
public interface IObjectItem
{
    /// <summary>
    /// 오브젝트를 클릭했을 때 해당 오브젝트의 아이템을 반환합니다.
    /// </summary>
    /// <returns>오브젝트가 가지고 있는 아이템</returns>
    Item ClickItem();
}
