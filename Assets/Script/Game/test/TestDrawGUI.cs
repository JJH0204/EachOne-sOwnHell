using UnityEngine;
using System.Collections;

public class TestDrawGUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnGUI()
    {
        GUI.color = new Color(1, 1, 1, 0.6f);
        GUI.Label(new Rect(Screen.width - 300, 10, 300, 110),
            "WASD : 이동\n 사각형 위에서 엔터키 누를시 Arena씬으로 넘어감");
        GUI.color = Color.white;
    }
}
