각자의 지옥 - 제네릭 정적 이벤트 버스
사용법:
  발행 : EventBus&lt;EnemyDiedEvent&gt;.Raise(new EnemyDiedEvent(score));
  구독 : EventBus&lt;EnemyDiedEvent&gt;.Subscribe(OnEnemyDied);   // OnEnable
  해제 : EventBus&lt;EnemyDiedEvent&gt;.Unsubscribe(OnEnemyDied); // OnDisable
주의:
  - 이벤트 타입은 struct 여야 합니다 (GameEvents.cs 참고).
  - MonoBehaviour 구독자는 반드시 OnDisable/OnDestroy에서 Unsubscribe 해야 합니다.