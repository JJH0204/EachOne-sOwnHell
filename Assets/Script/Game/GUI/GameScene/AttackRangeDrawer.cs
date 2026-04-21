using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackRangeDrawer : MonoBehaviour
{
    public int segments = 50; // 원을 구성하는 선의 개수
    public float radius = 5f; // 공격 사거리
    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = segments + 1;
        line.useWorldSpace = false; // 오브젝트 이동 시 함께 이동하도록 설정
        DrawCircle();
    }

    void OnValidate()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        SetupLineRenderer();
        DrawCircle();
    }

    void SetupLineRenderer()
    {
        if (line == null) return;

        line.positionCount = segments + 1;
        line.useWorldSpace = false;
    }

    void DrawCircle()
    {
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            // 호도법(Radian) 변환: 2 * PI * (현재 인덱스 / 전체 분할 수)
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, 0, z));
            angle += (2f * Mathf.PI) / segments;
        }
    }

    // 사거리가 변할 때 실시간 업데이트 (예: Update 또는 특정 이벤트)
    public void UpdateRange(float newRange)
    {
        radius = newRange;
        DrawCircle();
    }
}