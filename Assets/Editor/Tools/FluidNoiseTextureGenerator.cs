using UnityEditor;
using UnityEngine;

// 메뉴: Tools > 2D Fluid Shader > Generate Noise Texture
// 2D_Fluid_Water 셰이더용 심리스(이음매 없는) 펄린 노이즈 텍스처를 생성합니다.
public static class FluidNoiseTextureGenerator
{
    private const string OutputPath = "Assets/Shaders/Custom/FluidNoiseTexture.png";

    [MenuItem("Tools/2D Fluid Shader/Generate Noise Texture")]
    private static void GenerateNoiseTexture()
    {
        // 파라미터 창 대신 고정값 사용 (디자이너가 이 스크립트를 수정해 조절 가능)
        const int   resolution = 256; // 256x256 픽셀
        const float scale      = 4f;  // 펄린 노이즈 타일 반복 수 (클수록 패턴이 촘촘)
        const int   octaves    = 4;   // 중첩 레이어 수 (많을수록 디테일 증가, 성능 비용↑)
        const float persistence = 0.5f; // 고주파 성분 감쇠율

        var texture = new Texture2D(resolution, resolution, TextureFormat.R8, mipChain: false);
        var pixels  = new Color[resolution * resolution];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float u = (float)x / resolution;
                float v = (float)y / resolution;

                // 심리스 노이즈: 2D UV를 4D 토러스(원환면) 좌표로 변환
                // 이렇게 하면 텍스처 가장자리가 반대쪽과 완벽하게 이어짐
                float nx = Mathf.Cos(u * Mathf.PI * 2f) * scale / (Mathf.PI * 2f);
                float ny = Mathf.Cos(v * Mathf.PI * 2f) * scale / (Mathf.PI * 2f);
                float nz = Mathf.Sin(u * Mathf.PI * 2f) * scale / (Mathf.PI * 2f);
                float nw = Mathf.Sin(v * Mathf.PI * 2f) * scale / (Mathf.PI * 2f);

                float noiseValue  = 0f;
                float amplitude   = 1f;
                float maxAmplitude = 0f;
                float frequency   = 1f;

                // 옥타브 합산: 저주파(큰 형태) + 고주파(세부 디테일)
                for (int o = 0; o < octaves; o++)
                {
                    // Unity의 PerlinNoise는 2D이므로 4D 좌표를 2회 샘플링해 합산
                    float s1 = Mathf.PerlinNoise(nx * frequency, ny * frequency);
                    float s2 = Mathf.PerlinNoise(nz * frequency, nw * frequency);
                    noiseValue    += ((s1 + s2) * 0.5f) * amplitude;
                    maxAmplitude  += amplitude;
                    amplitude     *= persistence;
                    frequency     *= 2f;
                }

                noiseValue /= maxAmplitude; // 0~1 범위로 정규화
                pixels[y * resolution + x] = new Color(noiseValue, noiseValue, noiseValue, 1f);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // PNG로 저장
        byte[] png = texture.EncodeToPNG();
        Object.DestroyImmediate(texture);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(
            System.IO.Path.Combine(Application.dataPath, "../", OutputPath)));
        System.IO.File.WriteAllBytes(
            System.IO.Path.Combine(Application.dataPath, "../", OutputPath), png);

        AssetDatabase.Refresh();

        // Import 세팅 자동 적용 (셰이더에 최적화된 설정)
        ApplyImportSettings(OutputPath);

        Debug.Log($"[FluidNoiseGenerator] 노이즈 텍스처 생성 완료: {OutputPath}");
        EditorUtility.DisplayDialog(
            "생성 완료",
            $"심리스 펄린 노이즈 텍스처가 생성되었습니다.\n경로: {OutputPath}\n\n" +
            "머티리얼의 Noise Texture 슬롯에 드래그하여 사용하세요.",
            "확인");

        // Project 창에서 생성된 파일을 자동 선택
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
    }

    private static void ApplyImportSettings(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType         = TextureImporterType.Default;
        importer.textureShape        = TextureImporterShape.Texture2D;
        importer.sRGBTexture         = false;     // 노이즈는 선형(Linear) 데이터
        importer.alphaSource         = TextureImporterAlphaSource.None;
        importer.wrapMode            = TextureWrapMode.Repeat;   // 심리스 타일링 필수
        importer.filterMode          = FilterMode.Bilinear;
        importer.mipmapEnabled       = false;     // 2D 셰이더 노이즈는 밉맵 불필요
        importer.maxTextureSize      = 256;
        importer.textureCompression  = TextureImporterCompression.Uncompressed; // R채널 정밀도 유지

        importer.SaveAndReimport();
    }

    // 미리보기: Scene 뷰에 노이즈 값 분포 확인용 로그
    [MenuItem("Tools/2D Fluid Shader/Check Existing Noise Asset")]
    private static void CheckNoiseAsset()
    {
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(OutputPath);
        if (tex == null)
        {
            EditorUtility.DisplayDialog("없음",
                $"노이즈 텍스처가 없습니다.\n먼저 'Generate Noise Texture'를 실행하세요.", "확인");
            return;
        }

        Selection.activeObject = tex;
        Debug.Log($"[FluidNoiseGenerator] 텍스처 정보: {tex.width}x{tex.height}, " +
                  $"Format={tex.format}, WrapMode={tex.wrapMode}, sRGB={tex.isDataSRGB}");
    }
}
