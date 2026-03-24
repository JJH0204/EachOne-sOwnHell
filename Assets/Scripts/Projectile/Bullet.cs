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

    private Vector3 direction;
    private float   speed;
    private float   spawnTime;
    private bool    isDead;

    // ───────────────────────────────────────────────────────────
    public void Initialize(Vector3 dir, float spd, bool playerBullet)
    {
        direction      = dir;
        speed          = spd;
        isPlayerBullet = playerBullet;
        spawnTime      = Time.time;
        isDead         = false;
    }

    void Update()
    {
        if (isDead) return;
        transform.position += direction * speed * Time.deltaTime;

        if (Time.time - spawnTime > lifetime) Kill();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        // 다른 탄환과는 충돌 무시
        if (other.GetComponent<Bullet>() != null) return;

        if (isPlayerBullet)
        {
            // 플레이어 탄 → 적 피격
            var enemy = other.GetComponentInParent<EnemyController>();
            if (enemy != null) { enemy.TakeDamage(damage); Kill(); return; }
        }
        else
        {
            // 적 탄 → 플레이어 피격
            var stats = other.GetComponentInParent<PlayerStats>();
            if (stats != null) { stats.TakeDamage(damage); Kill(); return; }
        }

        // 벽 태그와 충돌 시 소멸
        if (other.CompareTag("Wall")) Kill();
    }

    void Kill()
    {
        isDead = true;
        Destroy(gameObject);
    }
}
