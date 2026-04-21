using UnityEngine;

/// <summary>
/// 각자의 지옥 - 탄환 생성 중계자
///
/// 이벤트 버스:
///   구독 SpawnBulletRequestEvent - 요청을 받아 BulletHelper.Spawn 실행
/// </summary>
public class BulletSpawner : MonoBehaviour
{
    void OnEnable()  => EventBus<SpawnBulletRequestEvent>.Subscribe(OnSpawnRequested);
    void OnDisable() => EventBus<SpawnBulletRequestEvent>.Unsubscribe(OnSpawnRequested);

    void OnSpawnRequested(SpawnBulletRequestEvent evt)
    {
        BulletHelper.Spawn(evt.Origin, evt.Direction, evt.Speed, evt.IsPlayerBullet, evt.IsAutoAim);
    }
}