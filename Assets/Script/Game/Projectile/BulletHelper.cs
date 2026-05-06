using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 각자의 지옥 - 총알 관리
///
/// 역할:
///   - 총알을 오브젝트 풀로 관리하며 메모리에 부담이 되지 않게 합니다
/// </summary>


public static class BulletHelper
{
    private static Material _sPlayerMat;
    private static Material _sEnemyMat;
    private static readonly List<Bullet> SPool = new List<Bullet>();
    private const int SInitialSize = 30;

    static BulletHelper()
    {
        for (var i = 0; i < SInitialSize; i++)
        {
            CreateBullet(true).gameObject.SetActive(false);
            CreateBullet(false).gameObject.SetActive(false);
        }
    }

    public static void ClearPool()
    {
        SPool.Clear();
        _sPlayerMat = null;
        _sEnemyMat = null;
    }

    public static Bullet Spawn(Vector3 position, Vector3 direction, float speed, bool isPlayerBullet, bool isAutoAimBullet)
    {
        Bullet bullet = GetBullet(isPlayerBullet);

        if (!bullet)
        {
            Debug.LogWarning("no Bullet");
            return null;
        }

        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);
        bullet.gameObject.SetActive(true);
        bullet.Initialize(direction.normalized, speed, isPlayerBullet);

        Renderer rend = bullet.GetComponent<Renderer>();

        if (isPlayerBullet)
        {
            rend.material.color = isAutoAimBullet ? new Color(0.20f, 0.80f, 1.00f) : new Color(1.00f, 0.90f, 0.10f);
        }
        else
        {
            rend.material.color = new Color(1.00f, 0.20f, 0.20f);
        }

        return bullet;
    }

    private static Bullet GetBullet(bool isPlayerBullet)
    {
        for (var i = SPool.Count - 1; i >= 0; i--)
        {
            var bullet = SPool[i];

            if (!bullet)
            {
                SPool.RemoveAt(i);
                continue;
            }

            if (!bullet.gameObject.activeInHierarchy && bullet.isPlayerBullet == isPlayerBullet)
                return bullet;
        }

        Bullet newBullet = CreateBullet(isPlayerBullet);
        newBullet.gameObject.SetActive(false);

        return newBullet;
    }

    private static Bullet CreateBullet(bool isPlayerBullet)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        GameObject poolFolder = GameObject.Find("--- Bullet Pool ---");
        if (!poolFolder)
        {
            poolFolder = new GameObject("--- Bullet Pool ---");
        }

        go.transform.SetParent(poolFolder.transform);

        go.transform.localScale = isPlayerBullet
            ? new Vector3(0.22f, 0.22f, 0.22f)
            : new Vector3(0.30f, 0.30f, 0.30f);

        var rend = go.GetComponent<Renderer>();

        if (isPlayerBullet)
        {
            if (!_sPlayerMat)
            {
                _sPlayerMat = new Material(rend.sharedMaterial)
                {
                    color = new Color(1.00f, 0.90f, 0.10f)
                };
            }

            rend.sharedMaterial = _sPlayerMat;
        }
        else
        {
            if (!_sEnemyMat)
            {
                _sEnemyMat = new Material(rend.sharedMaterial)
                {
                    color = new Color(1.00f, 0.20f, 0.20f)
                };
            }

            rend.sharedMaterial = _sEnemyMat;
        }

        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;

        var bullet = go.AddComponent<Bullet>();
        bullet.isPlayerBullet = isPlayerBullet;

        SPool.Add(bullet);

        return bullet;
    }
}
