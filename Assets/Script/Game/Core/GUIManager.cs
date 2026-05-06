using UnityEngine;

namespace Game.Core

/// <summary>
/// 각자의 지옥 - GUI , OnGuI 처리 및 관리
///
/// 역할:
///   - GUI 관련을 처리 합니다
///   - OnGUI로 그려지는 GUI들을 처리합니다
///   - 기본적인 GUI들은 여기서 관리 되거나 처리 합니다
/// 우선도:
///   - 추후 작업 우선 순위가 높은건 *** 이며 * 가 낮아질수록 우선도가 낮습니다
///   - 게임 시작시 UI가 출력 안됨 (중요도3) : 프로토타입 진입 후 추가되는 내용에 따라 중요도3중 최우선이 되거나 중요도3중 후 순위가 됩니다
///   - 게임 오버 GUI 저장용 (중요도1) : 현재 PlayerDeath 스크립트에서 출력되는 GUI랑 겹치는 상태라 주석처리 했으며 추후 변경 되는게 아니라면 프로토타입 종료 후 삭제 될 예정 입니다
/// </summary>
{
  public class GUIManager : Singleton<GUIManager>
  {
    // ─── HUD 레이아웃 상수 ────────────────────────────────────
    private const float BarX = 16f;
    private const float BarY = 16f;
    private const float BarW = 220f;
    private const float BarH = 22f;
    private const float BarGap = 30f;

    // ─── 표시용 상태 (GameManager에서 갱신) ────────────────────
    private bool _hasPlayerData;
    private float _hp;
    private float _maxHp;
    private float _stress;
    private float _maxStress;
    private bool _isIncap;
    private bool _isAwakened;

    private int _score;
    private string _statusMessage;
    private float _statusExpireTime;

    public bool IsGameOver { get; private set; }

    #region 게임 시작시 UI가 출력 안됨 ***
    //시작시 UI가 출력 안되다가 Enemy 등장 시 UI 출력되는 문제가 있었으며 SetPlayerStats 가 출력이 안되다가 리셋이 먼저 되는 현상으로 추측 됩니다
    //추후 별도로 분리 시켜 초기화 함수로 만들 예정
    public void ResetViewState()
    {
/*      _hasPlayerData = false;
      _hp = 0f;
      _maxHp = 0f;
      _stress = 0f;
      _maxStress = 0f;
      _isIncap = false;
      _isAwakened = false;
      _score = 0;
      _statusMessage = string.Empty;
      _statusExpireTime = 0f;
      IsGameOver = false;*/
    }
    #endregion 

    public void SetPlayerStats(float hp, float maxHp, float stress, float maxStress, bool isIncap, bool isAwakened)
    {
      _hasPlayerData = true;
      _hp = hp;
      _maxHp = maxHp;
      _stress = stress;
      _maxStress = maxStress;
      _isIncap = isIncap;
      _isAwakened = isAwakened;
    }

    public void SetScore(int score)
    {
      _score = Mathf.Max(0, score);
    }

    public void SetStatusMessage(string message, float duration)
    {
      _statusMessage = message ?? string.Empty;
      _statusExpireTime = Time.time + Mathf.Max(0f, duration);
    }

    public void SetGameOver(bool isGameOver)
    {
      IsGameOver = isGameOver;
    }

    // ─── OnGUI HUD ─────────────────────────────────────────────
    private void OnGUI()
    {
      DrawHUD();
      /*      if (IsGameOver) DrawGameOver();*/
    }

    private void DrawHUD()
    {
      if (_hasPlayerData)
      {
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
        var stressLabel = _isIncap ? "전투 불능!"
          : _isAwakened ? "각성!!"
          : $"STRESS  {_stress:F0} / {_maxStress:F0}";
        DrawBar(BarX, BarY + BarGap, BarW, BarH, stressRatio, stressColor, stressLabel);
      }

      // ── 점수 ─────────────────────────────────────────────
      GUI.color = Color.white;
      GUI.Label(new Rect(BarX, BarY + BarGap * 2 + 4, 300, 28),
        $"SCORE  {_score:N0}");

      // ── 상태 메시지 ───────────────────────────────────────
      if (!string.IsNullOrEmpty(_statusMessage) && Time.time < _statusExpireTime)
      {
        var style = new GUIStyle(GUI.skin.label)
        {
          fontSize = 22,
          alignment = TextAnchor.MiddleCenter,
          fontStyle = FontStyle.Bold,
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
    #region 게임오버 GUI 저장용 *
    /*  TODO : 이미 PlayerDeath에 게임오버 관련 GUI가 있으나 추후 A난이도 사망시 곧 바로 엔딩영상 출력에서 바뀔 경우 사용할 예정
    /*    void DrawGameOver()
        {
          GUI.color = new Color(0, 0, 0, 0.7f);
          GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
          GUI.color = Color.white;

          var titleStyle = new GUIStyle(GUI.skin.label)
          {
            fontSize = 42,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
              textColor = new Color(0.9f, 0.3f, 0.3f)
            }
          };

          var subStyle = new GUIStyle(GUI.skin.label)
          {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
              textColor = Color.white
            }
          };

          float cx = Screen.width / 2f;
          float cy = Screen.height / 2f;

          GUI.Label(new Rect(cx - 200, cy - 80, 400, 70), "GAME OVER", titleStyle);
          GUI.Label(new Rect(cx - 200, cy, 400, 40), $"FINAL SCORE : {_score:N0}", subStyle);
          GUI.Label(new Rect(cx - 200, cy + 50, 400, 30), "R 키 : 재시작", subStyle);
        }*/
    #endregion
  }
}
