using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 각자의 지옥 - 아이템 오브 처리
///
/// 역할:
///   - 플레이어가 아이템 오브와 충돌했는지 확인합니다.
///   - 플레이어가 오브를 획득하면 ItemOrbPickedUpEvent를 발행합니다.
///   - 이벤트 발행 후 오브 오브젝트를 제거합니다.
///
/// TODO:
///   - 추후 아이템은 데이터 테이블 또는 드롭 설정에서 관리하도록 변경할 예정입니다.
/// </summary>

public class ItemOrb : MonoBehaviour
{
    [FormerlySerializedAs("Item")] [SerializeField] private int item = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        EventBus<ItemOrbPickedUpEvent>.Raise(new ItemOrbPickedUpEvent(item));
        Destroy(gameObject);
    }
}
