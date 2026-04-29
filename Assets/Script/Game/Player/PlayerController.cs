using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// 각자의 지옥 - 플레이어 이동 관리
///
/// 역할:
///   - 플레이어 이동을 처리합니다
///   - 카메라 기준 이동 처리합니다
///   - 구르기 입력 과 구르기 쿨타임 처리합니다
///   - 게임오버 또는 전투불능시 움직이는걸 막습니다
/// 우선도:
///   - 추후 작업 우선 순위가 높은건 *** 이며 * 가 낮아질수록 우선도가 낮습니다
/// </summary>

[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
  [Header("Movement")]
  public float moveSpeed = 6f;
  public float rotationSpeed = 600f;

  /*임시적 주석 처리 후 리팩토링 끝나는 날 사용처가 없다고 판단시 지울 예정
    [Header("Shooting")]
    public float fireRate = 0.15f;
    public float bulletSpeed = 20f;
    [FormerlySerializedAs("firePOoint")] public GameObject firePoint;
    public bool isLobby = false;
    public bool tryShoot = false;
    private float _nextFireTime;*/

  #region Field

  private InputAction _moveAction;
  private InputAction _rollAction;
  private bool _isRolling;
  private Vector3 _previousPosition;

  private Vector2 _moveInput;
  private Rigidbody _rb;
  private Camera _mainCam;
  private PlayerStats _stats;

  #endregion

  //TODO : 추후 플레이어 스탯 과 플레이어 데스를 연결 시키는걸로 하며 임시적으로 값을 둔다
  private bool _isGameOver;
  private PlayerDeath _death;

  #region Unity Methods

  private void Awake()
  {
    var asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
    var playerMap = asset.FindActionMap("Player", throwIfNotFound: true);
    _moveAction = playerMap.FindAction("Move", throwIfNotFound: true);
    _rollAction = playerMap.FindAction("Sprint", throwIfNotFound: true);
  }

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

  //TODO : 추후 처리하는 로직 없으면 제거 할 예정
  private void Update()
  {
    if (_isGameOver) return;
    if (_stats.IsIncapacitated) return;
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
    _moveAction.canceled += OnMove;
    _rollAction.performed += OnRoll;
    _moveAction.Enable();
    _rollAction.Enable();

    EventBus<GameOverEvent>.Subscribe(OnGameOver);
  }

  private void OnDisable()
  {
    _moveAction.performed -= OnMove;
    _moveAction.canceled -= OnMove;
    _rollAction.performed -= OnRoll;
    _moveAction.Disable();
    _rollAction.Disable();

    EventBus<GameOverEvent>.Unsubscribe(OnGameOver);
  }

  private void OnGameOver(GameOverEvent _) => _isGameOver = true;

  #endregion

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
    Vector3 moveDirection = GetCameraRelativeMoveDirection();
    transform.Translate(moveDirection * (moveSpeed * Time.deltaTime), Space.World);
  }

  private void OnRoll(InputAction.CallbackContext context)
  {
    if (_isRolling) return;

    Vector3 dir = GetCameraRelativeMoveDirection();

    if (dir.sqrMagnitude < 0.01f) return;

    Roll(dir.normalized);
  }

  #region 카메라 ***
  //TODO : 추후 시네머신 카메라를 여러개 쓰이면서 Camera Manager 라던가 한곳에 모야아 할 경우 그때 분리 할 예정
  private Vector3 GetCameraRelativeMoveDirection()
  {
    Vector3 camForward = _mainCam.transform.forward;
    Vector3 camRight = _mainCam.transform.right;

    camForward.y = 0f;
    camRight.y = 0f;

    camForward.Normalize();
    camRight.Normalize();

    return (camForward * _moveInput.y) + (camRight * _moveInput.x);
  }
  #endregion
  #region 벽 관통 수정*
  //TODO : 구르기 쿨 타임용으로 지금은 냅두며 현 리기온은 추후 우선도 낮은 구르기 벽뚫는문제 해결하기 위함으로 둘것
  private void Roll(Vector3 dir)
  {
    _previousPosition = transform.position;
    _isRolling = true;

    transform.position += dir * 3f;
    StartCoroutine(RollCooldown());
  }

  private void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.CompareTag("Wall"))
      _isRolling = false;
  }

  private IEnumerator RollCooldown()
  {
    yield return new WaitForSeconds(1.0f);
    _isRolling = false;
  }
  #endregion

}
