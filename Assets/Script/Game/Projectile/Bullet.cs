using UnityEngine;

/// <summary>
/// 각자의 지옥 - 탄환 컴포넌트
/// BulletHelper.Spawn() 에 의해 Sphere 프리미티브에 부착됩니다.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class Bullet : MonoBehaviour
{
  [HideInInspector] public float damage = 10f;
  [HideInInspector] public float lifetime = 5f;
  [HideInInspector] public bool isPlayerBullet;

  private Vector3 _direction;
  private float _speed;
  private float _spawnTime;
  private bool _isDead;

  public void Initialize(Vector3 dir, float spd, bool playerBullet)
  {
    _direction = dir.normalized;
    _speed = spd;
    isPlayerBullet = playerBullet;
    _spawnTime = Time.time;
    _isDead = false;
  }

  private void Update()
  {
    if (_isDead)
    {
      return;
    }

    if (Time.time - _spawnTime > lifetime)
    {
      Kill();
      return;
    }

    Vector3 move = _direction * (_speed * Time.deltaTime);
    Vector3 nextPosition = transform.position + move;

    if (Physics.Linecast(transform.position, nextPosition, out RaycastHit hit))
    {
      if (HandleHit(hit.collider))
      {
        return;
      }
    }

    transform.position = nextPosition;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (_isDead)
    {
      return;
    }

    HandleHit(other);
  }

  private bool HandleHit(Collider other)
  {
    if (other.GetComponent<Bullet>() != null)
    {
      return false;
    }

    if (other.CompareTag("Wall"))
    {
      Kill();
      return true;
    }

    if (isPlayerBullet)
    {
      GameObject enemyRoot = FindTaggedRoot(other.transform, "Enemy");

      if (enemyRoot == null)
      {
        return false;
      }

      EventBus<BulletHitEnemyEvent>.Raise(
        new BulletHitEnemyEvent(enemyRoot, damage));

      Kill();
      return true;
    }

    GameObject playerRoot = FindTaggedRoot(other.transform, "Player");

    if (playerRoot == null)
    {
      return false;
    }

    EventBus<BulletHitPlayerEvent>.Raise(
      new BulletHitPlayerEvent(damage));

    Kill();
    return true;
  }

  private GameObject FindTaggedRoot(Transform target, string tag)
  {
    Transform current = target;

    while (current != null)
    {
      if (current.CompareTag(tag))
      {
        return current.gameObject;
      }

      current = current.parent;
    }

    return null;
  }

  private void Kill()
  {
    _isDead = true;
    gameObject.SetActive(false);
  }
}
