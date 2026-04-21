using UnityEngine;

/// <summary>
/// 각자의 지옥 - 탄환 컴포넌트
/// BulletHelper.Spawn() 에 의해 Sphere 프리미티브에 부착됩니다.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Bullet : MonoBehaviour
{
    [HideInInspector] public float damage        = 10f;
    [HideInInspector] public float lifetime      = 5f;
    [HideInInspector] public bool  isPlayerBullet;

    private Vector3 _direction;
    private float   _speed;
    private float   _spawnTime;
    private bool    _isDead;
    private Rigidbody _rb;

    // ───────────────────────────────────────────────────────────
    public void Initialize(Vector3 dir, float spd, bool playerBullet)
    {
        _direction      = dir;
        _speed          = spd;
        isPlayerBullet = playerBullet;
        _spawnTime      = Time.time;
        _isDead         = false;
    }

    private void Update()
    {
        if (_isDead) return;
        if (Time.time - _spawnTime > lifetime) Kill();


        // 1. lifetime 체크 (안전장치)
        // 2. 이동 + 충돌 체크
        var move = _direction * (_speed * Time.deltaTime);

        if (Physics.Linecast(transform.position, transform.position + move, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                Kill();
                return;
            }
        }

        transform.position += move;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        // 다른 탄환과는 충돌 무시
        if (other.GetComponent<Bullet>() != null) return;

        if (isPlayerBullet)
        {
            // 플레이어 탄 → 적 피격 (EnemyController 타입 불필요 — 태그+루트 GameObject 전달)
            var root = other.transform.root;
            if (!root.CompareTag("Enemy")) return;
            EventBus<BulletHitEnemyEvent>.Raise(new BulletHitEnemyEvent(root.gameObject, damage));
        }
        else
        {
            // 적 탄 → 플레이어 피격 (루트 오브젝트 태그만 확인 — PlayerStats 직접 참조 불필요)
            if (!other.transform.root.CompareTag("Player")) return;
            EventBus<BulletHitPlayerEvent>.Raise(new BulletHitPlayerEvent(damage));
        }

        Kill();
    }

    private void Kill()
    {
        _isDead = true;
        gameObject.SetActive(false);
    }
}
