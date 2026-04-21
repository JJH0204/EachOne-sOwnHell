/// <summary>
/// 각자의 지옥 - 게임 이벤트 정의
/// 모든 이벤트는 읽기 전용 struct 로 정의합니다.
/// </summary>

// ── 탄환 생성 관련 ───────────────────────────────────────────────────────────

/// <summary>탄환 생성 요청 — BulletSpawner가 구독해 BulletHelper.Spawn 실행</summary>
public readonly struct SpawnBulletRequestEvent
{
    public readonly UnityEngine.Vector3 Origin;
    public readonly UnityEngine.Vector3 Direction;
    public readonly float Speed;
    public readonly bool  IsPlayerBullet;
    public readonly bool  IsAutoAim;

    public SpawnBulletRequestEvent(
        UnityEngine.Vector3 origin, UnityEngine.Vector3 direction,
        float speed, bool isPlayerBullet, bool isAutoAim)
    {
        Origin        = origin;
        Direction     = direction;
        Speed         = speed;
        IsPlayerBullet = isPlayerBullet;
        IsAutoAim     = isAutoAim;
    }
}

// ── 탄환 충돌 관련 ───────────────────────────────────────────────────────────

/// <summary>플레이어 탄환이 적에게 명중했을 때 — EnemyController가 구독해 자신이 타깃이면 피해 처리</summary>
public readonly struct BulletHitEnemyEvent
{
    public readonly UnityEngine.Object Target;
    public readonly float              Damage;
    public BulletHitEnemyEvent(UnityEngine.Object target, float damage) { Target = target; Damage = damage; }
}

/// <summary>적 탄환이 플레이어에게 명중했을 때 — PlayerStats가 구독해 피해 처리</summary>
public readonly struct BulletHitPlayerEvent
{
    public readonly float Damage;
    public BulletHitPlayerEvent(float damage) => Damage = damage;
}

/// <summary>적이 플레이어와 접촉 중 피해를 줄 때 — PlayerStats가 구독해 피해 처리</summary>
public readonly struct EnemyContactDamageEvent
{
    public readonly float Damage;
    public EnemyContactDamageEvent(float damage) => Damage = damage;
}

// ── 적 관련 ──────────────────────────────────────────────────────────────────

/// <summary>적이 사망했을 때</summary>
public readonly struct EnemyDiedEvent
{
    public readonly int ScoreValue;
    public EnemyDiedEvent(int scoreValue) => ScoreValue = scoreValue;
}

/// <summary>전장의 적 수가 바뀌었을 때 (스폰 / 사망 시)</summary>
public readonly struct EnemyCountChangedEvent
{
    public readonly int Count;
    public EnemyCountChangedEvent(int count) => Count = count;
}

// ── 플레이어 관련 ─────────────────────────────────────────────────────────────

/// <summary>플레이어가 피해를 받았을 때</summary>
public readonly struct PlayerDamagedEvent
{
    public readonly float Amount;
    public PlayerDamagedEvent(float amount) => Amount = amount;
}

/// <summary>플레이어 HP 가 0이 되었을 때</summary>
public readonly struct PlayerDiedEvent { }

/// <summary>플레이어가 전투 불능 상태에 진입했을 때</summary>
public readonly struct PlayerIncapacitatedEvent { }

/// <summary>전투 불능 해제 후 각성 상태에 진입했을 때</summary>
public readonly struct PlayerAwakenedEvent { }

/// <summary>각성 상태가 종료되었을 때</summary>
public readonly struct PlayerAwakenedEndedEvent { }

// ── 성장 관련 ─────────────────────────────────────────────────────────────────

/// <summary>경험치를 획득했을 때</summary>
public readonly struct ExpGainedEvent
{
    public readonly int Amount;
    public ExpGainedEvent(int amount) => Amount = amount;
}

/// <summary>레벨업 조건 충족 시 — LevelUpUI가 구독해 카드 선택 화면을 띄웁니다</summary>
public readonly struct LevelUpEvent { }

/// <summary>아이템을 획득했을 때</summary>
public readonly struct ItemGainedEvent
{
    public readonly int Amount;
    public ItemGainedEvent(int amount) => Amount = amount;
}

/// <summary>플레이어가 경험치 오브를 획득했을 때 — PlayerStats가 구독해 AddExp 처리</summary>
public readonly struct ExpOrbPickedUpEvent
{
    public readonly int Amount;
    public ExpOrbPickedUpEvent(int amount) => Amount = amount;
}

