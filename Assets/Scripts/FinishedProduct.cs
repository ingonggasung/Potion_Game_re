using UnityEngine;

/// <summary>
/// 아이템 데이터를 저장하는 ScriptableObject 클래스
/// Unity 에디터에서 아이템을 생성하고 관리할 수 있습니다.
/// </summary>
[CreateAssetMenu]
public class FinishedProduct : ScriptableObject
{
    /// <summary>완성 아이템의 이름</summary>
    public string itemName;

    /// <summary>완성 아이템의 이미지 스프라이트</summary>
    public Sprite itemImage;
}
