using UnityEngine;
using UnityEditor;

public class WaterSpiritGenerator : EditorWindow
{
    private string bossName = "New Spirit Boss";
    private float initialHP = 100f;
    private GameObject playerRef;

    [MenuItem("Tools/Spirit Boss/Generate Water Spirit Boss")]
    public static void ShowWindow()
    {
        GetWindow<WaterSpiritGenerator>("Spirit Boss Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Water Spirit Boss Generator", EditorStyles.boldLabel);
        bossName = EditorGUILayout.TextField("Boss Name", bossName);
        initialHP = EditorGUILayout.FloatField("Initial HP", initialHP);
        
        playerRef = (GameObject)EditorGUILayout.ObjectField("Player Reference (Optional)", playerRef, typeof(GameObject), true);

        if (GUILayout.Button("Generate Boss"))
        {
            GenerateBoss();
        }
    }

    private void GenerateBoss()
    {
        // 1. 플레이어 찾기 및 크기 결정
        Vector3 targetScale = Vector3.one;
        if (playerRef == null)
        {
            playerRef = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerRef != null)
        {
            targetScale = playerRef.transform.localScale;
            Debug.Log($"[Generator] Player found. Using scale: {targetScale}");
        }
        else
        {
            Debug.LogWarning("[Generator] Player not found in scene. Using default scale (1,1,1).");
        }

        // 2. 보스 생성
        GameObject bossGO = new GameObject(bossName);
        Undo.RegisterCreatedObjectUndo(bossGO, "Create Spirit Boss");

        // 3. 컴포넌트 추가
        SpriteRenderer sr = bossGO.AddComponent<SpriteRenderer>();
        // 임시 색상 설정 (시각적 확인용)
        sr.color = new Color(0.2f, 0.5f, 1f, 1f); 

        Rigidbody2D rb = bossGO.GetComponent<Rigidbody2D>(); // SpiritController가 Require함
        if (rb == null) rb = bossGO.AddComponent<Rigidbody2D>();
        rb.linearDamping = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CapsuleCollider2D col = bossGO.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(1f, 2f); // 기본값

        Animator anim = bossGO.AddComponent<Animator>();

        SpiritStats stats = bossGO.AddComponent<SpiritStats>();
        stats.MaxHealth = initialHP;
        // stats.Reset() 은 private일 수 있으므로 인스펙터 설정을 따름

        SpiritController ctrl = bossGO.AddComponent<SpiritController>();
        
        // 4. 레퍼런스 연결
        ctrl.Stats = stats;
        ctrl.Anim = anim;
        if (playerRef != null)
        {
            ctrl.Target = playerRef.transform;
        }

        // 5. 스케일 설정
        bossGO.transform.localScale = targetScale;

        // 6. 레이어 설정 (Enemy 레이어가 있는지 확인 후 설정)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer != -1)
        {
            bossGO.layer = enemyLayer;
        }

        Selection.activeGameObject = bossGO;
        Debug.Log($"[Generator] '{bossName}' has been generated successfully at scale {targetScale}.");
    }
}
