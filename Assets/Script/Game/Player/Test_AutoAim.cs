using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 각자의 지옥 - 플레이어 자동공격 관리
///
/// 역할:
///   - 플레이어 자동 공격(총알 발사,근접 공격)을 처리합니다
///   - 설정에서 플레이어가 수동 공격으로 바뀔시 수동 공격이 되도록 처리합니다
///   - 플레이어가 스킬 사용시 공격이 안나가도록 처리합니다
/// TODO:
///   - 설정 스크립트가 완성 된 후 설정에서 자동공격 <-> 수동공격 변경 가능하도록 처리할것
///   - 추후 가능하다면 스크립트 한번 정리 할것 불가능하다 싶음 현 상태에서 냅두고 최소한으로만 개선 할 예정( 현재 목업 버전이며 작업시 프로토타입에 맞게 현 스크립트를 변경 할 예정 )
/// </summary>

public class TestAutoAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;

    [Header("Auto Fire Settings")]
    [SerializeField] private float fireInterval = 0.3f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float bulletSpeed = 12f;

    [FormerlySerializedAs("Skill")] [SerializeField] private PlayerLaserAttack skill;


    [FormerlySerializedAs("Stats")] public PlayerStats stats;
    private PlayerDeath _death;
    private float _nextFireTime;


    private void Start()
    {
        stats = GetComponentInParent<PlayerStats>();
        skill = GetComponentInParent<PlayerLaserAttack>();
        _death = GetComponent<PlayerDeath>();
    }

    private void Update()
    {

        if (_death && _death.isDead)
            return;


        if (Time.time < _nextFireTime)
            return;

        if (skill && skill.IsUsingSkill)
            return;

        if (stats.currentHp <= 0)
        {
            enabled = false;
            return;
        }

        Transform target = FindClosestEnemy();

        if (target == null)
            return;

        float sqrDist = (target.position - transform.position).sqrMagnitude;
        if (sqrDist > attackRange * attackRange)
            return;

        Fire(target);
        _nextFireTime = Time.time + fireInterval;
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy)
                continue;

            float sqrDist = (enemy.transform.position - transform.position).sqrMagnitude;

            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                closest = enemy.transform;
            }
        }

        return closest;
    }

    void Fire(Transform target)
    {
        Transform shootOrigin = firePoint != null ? firePoint : transform;
        Vector3 direction = (target.position - shootOrigin.position).normalized;

        EventBus<SpawnBulletRequestEvent>.Raise(
            new SpawnBulletRequestEvent(shootOrigin.position, direction, bulletSpeed,
                isPlayerBullet: true, isAutoAim: true));
    }
}
