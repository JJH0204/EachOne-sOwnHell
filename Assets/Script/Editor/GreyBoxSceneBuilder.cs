#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// 각자의 지옥 - 그레이박스 씬 자동 빌더
///
/// 사용법:
///   Unity 메뉴 → 각자의 지옥 → 🔨 Setup Grey Box Scene
///
/// 빌드 내용:
///   ① 아레나 바닥 + 벽 4면
///   ② 플레이어 (Capsule) + 카메라 설정
///   ③ GameManager 오브젝트
///   ④ 초기 테스트 적 1마리
///   ⑤ 방향광 설정
/// </summary>
public static class GreyBoxSceneBuilder
{
    // ── 아레나 크기 상수 ──────────────────────────────────────
    const float ARENA    = 24f;   // 전체 가로/세로 크기
    const float WALL_H   = 2.5f;  // 벽 높이
    const float WALL_T   = 1.0f;  // 벽 두께

    // ── 색상 상수 ─────────────────────────────────────────────
    static readonly Color FloorColor  = new Color(0.22f, 0.22f, 0.22f);
    static readonly Color WallColor   = new Color(0.35f, 0.35f, 0.35f);
    static readonly Color PlayerColor = new Color(0.30f, 0.55f, 0.85f);
    static readonly Color EnemyColor  = new Color(0.55f, 0.20f, 0.20f);

    // ───────────────────────────────────────────────────────────
    [MenuItem("각자의 지옥/🔨 Setup Grey Box Scene")]
    static void Build()
    {
        bool ok = EditorUtility.DisplayDialog(
            "각자의 지옥 - Grey Box Setup",
            "현재 씬의 기존 오브젝트를 모두 삭제하고\n그레이박스 목업 씬을 구성합니다.\n\n계속하시겠습니까?",
            "예, 구성합니다", "취소");
        if (!ok) return;

        // 씬 클리어
        ClearScene();

        // ── 커스텀 태그 등록 ─────────────────────────────────
        EnsureTag("Wall");

        // ── 방향광 ───────────────────────────────────────────
        SetupLight();

        // ── 바닥 ─────────────────────────────────────────────
        SetupFloor();

        // ── 벽 4면 ───────────────────────────────────────────
        SetupWalls();

        // ── 카메라 ───────────────────────────────────────────
        GameObject cam = SetupCamera();

        // ── 플레이어 ─────────────────────────────────────────
        GameObject player = SetupPlayer();

        // ── 카메라 → 플레이어 연결 ───────────────────────────
        cam.GetComponent<QuarterviewCamera>().target = player.transform;

        // ── GameManager ───────────────────────────────────────
        SetupGameManager();

        // ── 테스트 적 ─────────────────────────────────────────
        SetupTestEnemy();

        // ── 씬 저장 요청 ──────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(
            SceneManager.GetActiveScene());

