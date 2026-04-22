using UnityEngine;

// TODO: inputsystem 사용법 다시 확인하기
// using UnityEngine.InputSystem;

namespace Game.Core
{
  public class GameManager : Singleton<GameManager>
  {
    private bool IsGameOver { get; set; }

    // ─── 내부 상태 ─────────────────────────────────────────────
    private int _score;

    // ───────────────────────────────────────────────────────────

    private void OnEnable()
    {
      EventBus<EnemyDiedEvent>.Subscribe(OnEnemyDied);
      EventBus<GameStatusEvent>.Subscribe(OnGameStatus);
      EventBus<PlayerStatsChangedEvent>.Subscribe(OnPlayerStatsChanged);
      EventBus<RestartRequestedEvent>.Subscribe(OnRestartRequested);
      EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
    }

    private void OnDisable()
    {
      EventBus<EnemyDiedEvent>.Unsubscribe(OnEnemyDied);
      EventBus<GameStatusEvent>.Unsubscribe(OnGameStatus);
      EventBus<PlayerStatsChangedEvent>.Unsubscribe(OnPlayerStatsChanged);
      EventBus<RestartRequestedEvent>.Unsubscribe(OnRestartRequested);
      EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
    }

    private void OnEnemyDied(EnemyDiedEvent evt)
    {
      _score += evt.ScoreValue;
      GUIManager.Instance?.SetScore(_score);
    }

    private void OnPlayerDied(PlayerDiedEvent _) => TriggerGameOver();

    private void OnGameStatus(GameStatusEvent evt) => PostStatus(evt.Message, evt.Duration);

    private void OnPlayerStatsChanged(PlayerStatsChangedEvent evt)
    {
      GUIManager.Instance?.SetPlayerStats(
        evt.CurrentHP,
        evt.MaxHP,
        evt.CurrentStress,
        evt.MaxStress,
        evt.IsIncapacitated,
        evt.IsAwakened);
    }

    private void Start()
    {
      IsGameOver = false;
      GUIManager.Instance?.ResetViewState();
      GUIManager.Instance?.SetScore(_score);
    }

    private void Update()
    {
      // 입력은 이벤트로만 전달하고 실제 처리 여부는 핸들러에서 판단한다.
      if (Input.GetKeyDown(KeyCode.R))
      {
        EventBus<RestartRequestedEvent>.Raise(new RestartRequestedEvent());
      }
    }

    private void OnRestartRequested(RestartRequestedEvent _)
    {
      if (IsGameOver) RestartScene();
    }

    // ─── 점수 ──────────────────────────────────────────────────
    public void AddScore(int amount)
    {
      _score += amount;
      GUIManager.Instance?.SetScore(_score);
    }

    // ─── 상태 메시지 ───────────────────────────────────────────
    private void PostStatus(string msg, float duration = 2.5f)
    {
      GUIManager.Instance?.SetStatusMessage(msg, duration);
    }

    // ─── 게임 오버 ─────────────────────────────────────────────
    private void TriggerGameOver()
    {
      if (IsGameOver) return;
      IsGameOver = true;
      GUIManager.Instance?.SetGameOver(true);
      EventBus<GameOverEvent>.Raise(new GameOverEvent());
    }

    private static void RestartScene()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
  }
}
