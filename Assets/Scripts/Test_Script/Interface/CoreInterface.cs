using UnityEngine;

public interface IMovable
{
    public void Move(Vector2 direction);
    public void Jump();
}

public interface IProjectileLauncher
{
    public void Shoot();
}
