using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// TODO: InputSystem 확인

[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 600f;

    [Header("Shooting")]
    public float fireRate = 0.15f;   // 초당 발사 간격
    public float bulletSpeed = 20f;
    [FormerlySerializedAs("firePOoint")] public GameObject firePoint;
    public bool isLobby = false;
    public bool tryShoot = false;

    [Header("Input Action")] 
    private InputAction _moveAction;
    private InputAction _rollAction;


    private Vector2 _moveInput;
    private bool _isRolling;
    private Vector3 _previousPosition;
    private PlayerDeath _death;
    // ─── 내부 참조 ─────────────────────────────────────────────
    private Rigidbody _rb;
    private Camera _mainCam;
    private PlayerStats _stats;
    private float _nextFireTime;
    private bool _isGameOver;

    // ───────────────────────────────────────────────────────────

    #region Unity Methods

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCam = Camera.main;
        _stats = GetComponent<PlayerStats>();

        _rb.useGravity = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;

        EventBus<PlayerMovementChangedEvent>.Raise(new PlayerMovementChangedEvent(moveSpeed));
    }
    
    private void Update()
    {
        if (_isGameOver) return;
        if (_stats.IsIncapacitated) return;

        //로비에서 총알 발사 되는거 막기용 추후 로비 매니저로 옮길 예정
        // if (Mouse.current.leftButton.wasPressedThisFrame && isLobby)
        // {
        //     Debug.Log("왼쪽 클릭되었으나 로비임으로 수동발사되지 않음");
        // }

        /*HandleShooting();*/
    }

    private void FixedUpdate()
    {
        if (_isGameOver) return;
        if (_stats.IsIncapacitated) return;

        HandleMovement();
    }
    
    private void OnEnable()
    {
        _moveAction.performed += OnMove;
        _rollAction.performed += OnRoll;
        _moveAction.canceled  += OnMove;
        _moveAction.Enable();
        _rollAction.Enable();
    
        EventBus<GameOverEvent>.Subscribe(OnGameOver);
    }
    
    private void OnDisable()
    {
        _moveAction.performed -= OnMove;
        _rollAction.performed -= OnRoll;
        _moveAction.canceled  -= OnMove;
        _moveAction.Disable();
        _rollAction.Disable();
    
        EventBus<GameOverEvent>.Unsubscribe(OnGameOver);
    }

    private void OnGameOver(GameOverEvent _) => _isGameOver = true;

    #endregion

    // ─── 이동 ──────────────────────────────────────────────────

    private void OnMove(InputAction.CallbackContext context)
    {
        if (_death != null && _death.isDead)
        {
            _moveInput = Vector2.zero;
            return;
        }
    
        _moveInput = context.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        //메인 카메라 앞,오른쪽 방향 가져오기
        Vector3 camForward = _mainCam.transform.forward;
        Vector3 camRight = _mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        //카메라 방향이랑 키보드 입력 섞은뒤 월드 좌표 기준으로 움직이게 하기
        Vector3 moveDirection = (camForward * _moveInput.y) + (camRight * _moveInput.x);
        transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
    }


    private void OnRoll(InputAction.CallbackContext context)
    {
        if (_isRolling) return;
        Vector3 camForward = _mainCam.transform.forward;
        Vector3 camRight = _mainCam.transform.right;
    
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
    
    
        Vector3 dir = (camForward * _moveInput.y) + (camRight * _moveInput.x);
    
        if (dir.sqrMagnitude < 0.01f)
        {
            return;
        }
    
        Roll(dir.normalized);
    
    }

    private void Roll(Vector3 dir)
    {
        _previousPosition = transform.position;
        _isRolling = true;

        Debug.Log("진짜로 구름");
        transform.position += dir * 3f;
        StartCoroutine(Wait());
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            _isRolling = false; // 구르기 취소
        }
    }


    private IEnumerator Wait()
    {
        Debug.Log("코루틴 시작");
        yield return new WaitForSeconds(1.0f);
        _isRolling = false;
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
