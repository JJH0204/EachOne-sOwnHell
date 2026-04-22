using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 600f;

    [Header("Shooting")]
    public float fireRate = 0.15f;
    public float bulletSpeed = 20f;
    [FormerlySerializedAs("firePOoint")] public GameObject firePoint;
    public bool isLobby = false;
    public bool tryShoot = false;

    private InputAction _moveAction;
    private InputAction _rollAction;

    private Vector2 _moveInput;
    private bool _isRolling;
    private Vector3 _previousPosition;
    private PlayerDeath _death;

    private Rigidbody _rb;
    private Camera _mainCam;
    private PlayerStats _stats;
    private float _nextFireTime;
    private bool _isGameOver;

    #region Unity Methods

    private void Awake()
    {
        var asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        var playerMap = asset.FindActionMap("Player", throwIfNotFound: true);
        _moveAction = playerMap.FindAction("Move",   throwIfNotFound: true);
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
        _moveAction.canceled  += OnMove;
        _rollAction.performed += OnRoll;
        _moveAction.Enable();
        _rollAction.Enable();

        EventBus<GameOverEvent>.Subscribe(OnGameOver);
    }

    private void OnDisable()
    {
        _moveAction.performed -= OnMove;
        _moveAction.canceled  -= OnMove;
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
        Vector3 camForward = _mainCam.transform.forward;
        Vector3 camRight = _mainCam.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

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

        if (dir.sqrMagnitude < 0.01f) return;

        Roll(dir.normalized);
    }

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
}