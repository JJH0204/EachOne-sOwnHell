using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 각자의 지옥 - 플레이어 스탯 시스템
/// HP와 스트레스 수치를 관리합니다.
///
/// 스트레스 루프:
///   전투 중 스트레스 축적 → 임계점 도달 → 전투 불능(3초)
///   → 회복 시 '각성' 상태 발동 (공격속도/이동속도 1.5배, 5초)
///
/// 이벤트 버스:
///   구독 EnemyCountChangedEvent    - 전투 상태(inCombat) 판단
///   구독 BulletHitPlayerEvent     - 적 탄환 피격 → TakeDamage 처리
///   구독 EnemyContactDamageEvent  - 적 접촉 피격 → TakeDamage 처리
///   구독 ExpOrbPickedUpEvent      - 경험치 오브 획득 → AddExp 처리
///   구독 ItemOrbPickedUpEvent     - 아이템 오브 획득 → AddItem 처리
///   발행 PlayerStatsChangedEvent   - HP/Stress/상태 변경 시 HUD 캐시 갱신 요청
///   발행 PlayerDamagedEvent        - 피해 발생
///   발행 PlayerDiedEvent           - 사망
///   발행 PlayerIncapacitatedEvent  - 전투 불능 진입
///   발행 PlayerAwakenedEvent       - 각성 진입
///   발행 PlayerAwakenedEndedEvent  - 각성 종료
///   발행 ExpGainedEvent            - 경험치 획득
///   발행 LevelUpEvent             - 경험치 임계치 도달 시 레벨업 UI 요청
///   발행 ItemGainedEvent           - 아이템 획득
///   발행 GameStatusEvent           - HUD 메시지 요청
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlayerStats : MonoBehaviour
{

    private PlayerDeath _death;
    private bool _cancelAwake;

    [FormerlySerializedAs("maxHP")] [Header("HP")]
    public float maxHp      = 100f;
    [FormerlySerializedAs("currentHP")] public float currentHp;

    [FormerlySerializedAs("Damage")] [Header("목업용(GUI) 테스트 버전 보여주기 스텟")]
    public float damage = 10f;

    [Header("Stress (스트레스 수치)")]
    public float maxStress             = 100f;
    public float currentStress;
    public float stressGainRate        = 4f;   // 전투 중 초당 증가량
    public float stressRecoveryRate    = 6f;   // 전투 외 초당 감소량
    public float incapacitatedDuration = 3f;   // 전투 불능 지속 시간(초)
    public float awakenedDuration      = 5f;   // 각성 지속 시간(초)

    [Header("목업용 경험치")]
    public int currentExp;
    public int currentItem;


    // ─── 상태 플래그 ───────────────────────────────────────────
    public bool IsIncapacitated { get; private set; }
    private bool IsAwakened      { get; set; }

    // ─── 색상 상수 ─────────────────────────────────────────────
    static readonly Color NormalColor      = new Color(0.30f, 0.55f, 0.85f); // 파란 회색
    static readonly Color IncapColor       = new Color(0.40f, 0.40f, 0.80f); // 보라색
    static readonly Color AwakenColor      = new Color(1.00f, 0.80f, 0.10f); // 황금색
    static readonly Color DamageFlashColor = new Color(1.00f, 0.20f, 0.20f); // 빨간색

    private Renderer[] _renderers;
    private Coroutine  _stressCoroutine;
    private int        _enemyCount;

  // ─── 헬퍼 ─────────────────────────────────────────────────
  private void RaiseStatsChanged()
  {
    EventBus<PlayerStatsChangedEvent>.Raise(new PlayerStatsChangedEvent(
        currentHp, maxHp, currentStress, maxStress, damage, IsIncapacitated, IsAwakened));
  }

    // ───────────────────────────────────────────────────────────

    private void Awake()
    {
        _death = GetComponent<PlayerDeath>();
    }

    private void OnEnable()
    {
        EventBus<EnemyCountChangedEvent>.Subscribe(OnEnemyCountChanged);
        EventBus<BulletHitPlayerEvent>.Subscribe(OnBulletHit);
        EventBus<EnemyContactDamageEvent>.Subscribe(OnEnemyContact);
        EventBus<ExpOrbPickedUpEvent>.Subscribe(OnExpOrbPickedUp);
        EventBus<ItemOrbPickedUpEvent>.Subscribe(OnItemOrbPickedUp);
    }

    private void OnDisable()
    {
        EventBus<EnemyCountChangedEvent>.Unsubscribe(OnEnemyCountChanged);
        EventBus<BulletHitPlayerEvent>.Unsubscribe(OnBulletHit);
        EventBus<EnemyContactDamageEvent>.Unsubscribe(OnEnemyContact);
        EventBus<ExpOrbPickedUpEvent>.Unsubscribe(OnExpOrbPickedUp);
        EventBus<ItemOrbPickedUpEvent>.Unsubscribe(OnItemOrbPickedUp);
    }

    private void OnEnemyCountChanged(EnemyCountChangedEvent evt)   => _enemyCount = evt.Count;
    private void OnBulletHit(BulletHitPlayerEvent evt)             => TakeDamage(evt.Damage);
    private void OnEnemyContact(EnemyContactDamageEvent evt)       => TakeDamage(evt.Damage);
    private void OnExpOrbPickedUp(ExpOrbPickedUpEvent evt)         => AddExp(evt.Amount);
    private void OnItemOrbPickedUp(ItemOrbPickedUpEvent evt)       => AddItem(evt.Amount);

    private void Start()
    {
        currentHp     = maxHp;
        currentStress = 0f;
        _renderers     = GetComponentsInChildren<Renderer>();
        _cancelAwake = false;
        SetColor(NormalColor);
        RaiseStatsChanged();
    }

    private void Update()
    {
        if (IsIncapacitated || IsAwakened) return;

        // 적이 살아있으면 스트레스 증가, 없으면 감소
        var inCombat = _enemyCount > 0;
        var prevStress = currentStress;
        currentStress = inCombat ? Mathf.Min(currentStress + stressGainRate * Time.deltaTime, maxStress) : Mathf.Max(currentStress - stressRecoveryRate * Time.deltaTime, 0f);

        if (!Mathf.Approximately(currentStress, prevStress))
            RaiseStatsChanged();

        if (currentStress >= maxStress)
            _stressCoroutine = StartCoroutine(IncapRoutine());
    }

    // ─── 피해 처리 ─────────────────────────────────────────────
    private void TakeDamage(float amount)
    {
        if (IsIncapacitated) return;

        currentHp     = Mathf.Max(currentHp - amount, 0f);
        currentStress = Mathf.Min(currentStress + amount * 0.4f, maxStress);

        EventBus<PlayerDamagedEvent>.Raise(new PlayerDamagedEvent(amount));
        RaiseStatsChanged();
        StartCoroutine(DamageFlash());

        if (currentHp <= 0f) Die();
    }

    void Die()
    {
        if (_death != null)
        {
            _death.HandleDeath();
            _cancelAwake = true;
            EventBus<PlayerDiedEvent>.Raise(new PlayerDiedEvent());
        }
    }

    public void ReviveForMockup(int hp)
    {
        currentHp = hp;
        RaiseStatsChanged();
        Debug.Log("부활 HP : " + currentHp);
    }

    // ─── 경험치 ────────────────────────────────

    private void AddExp(int amount)
    {
        currentExp += amount;
        EventBus<ExpGainedEvent>.Raise(new ExpGainedEvent(amount));
        Debug.Log($"경험치 획득! +{amount} / 현재 경험치 : {currentExp}");

        if (currentExp >= 10)
        {
            currentExp -= 10;
            EventBus<LevelUpEvent>.Raise(new LevelUpEvent());
        }
    }

    private void AddItem(int amount2)
    {
        currentItem += amount2;
        EventBus<ItemGainedEvent>.Raise(new ItemGainedEvent(amount2));
        Debug.Log($"아이템 획득! +{amount2} / 현재 흭득 아이템 갯수 : {currentItem}");
    }




    // ─── 전투 불능 → 각성 루틴 ────────────────────────────────
    IEnumerator IncapRoutine()
    {
        if (_cancelAwake) { yield break; }
        IsIncapacitated = true;
        SetColor(IncapColor);
        EventBus<PlayerIncapacitatedEvent>.Raise(new PlayerIncapacitatedEvent());
        EventBus<GameStatusEvent>.Raise(new GameStatusEvent("전투 불능!"));
        RaiseStatsChanged();

        yield return new WaitForSeconds(incapacitatedDuration);

        IsIncapacitated = false;
        currentStress   = 0f;

        // 각성
        IsAwakened = true;
        SetColor(AwakenColor);
        EventBus<PlayerAwakenedEvent>.Raise(new PlayerAwakenedEvent());
        EventBus<GameStatusEvent>.Raise(new GameStatusEvent("각성!"));
        RaiseStatsChanged();

        yield return new WaitForSeconds(awakenedDuration);

        IsAwakened = false;
        SetColor(NormalColor);
        EventBus<PlayerAwakenedEndedEvent>.Raise(new PlayerAwakenedEndedEvent());
        EventBus<GameStatusEvent>.Raise(new GameStatusEvent("", 0f));
        RaiseStatsChanged();
    }

    IEnumerator DamageFlash()
    {
        SetColor(DamageFlashColor);
        yield return new WaitForSeconds(0.08f);
        SetColor(IsAwakened ? AwakenColor : IsIncapacitated ? IncapColor : NormalColor);
    }

    void SetColor(Color c)
    {
        foreach (var r in _renderers)
            if (r != null) r.material.color = c;
    }
}
