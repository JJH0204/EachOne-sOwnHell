using UnityEngine;
// TODO: inputsystem 사용법 다시 확인하기
// using UnityEngine.InputSystem;

namespace Game.Core
{
    public class GameManager : Singleton<GameManager>
    {
        private bool IsGameOver { get; set; }

        // ─── 내부 상태 ─────────────────────────────────────────────
        private int    _score;
        private string _statusMessage;
        private float  _statusExpireTime;

        // ─── HUD 레이아웃 상수 ────────────────────────────────────
        private const float BarX   = 16f;
        private const float BarY   = 16f;
        private const float BarW   = 220f;
        private const float BarH   = 22f;
        private const float BarGap = 30f;
        
        // ─── 플레이어 스탯 캐시 (PlayerStatsChangedEvent로 갱신) ──
        private bool  _hasPlayerData;
        private float _hp, _maxHp, _stress, _maxStress;
        private bool  _isIncap, _isAwakened;

        // ───────────────────────────────────────────────────────────

        void OnEnable()
        {
            EventBus<EnemyDiedEvent>.Subscribe(OnEnemyDied);
            EventBus<GameStatusEvent>.Subscribe(OnGameStatus);
            EventBus<PlayerStatsChangedEvent>.Subscribe(OnPlayerStatsChanged);
            EventBus<RestartRequestedEvent>.Subscribe(OnRestartRequested);
            EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
        }

        void OnDisable()
        {
            EventBus<EnemyDiedEvent>.Unsubscribe(OnEnemyDied);
            EventBus<GameStatusEvent>.Unsubscribe(OnGameStatus);
            EventBus<PlayerStatsChangedEvent>.Unsubscribe(OnPlayerStatsChanged);
            EventBus<RestartRequestedEvent>.Unsubscribe(OnRestartRequested);
            EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
        }

        private void OnEnemyDied(EnemyDiedEvent evt) => _score += evt.ScoreValue;
        private void OnPlayerDied(PlayerDiedEvent _) => TriggerGameOver();

        private void OnGameStatus(GameStatusEvent evt) => PostStatus(evt.Message, evt.Duration);

        private void OnPlayerStatsChanged(PlayerStatsChangedEvent evt)
        {
            _hasPlayerData = true;
            _hp            = evt.CurrentHP;
            _maxHp         = evt.MaxHP;
            _stress        = evt.CurrentStress;
            _maxStress     = evt.MaxStress;
            _isIncap       = evt.IsIncapacitated;
            _isAwakened    = evt.IsAwakened;
            // evt.Damage — 필요 시 HUD에 표시 가능
        }

        private void Start()
        {
            IsGameOver = false;
        }

        private void Update()
        {
            // ── 입력 → 이벤트 발행 (게임 로직은 핸들러에서) ──────
            // TODO: 입력 시스템이 바뀌면 이 부분도 수정 필요
            // if (UnityEngine.InputSystem.Keyboard.current?.rKey.wasPressedThisFrame == true)
                EventBus<RestartRequestedEvent>.Raise(new RestartRequestedEvent());
        }

        private void OnRestartRequested(RestartRequestedEvent _)
        {
            if (IsGameOver) RestartScene();
        }

        // ─── 점수 ──────────────────────────────────────────────────
        public void AddScore(int amount) => _score += amount;

        // ─── 상태 메시지 ───────────────────────────────────────────
        private void PostStatus(string msg, float duration = 2.5f)
        {
            _statusMessage    = msg;
            _statusExpireTime = Time.time + duration;
        }

        // ─── 게임 오버 ─────────────────────────────────────────────
        private void TriggerGameOver()
        {
            if (IsGameOver) return;
            IsGameOver = true;
            EventBus<GameOverEvent>.Raise(new GameOverEvent());
        }

        void RestartScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        // ─── OnGUI HUD ─────────────────────────────────────────────
        void OnGUI()
        {
            DrawHUD();
            if (IsGameOver) DrawGameOver();
        }

        void DrawHUD()
        {
            if (!_hasPlayerData) return;

            // 배경 패널
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.Box(new Rect(BarX - 8, BarY - 8, BarW + 16, BarH * 2 + BarGap + 12), GUIContent.none);
            GUI.color = Color.white;

            // ── HP 바 ────────────────────────────────────────────
            var hpRatio = _maxHp > 0f ? _hp / _maxHp : 0f;
            DrawBar(BarX, BarY, BarW, BarH,
                hpRatio,
                new Color(0.2f, 0.8f, 0.2f),
                $"HP  {_hp:F0} / {_maxHp:F0}");

            // ── 스트레스 바 ──────────────────────────────────────
            var stressRatio = _maxStress > 0f ? _stress / _maxStress : 0f;
            var stressColor = Color.Lerp(new Color(0.3f, 0.5f, 1f), new Color(1f, 0.15f, 0.15f), stressRatio);
            var stressLabel = _isIncap    ? "전투 불능!"
                               : _isAwakened ? "각성!!"
                               : $"STRESS  {_stress:F0} / {_maxStress:F0}";
            DrawBar(BarX, BarY + BarGap, BarW, BarH, stressRatio, stressColor, stressLabel);

            // ── 점수 ─────────────────────────────────────────────
            GUI.color = Color.white;
            GUI.Label(new Rect(BarX, BarY + BarGap * 2 + 4, 300, 28),
                $"SCORE  {_score:N0}");

            // ── 상태 메시지 ───────────────────────────────────────
            if (!string.IsNullOrEmpty(_statusMessage) && Time.time < _statusExpireTime)
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize   = 22,
                    alignment  = TextAnchor.MiddleCenter,
                    fontStyle  = FontStyle.Bold,
                    normal =
                    {
                        textColor = Color.yellow
                    }
                };
                GUI.Label(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 40, 320, 50),
                    _statusMessage, style);
            }

            // ── 조작 안내 (우측 상단) ────────────────────────────
            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.Label(new Rect(Screen.width - 220, 10, 210, 110),
                "WASD : 이동\nLMB  : 사격\n적 처치 : 점수 획득\n스트레스 MAX → 전투 불능 → 각성\nQ,E : 캐릭터 변경\n마우스 오른쪽,왼쪽 : 스킬1,2");
            GUI.color = Color.white;
        }

        void DrawBar(float x, float y, float w, float h, float ratio, Color fillColor, string label)
        {
            GUI.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(ratio), h), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
            GUI.Label(new Rect(x + 4, y, w - 4, h), label, s);
        }

        void DrawGameOver()
        {
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 42,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.9f, 0.3f, 0.3f)
                }
            };

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 20,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                }
            };

            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            GUI.Label(new Rect(cx - 200, cy - 80, 400, 70), "GAME OVER", titleStyle);
            GUI.Label(new Rect(cx - 200, cy,       400, 40), $"FINAL SCORE : {_score:N0}", subStyle);
            GUI.Label(new Rect(cx - 200, cy + 50,  400, 30), "R 키 : 재시작", subStyle);
        }
    }   
}