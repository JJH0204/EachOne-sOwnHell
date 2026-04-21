using System.Collections;
using System.Net;
using Unity.VisualScripting;
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
public class Player_LaserAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] bool isUsingSkill = false;

    [Header("Shooting")]
    public float fireRate = 0.15f;   // 초당 발사 간격
    public float bulletSpeed = 20f;

    [Header("inputAction")]
    public InputAction Skill1Action;
    public InputAction Skill2Action;

    [Header("Skill1 - Pierce Laser")]
    [SerializeField] private float skill1Damage = 30f;
    [SerializeField] private float skill1Range = 20f;
    [SerializeField] private float skill1Cooldown = 3f;

    [Header("Skill1 Visual")]
    [SerializeField] private LineRenderer skill1Line;
    [SerializeField] private float skill1LineDuration = 0.15f;

    [Header("Skill2 - Channeling Laser")]
    [SerializeField] private float skill2DamagePerSecond = 200f;
    [SerializeField] private float skill2Range = 15f;

    [Header("Skill2 Visual")]
    [SerializeField] private LineRenderer skill2Line;
    [SerializeField] private float skill2LineDuration = 0.15f;





    [SerializeField] private Camera mainCamera;

    private Vector2 MoveInput;
    private bool isChannelingLaser = false;
    private float nextSkill1Time = 0f;
    /*   private float nextSkill2Time = 0f;*/
    private EnemyController EnemyHP;
    private float EnemyHealth;

    // ─── 내부 참조 ─────────────────────────────────────────────
    private Rigidbody rb;
    private Camera mainCam;
    private PlayerStats stats;
    private float nextFireTime;

    // ───────────────────────────────────────────────────────────
    public bool IsUsingSkill
    {
        get { return isUsingSkill; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        stats = GetComponent<PlayerStats>();
        EnemyHP = GetComponent<EnemyController>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;

    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (stats.IsIncapacitated) return;

        /*        HandleShooting();*/

        AimToMouse();

        if (isChannelingLaser)
        {
            FireChannelingLaser();
        }

    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (stats.IsIncapacitated) return;

    }

    public float Skill1RemainCooldown
    {
        get { return Mathf.Max(0f, nextSkill1Time - Time.time); }
    }



    // ─── 스킬 키 입력 ──────────────────────────────────────────────────

    void OnEnable()
    {
        Skill1Action.performed += OnSkill1;
        Skill2Action.performed += OnSkill2Started;
        Skill2Action.canceled += OnSkill2Canceld;

        Skill1Action.Enable();
        Skill2Action.Enable();
    }

    void OnDisable()
    {
        Skill1Action.performed -= OnSkill1;
        Skill2Action.performed -= OnSkill2Started;
        Skill2Action.canceled -= OnSkill2Canceld;

        Skill1Action.Disable();
        Skill2Action.Disable();
    }

    void OnSkill1(InputAction.CallbackContext context)
    {
        StartCoroutine(UseSkill1());
    }

    void OnSkill2Started(InputAction.CallbackContext context)
    {
        if (isUsingSkill) return;

        isChannelingLaser = true;
        isUsingSkill = true;
    }

    void OnSkill2Canceld(InputAction.CallbackContext context)
    {
        isChannelingLaser = false;
        isUsingSkill = false;
        HideSkill2Laser();
    }


    // ─── 스킬 ──────────────────────────────────────────────────

    void TryFirePierceLaser()
    {
        if (Time.time < nextSkill1Time)
            return;

        nextSkill1Time = Time.time + skill1Cooldown;

        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;
        Vector3 endPoint = origin + direction * skill1Range;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, skill1Range);

        Debug.DrawRay(origin, direction * skill1Range, Color.red, 5f);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(skill1Damage);
                }
            }
        }

        ShowSkill1Laser(origin, endPoint);

        Debug.Log("스킬1 관통 레이저 발사");
    }

    void ShowSkill1Laser(Vector3 start, Vector3 end)
    {
        if (skill1Line == null)
            return;

        skill1Line.SetPosition(0, start);
        skill1Line.SetPosition(1, end);
        skill1Line.enabled = true;

        CancelInvoke(nameof(HideSkill1Laser));
        Invoke(nameof(HideSkill1Laser), skill1LineDuration);
    }

    void HideSkill1Laser()
    {
        if (skill1Line == null)
            return;

        skill1Line.enabled = false;
    }


    void AimToMouse()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(distance);
            Vector3 direction = mouseWorldPos - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.forward = direction.normalized;
            }
        }
    }


    void FireChannelingLaser()
    {
        Vector3 origin = firePoint.position;
        Vector3 direction = firePoint.forward;
        Vector3 endPoint = origin + direction * skill2Range;


        Debug.DrawRay(origin, direction * skill2Range, Color.blue);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, skill2Range))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyController enemy = hit.collider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    float damage = skill2DamagePerSecond * Time.deltaTime;
                    enemy.TakeDamage(damage);
                }
            }
        }

        ShowSkill2Laser(origin, endPoint);
    }


    void HideSkill2Laser()
    {
        if (skill2Line == null)
            return;

        skill2Line.enabled = false;
    }


    void ShowSkill2Laser(Vector3 start, Vector3 end)
    {
        if (skill2Line == null)
            return;

        skill2Line.SetPosition(0, start);
        skill2Line.SetPosition(1, end);
        skill2Line.enabled = true;

        CancelInvoke(nameof(HideSkill2Laser));
        Invoke(nameof(HideSkill2Laser), skill2LineDuration);
    }

    IEnumerator UseSkill1()
    {
        isUsingSkill = true;

        TryFirePierceLaser();

        yield return new WaitForSeconds(3.0f);

        isUsingSkill = false;
    }

}




// ─── 사격 ──────────────────────────────────────────────────

/*void HandleShooting()
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
*/