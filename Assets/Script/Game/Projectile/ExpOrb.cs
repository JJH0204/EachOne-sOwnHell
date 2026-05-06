using UnityEngine;

/// <summary>
/// 각자의 지옥 - 경험치 획득 처리
///
/// 역할:
///   - 플레이어가 경험치 오브와 충돌했는지 확인합니다.
///   - 플레이어가 오브를 획득하면 ExpOrbPickedUpEvent를 발행합니다.
///   - 이벤트 발행 후 오브 오브젝트를 제거합니다.
///
/// TODO:
///   - 추후 경험치량은 데이터 테이블 또는 드롭 설정에서 관리하도록 변경할 예정입니다.
/// </summary>

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private int expAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        EventBus<ExpOrbPickedUpEvent>.Raise(new ExpOrbPickedUpEvent(expAmount));
        Destroy(gameObject);
    }
}
