각자의 지옥 - 기본 적 컨트롤러 (FSM 기반)

상태 머신:
  Idle   → (플레이어 감지 범위 진입) → Chase
  Chase  → (공격 범위 진입)          → Attack
  Attack → (공격 범위 이탈)          → Chase
  Any    → (HP = 0)                  → Dead

이벤트 버스:
  구독 BulletHitEnemyEvent    - 플레이어 탄환 명중 → TakeDamage 처리
  발행 EnemyCountChangedEvent - 스폰/사망 시 적 수 변경 알림
  발행 EnemyDiedEvent         - 사망 시 점수값 전달