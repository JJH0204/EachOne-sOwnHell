using System.Collections.Generic;
using UnityEngine;


public static class BulletHelper

{
    static Material s_playerMat;
    static Material s_enemyMat;
    static readonly List<Bullet> s_pool = new List<Bullet>();
    static int s_initialSize = 30;

    static BulletHelper()
    {
        for (int i = 0; i < s_initialSize; i++)
        {
            CreateBullet(true).gameObject.SetActive(false);
            CreateBullet(false).gameObject.SetActive(false);
        }
    }



    public static Bullet Spawn(Vector3 position, Vector3 direction, float speed, bool isPlayerBullet)

    {
        Bullet bullet = GetBullet(isPlayerBullet);

        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(direction.normalized);
        bullet.gameObject.SetActive(true);
        bullet.Initialize(direction.normalized, speed, isPlayerBullet);

        return bullet;
    }



    static Bullet GetBullet(bool isPlayerBullet)

    {
        for (int i = 0; i < s_pool.Count; i++)
        {
            Bullet bullet = s_pool[i];

            if (!bullet.gameObject.activeInHierarchy && bullet.isPlayerBullet == isPlayerBullet)

                return bullet;
        }

        Bullet newBullet = CreateBullet(isPlayerBullet);
        newBullet.gameObject.SetActive(false);

        return newBullet;
    }

    static Bullet CreateBullet(bool isPlayerBullet)
    {
        //하이리아키 창에서 총알 저장 되는걸 Bullet Pool로 정리하기
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        GameObject poolFolder = GameObject.Find("--- Bullet Pool ---");
        if (poolFolder == null)
        {
            poolFolder = new GameObject("--- Bullet Pool ---");
        }
        go.transform.SetParent(poolFolder.transform);

        go.name = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";

        go.transform.localScale = isPlayerBullet

            ? new Vector3(0.22f, 0.22f, 0.22f)

            : new Vector3(0.30f, 0.30f, 0.30f);

        var rend = go.GetComponent<Renderer>();

        if (isPlayerBullet)
        {
            if (s_playerMat == null)
            {
                s_playerMat = new Material(rend.sharedMaterial);

                s_playerMat.color = new Color(1.00f, 0.90f, 0.10f);
            }
            rend.sharedMaterial = s_playerMat;
        }

        else
        {
            if (s_enemyMat == null)
            {
                s_enemyMat = new Material(rend.sharedMaterial);
                s_enemyMat.color = new Color(1.00f, 0.20f, 0.20f);

            }
            rend.sharedMaterial = s_enemyMat;
        }



        var col = go.GetComponent<SphereCollider>();
        col.isTrigger = true;

        var bullet = go.AddComponent<Bullet>();
        bullet.isPlayerBullet = isPlayerBullet;

        s_pool.Add(bullet);

        return bullet;

    }

}