        Debug.Log("[각자의 지옥] 그레이박스 씬 빌드 완료! 플레이 버튼을 눌러 테스트하세요.");
        EditorUtility.DisplayDialog(
            "완료",
            "그레이박스 씬이 준비됐습니다!\n\n" +
            "▶ 플레이 버튼으로 바로 테스트 가능합니다.\n" +
            "WASD: 이동 | 마우스 LMB: 사격\n\n" +
            "⚠ 씬을 Ctrl+S로 저장해 주세요.",
            "확인");
    }

    // ───────────────────────────────────────────────────────────
    static void ClearScene()
    {
        var objs = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in objs)
            if (go.transform.parent == null)
                Object.DestroyImmediate(go);
    }

    // ── 방향광 ───────────────────────────────────────────────
    static void SetupLight()
    {
        GameObject go    = new GameObject("Directional Light");
        var light        = go.AddComponent<Light>();
        light.type       = LightType.Directional;
        light.intensity  = 1.2f;
        light.color      = new Color(1f, 0.97f, 0.88f);
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Register(go);
    }

    // ── 바닥 ─────────────────────────────────────────────────
    static void SetupFloor()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Arena_Floor";
        go.transform.localScale = new Vector3(ARENA * 0.1f, 1f, ARENA * 0.1f);
        SetMaterialColor(go, FloorColor);
        Register(go);
    }

    // ── 벽 4면 ───────────────────────────────────────────────
    static void SetupWalls()
    {
        float half = ARENA * 0.5f;

        CreateWall("Wall_North", new Vector3( 0,      WALL_H * 0.5f,  half),     new Vector3(ARENA, WALL_H, WALL_T));
        CreateWall("Wall_South", new Vector3( 0,      WALL_H * 0.5f, -half),     new Vector3(ARENA, WALL_H, WALL_T));
        CreateWall("Wall_East",  new Vector3( half,   WALL_H * 0.5f,  0),        new Vector3(WALL_T, WALL_H, ARENA));
        CreateWall("Wall_West",  new Vector3(-half,   WALL_H * 0.5f,  0),        new Vector3(WALL_T, WALL_H, ARENA));
    }

    static void CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.position   = pos;
        go.transform.localScale = scale;
        go.tag = "Wall";
        SetMaterialColor(go, WallColor);
        Register(go);
    }

    // ── 카메라 ───────────────────────────────────────────────
    static GameObject SetupCamera()
    {
        // Main Camera 생성
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";

        var cam         = go.AddComponent<Camera>();
        cam.clearFlags  = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f);
        cam.fieldOfView = 50f;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane  = 100f;

        go.AddComponent<AudioListener>();

        var qvc         = go.AddComponent<QuarterviewCamera>();
        qvc.offset      = new Vector3(-9f, 13f, -9f);
        qvc.pitchAngle  = 45f;
        qvc.yawAngle    = 45f;
        qvc.smoothSpeed = 8f;

        go.transform.position = new Vector3(-9f, 13f, -9f);
        go.transform.rotation = Quaternion.Euler(45f, 45f, 0f);

        Register(go);
        return go;
    }

    // ── 플레이어 ─────────────────────────────────────────────
    static GameObject SetupPlayer()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Player";
        go.tag  = "Player";
        go.transform.position   = Vector3.zero;
        go.transform.localScale = new Vector3(0.7f, 0.85f, 0.7f);
        SetMaterialColor(go, PlayerColor);

        // Rigidbody (PlayerController가 RequireComponent로도 추가하지만 미리 설정)
        var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
        rb.useGravity   = false;
        rb.constraints  = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // 스탯 (PlayerStats가 Renderer를 RequireComponent로 요구함)
        var stats = go.AddComponent<PlayerStats>();
        stats.maxHP          = 100f;
        stats.maxStress      = 100f;
        stats.stressGainRate = 4f;

        // 컨트롤러
        var ctrl = go.AddComponent<PlayerController>();
        ctrl.moveSpeed   = 6f;
        ctrl.fireRate    = 0.15f;
        ctrl.bulletSpeed = 20f;

        Register(go);
        return go;
    }

    // ── GameManager ───────────────────────────────────────────
    static void SetupGameManager()
    {
        GameObject go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        gm.enemySpawnInterval = 5f;
        gm.maxEnemies         = 5;
        gm.arenaRadius        = ARENA * 0.5f * 0.88f;
        Register(go);
    }

    // ── 테스트 적 ─────────────────────────────────────────────
    static void SetupTestEnemy()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Enemy_Test";
        go.transform.position   = new Vector3(5f, 0f, 5f);
        go.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
        SetMaterialColor(go, EnemyColor);

        var rb = go.GetComponent<Rigidbody>() ?? go.AddComponent<Rigidbody>();
        rb.useGravity   = false;
        rb.constraints  = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var enemy   = go.AddComponent<EnemyController>();
        enemy.hp        = 30f;
        enemy.moveSpeed = 2.8f;

        var emitter = go.AddComponent<BulletPatternEmitter>();
        emitter.pattern      = BulletPatternEmitter.PatternType.Circular;
        emitter.bulletCount  = 8;
        emitter.bulletSpeed  = 5.5f;
        emitter.fireInterval = 1.4f;

        Register(go);
    }

    // ─── 태그 자동 등록 ────────────────────────────────────────
    static void EnsureTag(string tagName)
    {
        // TagManager SerializedObject를 통해 태그가 없으면 추가
        var assetPath = "ProjectSettings/TagManager.asset";
        var tagManager = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        if (tagManager.Length == 0) return;

        SerializedObject so = new SerializedObject(tagManager[0]);
        SerializedProperty tags = so.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tagName) return; // 이미 존재

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tagName;
        so.ApplyModifiedProperties();
        Debug.Log($"[각자의 지옥] 태그 '{tagName}' 등록 완료");
    }

    // ─── 유틸 ──────────────────────────────────────────────────
    static void SetMaterialColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        // 인스턴스 머티리얼 생성 후 색상 설정
        rend.material.color = color;
    }

    static void Register(GameObject go)
    {
        Undo.RegisterCreatedObjectUndo(go, $"Create {go.name}");
    }
}
#endif
