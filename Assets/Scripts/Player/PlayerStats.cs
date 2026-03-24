using System.Collections;
using UnityEngine;

/// <summary>
/// 각자의 지옥 - 플레이어 스탯 시스템
/// HP와 스트레스 수치를 관리합니다.
///
/// 스트레스 루프:
///   전투 중 스트레스 축적 → 임계점 도달 → 전투 불능(3초)
///   → 회복 시 '각성' 상태 발동 (공격속도/이동속도 1.5배, 5초)
/// </summary>
[RequireComponent(typeof(Renderer))]
public class PlayerStats : MonoBehaviour
{
    [Header("HP")]
    public float maxHP      = 100f;
    public float currentHP;

    [Header("Stress (스트레스 수치)")]
    public float maxStress             = 100f;
    public float currentStress;
    public float stressGainRate        = 4f;   // 전투 중 초당 증가량
    public float stressRecoveryRate    = 6f;   // 전투 외 초당 감소량
    public float incapacitatedDuration = 3f;   // 전투 불능 지속 시간(초)
    public float awakenedDuration      = 5f;   // 각성 지속 시간(초)

    // ─── 상태 플래그 ───────────────────────────────────────────
    public bool IsIncapacitated { get; private set; }
    public bool IsAwakened      { get; private set; }

    // ─── 이벤트 ────────────────────────────────────────────────
    public event System.Action onDeath;

    // ─── 색상 상수 ─────────────────────────────────────────────
    static readonly Color NormalColor      = new Color(0.30f, 0.55f, 0.85f); // 파란 회색
    static readonly Color IncapColor       = new Color(0.40f, 0.40f, 0.80f); // 보라색
    static readonly Color AwakenColor      = new Color(1.00f, 0.80f, 0.10f); // 황금색
    static readonly Color DamageFlashColor = new Color(1.00f, 0.20f, 0.20f); // 빨간색

    private Renderer[] renderers;
    private Coroutine  stressCoroutine;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        currentHP     = maxHP;
        currentStress = 0f;
        renderers     = GetComponentsInChildren<Renderer>();
        SetColor(NormalColor);
    }

    void Update()
    {
        if (IsIncapacitated || IsAwakened) return;

        // 적이 살아있으면 스트레스 증가, 없으면 감소
        bool inCombat = EnemyController.ActiveCount > 0;
        if (inCombat)
            currentStress = Mathf.Min(currentStress + stressGainRate * Time.deltaTime, maxStress);
        else
            currentStress = Mathf.Max(currentStress - stressRecoveryRate * Time.deltaTime, 0f);

        if (currentStress >= maxStress)
            stressCoroutine = StartCoroutine(IncapRoutine());
    }

    // ─── 피해 처리 ─────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (IsIncapacitated) return;

        currentHP     = Mathf.Max(currentHP - amount, 0f);
        currentStress = Mathf.Min(currentStress + amount * 0.4f, maxStress);

        StartCoroutine(DamageFlash());

        if (currentHP <= 0f) Die();
    }

    void Die()
    {
        onDeath?.Invoke();
        GameManager.Instance?.TriggerGameOver();
    }

    // ─── 전투 불능 → 각성 루틴 ────────────────────────────────
    IEnumerator IncapRoutine()
    {
        IsIncapacitated = true;
        SetColor(IncapColor);
        GameManager.Instance?.PostStatus("전투 불능!");

        yield return new WaitForSeconds(incapacitatedDuration);

        IsIncapacitated = false;
        currentStress   = 0f;

        // 각성
        IsAwakened = true;
        SetColor(AwakenColor);
        GameManager.Instance?.PostStatus("각성!");

        yield return new WaitForSeconds(awakenedDuration);

        IsAwakened = false;
        SetColor(NormalColor);
        GameManager.Instance?.PostStatus("");
    }

    IEnumerator DamageFlash()
    {
        SetColor(DamageFlashColor);
        yield return new WaitForSeconds(0.08f);
        SetColor(IsAwakened ? AwakenColor : IsIncapacitated ? IncapColor : NormalColor);
    }

    void SetColor(Color c)
    {
        foreach (var r in renderers)
            if (r != null) r.material.color = c;
    }
}
