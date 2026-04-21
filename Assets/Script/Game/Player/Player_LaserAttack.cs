using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// 각자의 지옥 - 플레이어 레이저 스킬
///
/// 스킬1 (Pierce Laser) : 관통 레이저 — 범위 내 모든 적에게 즉발 피해
/// 스킬2 (Channeling Laser) : 채널링 레이저 — 가장 가까운 적에게 지속 피해
///
/// 이벤트 버스:
///   구독 GameOverEvent         - 게임 오버 시 스킬 입력 차단
///   발행 BulletHitEnemyEvent   - 레이저 명중 시 적에게 피해 전달
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PlayerStats))]
public class PlayerLaserAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private bool isUsingSkill = false;

    [FormerlySerializedAs("Skill1Action")] [Header("inputAction")]
    public InputAction skill1Action;
    [FormerlySerializedAs("Skill2Action")] public InputAction skill2Action;

    [Header("Skill1 - Pierce Laser")]
    [SerializeField] private float skill1Damage   = 30f;
    [SerializeField] private float skill1Range    = 20f;
    [SerializeField] private float skill1Cooldown = 3f;

    [Header("Skill1 Visual")]
    [SerializeField] private LineRenderer skill1Line;
    [SerializeField] private float skill1LineDuration = 0.15f;

    [Header("Skill2 - Channeling Laser")]
    [SerializeField] private float skill2DamagePerSecond = 200f;
    [SerializeField] private float skill2Range           = 15f;

    [Header("Skill2 Visual")]
    [SerializeField] private LineRenderer skill2Line;
    [SerializeField] private float skill2LineDuration = 0.15f;

    [SerializeField] private Camera mainCamera;

    // ─── 내부 상태 ────────────────────────────────────────────────
    private bool        _isGameOver;
    private bool        _isChannelingLaser;
    private float       _nextSkill1Time;
    private PlayerStats _stats;

    public bool IsUsingSkill => isUsingSkill;
    public float Skill1RemainCooldown => Mathf.Max(0f, _nextSkill1Time - Time.time);

    private void RaiseSkillStateChanged() =>
        EventBus<PlayerSkillStateChangedEvent>.Raise(
            new PlayerSkillStateChangedEvent(_nextSkill1Time, skill1Cooldown, isUsingSkill));

    // ─────────────────────────────────────────────────────────────
    private void Start()
    {
        _stats = GetComponent<PlayerStats>();
        if (mainCamera == null) mainCamera = Camera.main;
        RaiseSkillStateChanged();
    }

    private void OnEnable()
    {
        skill1Action.performed += OnSkill1;
        skill2Action.performed += OnSkill2Started;
        skill2Action.canceled  += OnSkill2Cancelled;
        skill1Action.Enable();
        skill2Action.Enable();
    
        EventBus<GameOverEvent>.Subscribe(OnGameOver);
    }
    
    private void OnDisable()
    {
        skill1Action.performed -= OnSkill1;
        skill2Action.performed -= OnSkill2Started;
        skill2Action.canceled  -= OnSkill2Cancelled;
        skill1Action.Disable();
        skill2Action.Disable();
    
        EventBus<GameOverEvent>.Unsubscribe(OnGameOver);
    }

    private void OnGameOver(GameOverEvent _) => _isGameOver = true;

    // ─── 업데이트 ─────────────────────────────────────────────────
    private void Update()
    {
        if (_isGameOver) return;
        if (_stats.IsIncapacitated) return;

        // AimToMouse();

        if (_isChannelingLaser)
            FireChannelingLaser();
    }

    // ─── 입력 핸들러 ──────────────────────────────────────────────
    private void OnSkill1(InputAction.CallbackContext ctx) => StartCoroutine(UseSkill1());

    private void OnSkill2Started(InputAction.CallbackContext ctx)
    {
        if (isUsingSkill) return;
        _isChannelingLaser = true;
        isUsingSkill       = true;
        RaiseSkillStateChanged();
    }

    private void OnSkill2Cancelled(InputAction.CallbackContext ctx)
    {
        _isChannelingLaser = false;
        isUsingSkill       = false;
        HideSkill2Laser();
        RaiseSkillStateChanged();
    }

    // ─── 스킬1 : 관통 레이저 ─────────────────────────────────────
    private void TryFirePierceLaser()
    {
        if (Time.time < _nextSkill1Time) return;
        _nextSkill1Time = Time.time + skill1Cooldown;

        var origin    = firePoint.position;
        var direction = firePoint.forward;
        var endPoint  = origin + direction * skill1Range;

        RaycastHit[] hits = Physics.RaycastAll(origin, direction, skill1Range);
        foreach (RaycastHit hit in hits)
        {
            var root = hit.collider.transform.root;
            if (!root.CompareTag("Enemy")) continue;
            EventBus<BulletHitEnemyEvent>.Raise(new BulletHitEnemyEvent(root.gameObject, skill1Damage));
        }

        ShowSkill1Laser(origin, endPoint);
    }

    IEnumerator UseSkill1()
    {
        isUsingSkill = true;
        TryFirePierceLaser();
        RaiseSkillStateChanged();
        yield return new WaitForSeconds(skill1Cooldown);
        isUsingSkill = false;
        RaiseSkillStateChanged();
    }

    void ShowSkill1Laser(Vector3 start, Vector3 end)
    {
        if (skill1Line == null) return;
        skill1Line.SetPosition(0, start);
        skill1Line.SetPosition(1, end);
        skill1Line.enabled = true;
        CancelInvoke(nameof(HideSkill1Laser));
        Invoke(nameof(HideSkill1Laser), skill1LineDuration);
    }

    void HideSkill1Laser()
    {
        if (skill1Line != null) skill1Line.enabled = false;
    }

    // ─── 스킬2 : 채널링 레이저 ───────────────────────────────────
    void FireChannelingLaser()
    {
        Vector3 origin    = firePoint.position;
        Vector3 direction = firePoint.forward;
        Vector3 endPoint  = origin + direction * skill2Range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, skill2Range))
        {
            var root = hit.collider.transform.root;
            if (root.CompareTag("Enemy"))
            {
                float damage = skill2DamagePerSecond * Time.deltaTime;
                EventBus<BulletHitEnemyEvent>.Raise(new BulletHitEnemyEvent(root.gameObject, damage));
            }
        }

        ShowSkill2Laser(origin, endPoint);
    }

    void ShowSkill2Laser(Vector3 start, Vector3 end)
    {
        if (skill2Line == null) return;
        skill2Line.SetPosition(0, start);
        skill2Line.SetPosition(1, end);
        skill2Line.enabled = true;
        CancelInvoke(nameof(HideSkill2Laser));
        Invoke(nameof(HideSkill2Laser), skill2LineDuration);
    }

    void HideSkill2Laser()
    {
        if (skill2Line != null) skill2Line.enabled = false;
    }

    // ─── 마우스 조준 ──────────────────────────────────────────────
    // void AimToMouse()
    // {
    //     Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
    //     var groundPlane = new Plane(Vector3.up, Vector3.zero);
    //
    //     if (groundPlane.Raycast(ray, out float distance))
    //     {
    //         Vector3 dir = ray.GetPoint(distance) - transform.position;
    //         dir.y = 0f;
    //         if (dir.sqrMagnitude > 0.001f)
    //             transform.forward = dir.normalized;
    //     }
    // }
}