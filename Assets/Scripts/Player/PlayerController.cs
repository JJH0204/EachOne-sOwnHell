using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 각자의 지옥 - 플레이어 컨트롤러 (Grey Box)
///
/// 조작:
///   이동  : WASD (카메라 기준 상대 방향)
///   사격  : 마우스 왼쪽 버튼 (마우스 위치 조준)
///   회피  : Space (대시, TODO)
///
/// 각성 상태에서는 이동속도·발사속도 1.5배 상승.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed     = 6f;
    public float rotationSpeed = 600f;

    [Header("Shooting")]
    public float fireRate    = 0.15f;   // 초당 발사 간격
    public float bulletSpeed = 20f;

    // ─── 내부 참조 ─────────────────────────────────────────────
    private Rigidbody   rb;
    private Camera      mainCam;
    private PlayerStats stats;
    private float       nextFireTime;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        rb      = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        stats   = GetComponent<PlayerStats>();

        rb.useGravity  = false;
        rb.interpolation    = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (stats.IsIncapacitated) return;

        HandleShooting();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (stats.IsIncapacitated) return;

        HandleMovement();
    }

    // ─── 이동 ──────────────────────────────────────────────────
    void HandleMovement()
    {
        float h = 0f, v = 0f;
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.aKey.isPressed) h -= 1f;
        if (kb.dKey.isPressed) h += 1f;
        if (kb.sKey.isPressed) v -= 1f;
        if (kb.wKey.isPressed) v += 1f;

        // 카메라 기준 방향으로 변환
        Vector3 camFwd   = mainCam.transform.forward; camFwd.y = 0f; camFwd.Normalize();
        Vector3 camRight = mainCam.transform.right;   camRight.y = 0f; camRight.Normalize();
        Vector3 moveDir  = camFwd * v + camRight * h;

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        float spd = stats.IsAwakened ? moveSpeed * 1.5f : moveSpeed;
        rb.MovePosition(rb.position + moveDir * spd * Time.fixedDeltaTime);

        // 이동 방향으로 회전
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            rb.MoveRotation(Quaternion.RotateTowards(
                rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // ─── 사격 ──────────────────────────────────────────────────
    void HandleShooting()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (!mouse.leftButton.isPressed) return;

        float rate = stats.IsAwakened ? fireRate * 0.5f : fireRate;
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + rate;

        // 마우스 위치 → 지면 교차점
        Ray ray = mainCam.ScreenPointToRay(mouse.position.ReadValue());
        var plane = new Plane(Vector3.up, transform.position);
        if (!plane.Raycast(ray, out float dist)) return;

        Vector3 target = ray.GetPoint(dist);
        Vector3 dir    = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        BulletHelper.Spawn(
            transform.position + dir.normalized * 0.7f + Vector3.up * 0.5f,
            dir.normalized,
            bulletSpeed,
            isPlayerBullet: true
        );
    }
}
