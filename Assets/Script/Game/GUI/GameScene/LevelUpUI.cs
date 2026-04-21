using UnityEngine;

public class LevelUpUI : MonoBehaviour
{
    public bool showLevelUpUI = false;

    void OnEnable()  => EventBus<LevelUpEvent>.Subscribe(OnLevelUp);
    void OnDisable() => EventBus<LevelUpEvent>.Unsubscribe(OnLevelUp);

    void OnLevelUp(LevelUpEvent _)
    {
        if (showLevelUpUI) return;
        showLevelUpUI = true;
        Time.timeScale = 0f;
    }

    private void OnGUI()
    {
        if (!showLevelUpUI)
            return;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float cardWidth = 200f;
        float cardHeight = 300f;
        float gap = 30f;

        float totalWidth = cardWidth * 3 + gap * 2;
        float startX = (Screen.width - totalWidth) * 0.5f;
        float y = (Screen.height - cardHeight) * 0.5f;

        DrawCard(startX, y, cardWidth, cardHeight, "���ݷ� ����", "���ݷ� +10%");
        DrawCard(startX + cardWidth + gap, y, cardWidth, cardHeight, "ü�� ����", "�ִ� ü�� +20");
        DrawCard(startX + (cardWidth + gap) * 2, y, cardWidth, cardHeight, "�̵��ӵ� ����", "�̵��ӵ� +10%");
    }

    void DrawCard(float x, float y, float w, float h, string title, string desc)
    {
        GUI.Box(new Rect(x, y, w, h), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.UpperCenter;
        titleStyle.fontSize = 20;
        titleStyle.fontStyle = FontStyle.Bold;

        GUIStyle descStyle = new GUIStyle(GUI.skin.label);
        descStyle.alignment = TextAnchor.UpperLeft;
        descStyle.wordWrap = true;
        descStyle.fontSize = 14;

        GUI.Label(new Rect(x + 10, y + 20, w - 20, 30), title, titleStyle);
        GUI.Label(new Rect(x + 10, y + 70, w - 20, 120), desc, descStyle);

        if (GUI.Button(new Rect(x + 40, y + h - 50, w - 80, 30), "����"))
        {
            Debug.Log($"{title} ����");
            showLevelUpUI = false;
            Time.timeScale = 1f;
        }
    }
}