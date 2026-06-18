using UnityEditor;
using UnityEngine;

// 메뉴: Tools > 2D Fluid Shader > Create Materials
// 2D_Fluid_Water 셰이더용 머티리얼 2종을 자동 생성합니다.
//   - FluidWater_Character : 보스 몸체 일렁임용 (로컬 UV)
//   - FluidWater_Puddle    : 바닥 물 웅덩이용 (월드 UV)
public static class FluidMaterialCreator
{
    private const string ShaderName  = "Custom/2D_Fluid_Water";
    private const string OutputDir   = "Assets/Shaders/Custom";
    private const string NoisePath   = "Assets/Shaders/Custom/FluidNoiseTexture.png";

    [MenuItem("Tools/2D Fluid Shader/Create Materials")]
    private static void CreateMaterials()
    {
        var shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            EditorUtility.DisplayDialog("셰이더 없음",
                $"셰이더 '{ShaderName}'를 찾을 수 없습니다.\n" +
                "Unity가 셰이더를 임포트할 때까지 잠시 기다린 뒤 다시 실행하세요.\n" +
                "(Project 창에서 2D_Fluid_Water.shader 파일이 보여야 합니다)", "확인");
            return;
        }

        var noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(NoisePath);
        if (noiseTex == null)
            Debug.LogWarning($"[FluidMaterial] 노이즈 텍스처를 찾을 수 없습니다: {NoisePath}\n" +
                             "Tools > 2D Fluid Shader > Generate Noise Texture 를 먼저 실행하세요.");

        CreateCharacterMaterial(shader, noiseTex);
        CreatePuddleMaterial(shader, noiseTex);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료",
            "머티리얼 2개가 생성되었습니다.\n\n" +
            $"• {OutputDir}/FluidWater_Character.mat\n" +
            $"  → 보스 몸체용 (로컬 UV, FlowMask 별도 설정 필요)\n\n" +
            $"• {OutputDir}/FluidWater_Puddle.mat\n" +
            $"  → 바닥 웅덩이용 (월드 UV, 전체 왜곡)",
            "확인");
    }

    // ── 캐릭터(보스 몸체)용 머티리얼 ──────────────────────────────────────────
    private static void CreateCharacterMaterial(Shader shader, Texture2D noise)
    {
        var mat = new Material(shader)
        {
            name = "FluidWater_Character"
        };

        // 로컬 UV 모드 (UseWorldSpace = OFF)
        mat.DisableKeyword("_USE_WORLD_SPACE");
        mat.SetFloat("_UseWorldSpace", 0f);

        // 색상: 반투명 청록 (보스 몸체 느낌, 디자이너가 이후 조절)
        mat.SetColor("_BaseColor", new Color(0.3f, 0.8f, 1.0f, 0.9f));

        // 노이즈 텍스처 + 느린 부유 흐름
        if (noise != null)
        {
            mat.SetTexture("_NoiseTex", noise);
            mat.SetTextureScale("_NoiseTex", new Vector2(2f, 2f));
        }
        mat.SetVector("_FlowSpeed", new Vector4(0.08f, 0.05f, 0f, 0f));
        mat.SetFloat("_DistortionStrength", 0.04f);

        // FlowMaskTex 기본값: 흰색(전체 왜곡) - 디자이너가 실제 마스크로 교체
        mat.SetTexture("_FlowMaskTex", Texture2D.whiteTexture);

        SaveMaterial(mat, $"{OutputDir}/FluidWater_Character.mat");
    }

    // ── 웅덩이(바닥 물)용 머티리얼 ────────────────────────────────────────────
    private static void CreatePuddleMaterial(Shader shader, Texture2D noise)
    {
        var mat = new Material(shader)
        {
            name = "FluidWater_Puddle"
        };

        // 월드 UV 모드 (UseWorldSpace = ON)
        mat.EnableKeyword("_USE_WORLD_SPACE");
        mat.SetFloat("_UseWorldSpace", 1f);

        // 색상: 짙은 파랑 반투명 (웅덩이 느낌)
        mat.SetColor("_BaseColor", new Color(0.1f, 0.4f, 0.9f, 0.75f));

        // 노이즈 텍스처 + 월드 기준 타일링
        if (noise != null)
        {
            mat.SetTexture("_NoiseTex", noise);
            mat.SetTextureScale("_NoiseTex", new Vector2(3f, 3f)); // 1월드유닛당 3회 반복
        }
        mat.SetVector("_FlowSpeed", new Vector4(0.05f, 0.03f, 0f, 0f));
        mat.SetFloat("_DistortionStrength", 0.08f);

        // MainTex 월드 스케일: 1유닛당 2회 반복
        mat.SetTextureScale("_MainTex", new Vector2(2f, 2f));

        // 웅덩이는 전체 왜곡이므로 마스크 = 흰색
        mat.SetTexture("_FlowMaskTex", Texture2D.whiteTexture);

        SaveMaterial(mat, $"{OutputDir}/FluidWater_Puddle.mat");
    }

    private static void SaveMaterial(Material mat, string path)
    {
        // 같은 경로에 이미 파일이 있으면 프로퍼티만 덮어씀 (GUID 유지)
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(mat, existing);
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(mat);
            Debug.Log($"[FluidMaterial] 기존 머티리얼 업데이트: {path}");
        }
        else
        {
            AssetDatabase.CreateAsset(mat, path);
            Debug.Log($"[FluidMaterial] 새 머티리얼 생성: {path}");
        }

        // Project 창에서 선택
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(path);
    }
}
