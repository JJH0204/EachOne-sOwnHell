using UnityEngine;
using UnityEngine.Serialization;

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