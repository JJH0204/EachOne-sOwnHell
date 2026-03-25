using UnityEngine;

public class TestPlayer : MonoBehaviour, IMovable, IProjectileLauncher
{
    public void Jump()
    {
        Debug.Log("TestPlayer Jumped!");
    }

    public void Move(Vector2 direction)
    {
        Debug.Log("TestPlayer Moved in direction: " + direction);
    }

    public void Shoot()
    {
        // 발사 시 Player가 행할 로직 (애니메이션, 반동에 따른 캐릭터 움직임 등)
        // Bullet과는 독립적
        Debug.Log("Shoot!");
    }
}
