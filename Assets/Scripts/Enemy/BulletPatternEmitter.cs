using System.Collections;
using UnityEngine;

/// <summary>
/// 각자의 지옥 - 탄막 패턴 방출기
///
/// 패턴 종류 (기획서 핵심 재미요소 1: "캐릭터의 아픔이 몬스터의 공격으로 형상화"):
///   Circular : 방사형 탄막 (전방위 균등 발사)
///   Spiral   : 나선형 탄막 (회전하며 발사)
///   Aimed    : 조준형  탄막 (플레이어 방향 3-way)
///
/// 보스 패턴 확장을 위해 PatternType을 추가하기만 하면 됩니다.
/// </summary>
public class BulletPatternEmitter : MonoBehaviour
{
    public enum PatternType { Circular, Spiral, Aimed }

    [Header("Pattern")]
    public PatternType pattern       = PatternType.Circular;
    public int         bulletCount   = 8;
    public float       bulletSpeed   = 5.5f;
    public float       fireInterval  = 1.4f;

    private bool      isActive;
    private Coroutine patternRoutine;
    private Transform player;
    private float     spiralAngle;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) player = go.transform;
    }

    public void StartPattern()
    {
        if (isActive) return;
        isActive       = true;
        patternRoutine = StartCoroutine(PatternLoop());
    }

    public void StopPattern()
    {
        if (!isActive) return;
        isActive = false;
        if (patternRoutine != null) StopCoroutine(patternRoutine);
    }

    // ─── 패턴 루프 ─────────────────────────────────────────────
    IEnumerator PatternLoop()
    {
        // 첫 발사 전 짧은 대기 (공격 진입 직후 즉발 방지)
        yield return new WaitForSeconds(0.4f);

        while (isActive)
        {
            switch (pattern)
            {
                case PatternType.Circular: FireCircular(); break;
                case PatternType.Spiral:   FireSpiral();   break;
                case PatternType.Aimed:    FireAimed();    break;
            }
            yield return new WaitForSeconds(fireInterval);
        }
    }

    // ─── 원형 탄막 ─────────────────────────────────────────────
    void FireCircular()
    {
        float step = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0, step * i, 0) * Vector3.forward;
            SpawnBullet(dir);
        }
    }

    // ─── 나선형 탄막 ───────────────────────────────────────────
    void FireSpiral()
    {
        int burst = 4;
        for (int i = 0; i < burst; i++)
        {
            float angle = spiralAngle + (360f / burst) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            SpawnBullet(dir);
        }
        spiralAngle = (spiralAngle + 25f) % 360f;
    }

    // ─── 조준형 탄막 (3-way) ───────────────────────────────────
    void FireAimed()
    {
        if (player == null) return;

        Vector3 baseDir = player.position - transform.position;
        baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.01f) return;
        baseDir.Normalize();

        int   ways   = 3;
        float spread = 18f;
        for (int i = 0; i < ways; i++)
        {
            float offset = (i - ways / 2) * spread;
            Vector3 dir  = Quaternion.Euler(0, offset, 0) * baseDir;
            SpawnBullet(dir);
        }
    }

    // ─── 탄환 생성 ─────────────────────────────────────────────
    void SpawnBullet(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        BulletHelper.Spawn(origin, direction, bulletSpeed, isPlayerBullet: false);
    }
}