/// <summary>플레이어가 아이템 오브를 획득했을 때 — PlayerStats가 구독해 AddItem 처리</summary>
public readonly struct ItemOrbPickedUpEvent
{
    public readonly int Amount;
    public ItemOrbPickedUpEvent(int amount) => Amount = amount;
}

// ── UI / HUD 관련 ─────────────────────────────────────────────────────────────

/// <summary>플레이어 스탯이 변경되었을 때 — GameManager가 이 값을 캐싱해 HUD를 그립니다</summary>
public readonly struct PlayerStatsChangedEvent
{
    public readonly float CurrentHP;
    public readonly float MaxHP;
    public readonly float CurrentStress;
    public readonly float MaxStress;
    public readonly float Damage;
    public readonly bool  IsIncapacitated;
    public readonly bool  IsAwakened;

    public PlayerStatsChangedEvent(
        float currentHP, float maxHP,
        float currentStress, float maxStress,
        float damage,
        bool isIncapacitated, bool isAwakened)
    {
        CurrentHP       = currentHP;
        MaxHP           = maxHP;
        CurrentStress   = currentStress;
        MaxStress       = maxStress;
        Damage          = damage;
        IsIncapacitated = isIncapacitated;
        IsAwakened      = isAwakened;
    }
}

/// <summary>플레이어 이동 속도가 변경되었을 때 — PlayerController가 발행</summary>
public readonly struct PlayerMovementChangedEvent
{
    public readonly float MoveSpeed;
    public PlayerMovementChangedEvent(float moveSpeed) => MoveSpeed = moveSpeed;
}

/// <summary>플레이어 스킬 상태가 변경되었을 때 — PlayerLaserAttack이 발행</summary>
public readonly struct PlayerSkillStateChangedEvent
{
    public readonly float Skill1NextFireTime;
    public readonly float Skill1Cooldown;
    public readonly bool  IsUsingSkill;

    public PlayerSkillStateChangedEvent(float skill1NextFireTime, float skill1Cooldown, bool isUsingSkill)
    {
        Skill1NextFireTime = skill1NextFireTime;
        Skill1Cooldown     = skill1Cooldown;
        IsUsingSkill       = isUsingSkill;
    }
}

/// <summary>게임 오버 상태로 전환될 때</summary>
public readonly struct GameOverEvent { }

/// <summary>플레이어가 재시작 키를 눌렀을 때</summary>
public readonly struct RestartRequestedEvent { }

/// <summary>활성 캐릭터가 전환되었을 때 — TestDrawGUI가 구독해 isAda 상태를 갱신합니다</summary>
public readonly struct CharacterChangedEvent
{
    public readonly bool IsAda;
    public CharacterChangedEvent(bool isAda) => IsAda = isAda;
}

/// <summary>화면 중앙에 상태 메시지를 표시할 때</summary>
public readonly struct GameStatusEvent
{
    public readonly string Message;
    public readonly float  Duration;
    public GameStatusEvent(string message, float duration = 2.5f)
    {
        Message  = message;
        Duration = duration;
    }
}

// ── 적 HP바 UI 관련 ──────────────────────────────────────────────────────────

/// <summary>적이 씬에 등록될 때 — TestDrawGUI가 구독해 캐시 목록에 추가</summary>
public readonly struct EnemyRegisteredEvent
{
    public readonly UnityEngine.GameObject Enemy;
    public EnemyRegisteredEvent(UnityEngine.GameObject enemy) => Enemy = enemy;
}

/// <summary>적이 씬에서 제거될 때 (사망 또는 강제 파괴) — TestDrawGUI가 구독해 캐시 목록에서 제거</summary>
public readonly struct EnemyUnregisteredEvent
{
    public readonly UnityEngine.GameObject Enemy;
    public EnemyUnregisteredEvent(UnityEngine.GameObject enemy) => Enemy = enemy;
}

/// <summary>적 HP가 변경되었을 때 — TestDrawGUI가 구독해 HP바 비율을 캐싱</summary>
public readonly struct EnemyHpChangedEvent
{
    public readonly UnityEngine.GameObject Enemy;
    public readonly float                  HpRatio;
    public EnemyHpChangedEvent(UnityEngine.GameObject enemy, float hpRatio) { Enemy = enemy; HpRatio = hpRatio; }
}