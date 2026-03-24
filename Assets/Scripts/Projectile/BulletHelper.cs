using UnityEngine;

/// <summary>
/// 탄환 생성 헬퍼 - 프리팹 없이 Sphere 프리미티브로 즉시 생성합니다.
/// 그레이박스 목업 단계에서 사용하며, 이후 풀링 시스템으로 교체합니다.
/// </summary>
public static class BulletHelper
{
    // 공유 머티리얼 (최초 생성 후 재사용)
    static Material s_playerMat;
    static Material s_enemyMat;

    /// <summary>탄환 생성</summary>
    public static Bullet Spawn(Vector3 position, Vector3 direction, float speed, bool isPlayerBullet)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        go.transform.position   = position;
        go.transform.localScale = isPlayerBullet
            ? new Vector3(0.22f, 0.22f, 0.22f)
            : new Vector3(0.30f, 0.30f, 0.30f);

        // 머티리얼 색상 설정
        var rend = go.GetComponent<Renderer>();
        if (isPlayerBullet)
        {
            if (s_playerMat == null)
            {
                s_playerMat = new Material(rend.sharedMaterial);
                s_playerMat.color = new Color(1.00f, 0.90f, 0.10f); // 황색
            }
            rend.sharedMaterial = s_playerMat;
        }
        else
        {
            if (s_enemyMat == null)
            {
                s_enemyMat = new Material(rend.sharedMaterial);
                s_enemyMat.color = new Color(1.00f, 0.20f, 0.20f); // 붉은색
            }
            rend.sharedMaterial = s_enemyMat;
        }

        // Collider를 트리거로 변경 (물리 간섭 없이 충돌 감지)
        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;

        // Bullet 컴포넌트 부착 및 초기화
        var bullet = go.AddComponent<Bullet>();
        bullet.Initialize(direction.normalized, speed, isPlayerBullet);

        return bullet;
    }
}
