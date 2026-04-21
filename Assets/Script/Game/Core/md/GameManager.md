각자의 지옥 - 게임 매니저 (싱글턴)

역할:
  - 점수 집계
  - 게임 오버 처리 및 재시작
  - OnGUI로 그레이박스 HUD 표시
    (HP 바, 스트레스 바, 점수, 상태 텍스트)

적 스폰은 EnemySpawner 가 담당합니다.

이벤트 버스:
  발행 GameOverEvent           - 게임 오버 전환 시
  구독 EnemyDiedEvent          - 점수 추가
  구독 PlayerDiedEvent         - 게임 오버 트리거
  구독 PlayerStatsChangedEvent - 플레이어 HP/Stress/상태 캐싱 → HUD 렌더링
  구독 GameStatusEvent         - HUD 상태 메시지 표시
  구독 RestartRequestedEvent   - 재시작 키 입력 처리

TODO (수직 슬라이스 단계):
  - 로그라이트 런 관리
  - 공명수치(Resonance) + 조율(Calibration) 시스템