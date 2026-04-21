using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TestDrawGUI : MonoBehaviour
{
    [Header("테스트 GUI")]
    public bool is_Lobby = false;
    public bool use_esc = false;
    public bool is_Arena = false;
    public bool is_Ada = false;

    public InputAction esc;

    public GameObject Ada;

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Player_LaserAttack laserAttack;

    void Start()
    {

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.name == "Arena")
        {
            is_Arena = true;
        }

        if (currentScene.name == "Test_Lobby")
        {
            is_Lobby = true;
        }

    }



    private void OnEnable()
    {
        esc.performed += useesc;

        esc.Enable();
    }

    private void OnDisable()
    {
        esc.performed -= useesc;

        esc.Disable();
    }

    private void useesc(InputAction.CallbackContext context)
    {
        use_esc = !use_esc;

        if (use_esc)
            Time.timeScale = 0f;
        else
            Time.timeScale = 1f;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnGUI()
    {

        if (is_Lobby)
        {
            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.Label(new Rect(Screen.width - 300, 10, 300, 110),
                "WASD : 이동\n 사각형 위에서 엔터키 누를시 Arena씬으로 넘어감");
            GUI.color = Color.white;
        }

        if (is_Arena)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            int enemyCount = enemies.Length;

            GUIStyle enemyCountStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            enemyCountStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width / 2f - 100f, 20f, 200f, 40f), $"남은 몹 수 : {enemyCount}", enemyCountStyle);


            DrawEnemyHPBars();

        }

        if (use_esc)
        {
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 62,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle StatsTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                alignment = TextAnchor.MiddleCenter
            };

            subStyle.normal.textColor = Color.white;

            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            float statBoxX = cx - 650;
            float statBoxY = cy - 120;
            float statBoxW = 420;
            float statBoxH = 500;

            GUI.color = new Color(0.3f, 0.3f, 0.3f, 0.85f);
            GUI.DrawTexture(new Rect(statBoxX, statBoxY, statBoxW, statBoxH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(cx - 200, cy - 200, 400, 70), "일시정지", titleStyle);
            GUI.Label(new Rect(cx - 650, cy - 80, 400, 70), "현재 스텟", StatsTitleStyle);
            GUI.Label(new Rect(cx - 650, cy - -10, 200, 70), $"공격력 : {playerStats.Damage}", subStyle);
            GUI.Label(new Rect(cx - 650, cy - -100, 200, 70), $"체력 : {playerStats.currentHP} ", subStyle);
            GUI.Label(new Rect(cx - 650, cy - -200, 200, 70), $"이속 : {playerController.moveSpeed} ", subStyle);

        }

        if (is_Ada)
        {
            DrawAdaSkillGUI();
        }
    }



    void DrawAdaSkillGUI()
    {
        float boxW = 120f;
        float boxH = 120f;
        float startX = 40f;
        float startY = Screen.height - 160f;
        float gap = 20f;

        GUIStyle skillStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        skillStyle.normal.textColor = Color.black;

        float skill1Remain = laserAttack.Skill1RemainCooldown;
        string skill2Text = laserAttack.IsUsingSkill ? "USING" : "READY";

        DrawSkillBox(startX, startY, boxW, boxH, "스킬 1", skill1Remain, skillStyle);
        DrawSkill2Box(startX + boxW + gap, startY, boxW, boxH, "스킬 2", skill2Text, skillStyle);
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

    void DrawSkill2Box(float x, float y, float w, float h, string skillName, string stateText, GUIStyle textStyle)
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
        GUI.Label(new Rect(x, y + 65f, w, 30f), stateText, textStyle);

        GUI.color = Color.white;
    }


    void DrawEnemyHPBars()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemyObj in enemies)
        {
            if (enemyObj == null) continue;

            EnemyController enemy = enemyObj.GetComponent<EnemyController>();
            if (enemy == null) continue;

            // 머리 위 위치
            Vector3 worldPos = enemyObj.transform.position + Vector3.up * 2.0f;

            // 월드 -> 스크린
            Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

            // 카메라 뒤에 있으면 그리지 않음
            if (screenPos.z <= 0f) continue;

            // OnGUI는 Y축이 반대라 뒤집어야 함
            float x = screenPos.x - 40f;
            float y = Screen.height - screenPos.y - 10f;

            float barWidth = 80f;
            float barHeight = 10f;

            // hp 비율
            float hpRatio = enemy.hp / enemy.hp;
            hpRatio = Mathf.Clamp01(hpRatio);

            // 배경 바
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(x, y, barWidth, barHeight), Texture2D.whiteTexture);

            // 현재 hp 바
            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(x, y, barWidth * hpRatio, barHeight), Texture2D.whiteTexture);

            // 테두리
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(x, y, barWidth, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y + barHeight - 1f, barWidth, 1f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x, y, 1f, barHeight), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(x + barWidth - 1f, y, 1f, barHeight), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }
    }
}
