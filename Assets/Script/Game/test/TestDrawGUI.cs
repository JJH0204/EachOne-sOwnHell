using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class TestDrawGUI : MonoBehaviour
{
    [FormerlySerializedAs("is_Lobby")] [Header("�׽�Ʈ GUI")]
    public bool isLobby = false;
    [FormerlySerializedAs("use_esc")] public bool useEsc = false;
    [FormerlySerializedAs("is_Arena")] public bool isArena = false;
    [FormerlySerializedAs("is_Ada")] public bool isAda = false;

    // public InputAction esc;

    private readonly List<GameObject> _enemies = new();
    private readonly Dictionary<GameObject, float> _enemyHpRatios = new();
    private int   _enemyCount;
    private float _damage;
    private float _currentHp;
    private float _moveSpeed;
    private float _skill1NextFireTime;
    private float _skill1Cooldown;
    private bool  _isUsingSkill;

    private void OnEnable()
    {
        EventBus<CharacterChangedEvent>.Subscribe(OnCharacterChanged);
        EventBus<EnemyRegisteredEvent>.Subscribe(OnEnemyRegistered);
        EventBus<EnemyUnregisteredEvent>.Subscribe(OnEnemyUnregistered);
        EventBus<EnemyHpChangedEvent>.Subscribe(OnEnemyHpChanged);
        EventBus<EnemyCountChangedEvent>.Subscribe(OnEnemyCountChanged);
        EventBus<PlayerStatsChangedEvent>.Subscribe(OnPlayerStatsChanged);
        EventBus<PlayerMovementChangedEvent>.Subscribe(OnPlayerMovementChanged);
        EventBus<PlayerSkillStateChangedEvent>.Subscribe(OnPlayerSkillStateChanged);
    }

    private void OnDisable()
    {
        EventBus<CharacterChangedEvent>.Unsubscribe(OnCharacterChanged);
        EventBus<EnemyRegisteredEvent>.Unsubscribe(OnEnemyRegistered);
        EventBus<EnemyUnregisteredEvent>.Unsubscribe(OnEnemyUnregistered);
        EventBus<EnemyHpChangedEvent>.Unsubscribe(OnEnemyHpChanged);
        EventBus<EnemyCountChangedEvent>.Unsubscribe(OnEnemyCountChanged);
        EventBus<PlayerStatsChangedEvent>.Unsubscribe(OnPlayerStatsChanged);
        EventBus<PlayerMovementChangedEvent>.Unsubscribe(OnPlayerMovementChanged);
        EventBus<PlayerSkillStateChangedEvent>.Unsubscribe(OnPlayerSkillStateChanged);
    }

    private void OnCharacterChanged(CharacterChangedEvent evt) => isAda = evt.IsAda;

    private void OnEnemyRegistered(EnemyRegisteredEvent evt)
    {
        if (!_enemies.Contains(evt.Enemy))
            _enemies.Add(evt.Enemy);
        _enemyHpRatios[evt.Enemy] = 1f;
    }

    private void OnEnemyUnregistered(EnemyUnregisteredEvent evt)
    {
        _enemies.Remove(evt.Enemy);
        _enemyHpRatios.Remove(evt.Enemy);
    }

    private void OnEnemyHpChanged(EnemyHpChangedEvent evt)       => _enemyHpRatios[evt.Enemy] = evt.HpRatio;
    private void OnEnemyCountChanged(EnemyCountChangedEvent evt) => _enemyCount = evt.Count;

    private void OnPlayerStatsChanged(PlayerStatsChangedEvent evt)
    {
        _damage    = evt.Damage;
        _currentHp = evt.CurrentHP;
    }

    private void OnPlayerMovementChanged(PlayerMovementChangedEvent evt) => _moveSpeed = evt.MoveSpeed;

    private void OnPlayerSkillStateChanged(PlayerSkillStateChangedEvent evt)
    {
        _skill1NextFireTime = evt.Skill1NextFireTime;
        _skill1Cooldown     = evt.Skill1Cooldown;
        _isUsingSkill       = evt.IsUsingSkill;
    }

    private void Start()
    {
        var currentScene = SceneManager.GetActiveScene();

        switch (currentScene.name)
        {
            case "Arena":
                isArena = true;
                break;
            case "Test_Lobby":
                isLobby = true;
                break;
        }
    }
    
    private void UseEsc(InputAction.CallbackContext context)
    {
        useEsc = !useEsc;

        Time.timeScale = useEsc ? 0f : 1f;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnGUI()
    {

        if (isLobby)
        {
            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.Label(new Rect(Screen.width - 300, 10, 300, 110),
                "WASD : �̵�\n �簢�� ������ ����Ű ������ Arena������ �Ѿ");
            GUI.color = Color.white;
        }

        if (isArena)
        {
            var enemyCountStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = Color.white
                }
            };

            GUI.Label(new Rect(Screen.width / 2f - 100f, 20f, 200f, 40f), $"���� �� �� : {_enemyCount}", enemyCountStyle);


            DrawEnemyHpBars();

        }

        var statsTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            alignment = TextAnchor.MiddleCenter
        };
        if (useEsc)
        {
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 62,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal =
                {
                    textColor = new Color(0.9f, 0.3f, 0.3f)
                }
            };

            var subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                }
            };

            var cx = Screen.width / 2f;
            var cy = Screen.height / 2f;

            var statBoxX = cx - 650;
            var statBoxY = cy - 120;
            const float statBoxW = 420;
            const float statBoxH = 500;

            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.85f);
            GUI.DrawTexture(new Rect(statBoxX, statBoxY, statBoxW, statBoxH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(cx - 200, cy - 200, 400, 70), "�Ͻ�����", titleStyle);
            GUI.Label(new Rect(cx - 650, cy - 80, 400, 70), "���� ����", statsTitleStyle);
            GUI.Label(new Rect(cx - 650, cy - -10, 200, 70), $"���ݷ� : {_damage}", subStyle);
            GUI.Label(new Rect(cx - 650, cy - -100, 200, 70), $"ü�� : {_currentHp} ", subStyle);
            GUI.Label(new Rect(cx - 650, cy - -200, 200, 70), $"�̼� : {_moveSpeed} ", subStyle);

        }

        if (isAda)
        {
            DrawAdaSkillGUI();
        }
    }


    private void DrawAdaSkillGUI()
    {
        const float boxW = 120f;
        const float boxH = 120f;
        const float startX = 40f;
        var startY = Screen.height - 160f;
        const float gap = 20f;

        var skillStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.black
            }
        };

        var skill1Remain = Mathf.Max(0f, _skill1NextFireTime - Time.time);
        var skill2Text = _isUsingSkill ? "USING" : "READY";

        DrawSkillBox(startX, startY, boxW, boxH, "��ų 1", skill1Remain, skillStyle);
        DrawSkill2Box(startX + boxW + gap, startY, boxW, boxH, "��ų 2", skill2Text, skillStyle);
    }


    void DrawSkillBox(float x, float y, float w, float h, string skillName, float remainCooldown, GUIStyle textStyle)
    {
        GUI.color = new Color(0.55f, 0.52f, 0.62f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        GUI.color = Color.white;
        float border = 3f;
        GUI.DrawTexture(new Rect(x, y, w, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y + h - border, w, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y, border, h), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + w - border, y, border, h), Texture2D.whiteTexture);

        GUI.color = Color.black;
        GUI.Label(new Rect(x, y + 20f, w, 35f), skillName, textStyle);

        if (remainCooldown > 0f)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(x, y + 65f, w, 30f), remainCooldown.ToString("F1") + "s", textStyle);
        }
        else
        {
            GUI.color = Color.black;
            GUI.Label(new Rect(x, y + 65f, w, 30f), "READY", textStyle);
        }

        GUI.color = Color.white;
    }

    private static void DrawSkill2Box(float x, float y, float w, float h, string skillName, string stateText, GUIStyle textStyle)
    {
        GUI.color = new Color(0.55f, 0.52f, 0.62f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        GUI.color = Color.white;
        const float border = 3f;
        GUI.DrawTexture(new Rect(x, y, w, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y + h - border, w, border), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x, y, border, h), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(x + w - border, y, border, h), Texture2D.whiteTexture);

        GUI.color = Color.black;
        GUI.Label(new Rect(x, y + 20f, w, 35f), skillName, textStyle);
        GUI.Label(new Rect(x, y + 65f, w, 30f), stateText, textStyle);

        GUI.color = Color.white;
    }


    private void DrawEnemyHpBars()
    {
        var cam = Camera.main;
        if (!cam) return;

        foreach (var enemy in _enemies)
        {
            if (!enemy) continue;

            var screenPos = cam.WorldToScreenPoint(enemy.transform.position + Vector3.up * 2.0f);
            if (screenPos.z <= 0f) continue;

            var x = screenPos.x - 40f;
            var y = Screen.height - screenPos.y - 10f;

            const float barWidth = 80f;
            const float barHeight = 10f;

            var hpRatio = _enemyHpRatios.TryGetValue(enemy, out var ratio) ? ratio : 1f;

            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);

            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(x, y, barWidth * hpRatio, barHeight), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, barWidth, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + barHeight - 1f, barWidth, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 1f, barHeight), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + barWidth - 1f, y, 1f, barHeight), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }
    }
}
