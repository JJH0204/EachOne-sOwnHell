using System.Collections;
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
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 600f;

    [Header("Shooting")]
    public float fireRate = 0.15f;   // 초당 발사 간격
    public float bulletSpeed = 20f;
    public GameObject firePOoint;
    public bool isLobby = false;
    public bool tryShoot = false;

    [Header("Input Action")]
    public InputAction MoveAction;
    public InputAction RollAction;


    private Vector2 MoveInput;
    private bool isrolling;
    private Vector3 previousPosition;
    private PlayerDeath death;
    // ─── 내부 참조 ─────────────────────────────────────────────
    private Rigidbody rb;
    private Camera mainCam;
    private PlayerStats stats;
    private float nextFireTime;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
        stats = GetComponent<PlayerStats>();

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

        //로비에서 총알 발사 되는거 막기용 추후 로비 매니저로 옮길 예정
        if (Mouse.current.leftButton.wasPressedThisFrame && isLobby == true)
        {
            Debug.Log("왼쪽 클릭되었으나 로비임으로 수동발사되지 않음");
        }

        /*HandleShooting();*/
    }

    void FixedUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (stats.IsIncapacitated) return;

        HandleMovement();
    }

    // ─── 이동 ──────────────────────────────────────────────────

    void OnEnable()
    {
        MoveAction.performed += OnMove;
        RollAction.performed += OnRoll;


        MoveAction.canceled += OnMove;

        MoveAction.Enable();
        RollAction.Enable();
    }

    void OnDisable()
    {
        MoveAction.performed -= OnMove;
        RollAction.performed -= OnRoll;

        MoveAction.canceled -= OnMove;

        MoveAction.Disable();
        RollAction.Disable();

    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (death != null && death.isDead)
        {
            MoveInput = Vector2.zero;
            return;
        }

        MoveInput = context.ReadValue<Vector2>();
    }

    void HandleMovement()
    {
        //메인 카메라 앞,오른쪽 방향 가져오기
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        //카메라 방향이랑 키보드 입력 섞은뒤 월드 좌표 기준으로 움직이게 하기
        Vector3 moveDirection = (camForward * MoveInput.y) + (camRight * MoveInput.x);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }


    void OnRoll(InputAction.CallbackContext context)
    {
        if (isrolling) return;
        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();


        Vector3 dir = (camForward * MoveInput.y) + (camRight * MoveInput.x);

        if (dir.sqrMagnitude < 0.01f)
        {
            return;
        }

        Roll(dir.normalized);

    }

    void Roll(Vector3 dir)
    {
        previousPosition = transform.position;
        isrolling = true;

        Debug.Log("진짜로 구름");
        transform.position += dir * 3f;
        StartCoroutine(Wait());
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isrolling = false; // 구르기 취소
        }
    }


    IEnumerator Wait()
    {
        Debug.Log("코루틴 시작");
        yield return new WaitForSeconds(1.0f);
        isrolling = false;
        Debug.Log("코루틴 종료");


    }


    // ─── 사격 ──────────────────────────────────────────────────

/*    void HandleShooting()
    {
        //로비에서 총알 발사되는거 막기용
        if (isLobby) { return; }

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
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        BulletHelper.Spawn(
            transform.position + dir.normalized * 0.7f + Vector3.up * 0.5f,
            dir.normalized,
            bulletSpeed,
            isPlayerBullet: true,
            isAutoAimBullet: false
        );
    } */
}
