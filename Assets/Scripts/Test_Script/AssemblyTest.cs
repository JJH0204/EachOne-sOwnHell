using UnityEngine;

// 최상위 클래스에서 다른 어셈블리의 클래스를 참조
public class AssemblyTest : MonoBehaviour
{
    void Start()
    {
        TestPlayer player = new GameObject("TestPlayer").AddComponent<TestPlayer>();
        if (player) player.Jump();
        else Debug.LogWarning("Failed to create TestPlayer instance.");

        TestBullet bullet = new GameObject("Bullet").AddComponent<TestBullet>();
        if (player) player.Shoot();
    }
}
