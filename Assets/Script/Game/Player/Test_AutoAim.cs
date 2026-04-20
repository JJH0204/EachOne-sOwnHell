using UnityEngine;

public class test_AutoAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;

    [Header("Auto Fire Settings")]
    [SerializeField] private float fireInterval = 0.3f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float bulletSpeed = 12f;

    [SerializeField] private Player_LaserAttack Skill;


    public PlayerStats Stats;
    private PlayerDeath death;
    private float nextFireTime;


    private void Start()
    {
        Stats = GetComponentInParent<PlayerStats>();
        Skill = GetComponentInParent<Player_LaserAttack>();
        death = GetComponent<PlayerDeath>();
    }

    void Update()
    {

        if (death != null && death.isDead)
            return;


        if (Time.time < nextFireTime)
            return;

        if (Skill != null && Skill.IsUsingSkill)
            return;

        if (Stats.currentHP <= 0)
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
        nextFireTime = Time.time + fireInterval;
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

        BulletHelper.Spawn(
            shootOrigin.position,
            direction,
            bulletSpeed,
            true,
            true
        );
    }
}