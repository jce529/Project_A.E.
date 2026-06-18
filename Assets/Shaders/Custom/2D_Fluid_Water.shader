// =============================================================================
// 2D_Fluid_Water.shader
// 보스 몸체 일렁임 + 바닥 물 웅덩이를 하나의 셰이더로 처리하는 공용 유체 셰이더
// 대상 환경: Unity 6 URP 17.x / 2D Renderer (Universal2D)
//
// [디자이너 설정 가이드]
// ▶ 캐릭터 (보스 몸체) 사용 시:
//   - Use World Space UV = OFF
//   - FlowMaskTex: 일렁일 부위만 흰색, 고정될 부위는 검은색인 흑백 마스크
//   - 스프라이트에 Mesh Type = "Full Rect" 권장 (여백이 있어야 왜곡이 잘림 없이 출력됨)
//
// ▶ 바닥 웅덩이 사용 시:
//   - Use World Space UV = ON
//   - MainTex의 Tiling X/Y: 월드 1유닛 당 텍스처 반복 횟수 조절 (클수록 촘촘)
//   - NoiseTex의 Tiling X/Y: 노이즈 패턴 스케일 조절
//   - FlowMaskTex: 흰색(white) 단색 기본 텍스처 사용 시 전체 왜곡 적용
//
// [주의사항]
// - NoiseTexture는 Seamless(이음매 없는) 그레이스케일 텍스처 권장 (Wrap Mode: Repeat)
// - DistortionStrength가 0.1 이상이면 스프라이트 테두리가 잘릴 수 있음
//   → Sprite의 Pixels Per Unit을 낮추거나 스프라이트에 충분한 여백(패딩)을 확보하세요
// - HDR BaseColor 사용 시 프로젝트에 Bloom 포스트 프로세싱이 활성화되어 있어야 발광 효과 표시됨
// =============================================================================

Shader "Custom/2D_Fluid_Water"
{
    Properties
    {
        // ── [Base] ──────────────────────────────────────────────────────────
        [MainTexture]
        _MainTex        ("Main Texture",      2D)    = "white" {}
        _FlowMaskTex    ("Flow Mask Texture", 2D)    = "white" {}
        [HDR][MainColor]
        _BaseColor      ("Base Color",        Color) = (1, 1, 1, 1)

        // ── [Flow & Distortion] ─────────────────────────────────────────────
        _NoiseTex           ("Noise Texture",        2D)            = "gray"  {}
        _FlowSpeed          ("Flow Speed (X, Y)",    Vector)         = (0.1, 0.05, 0, 0)
        _DistortionStrength ("Distortion Strength",  Range(0.0, 0.5)) = 0.05

        // ── [System] ────────────────────────────────────────────────────────
        // ON  = 월드 좌표 기반 UV (웅덩이용: 카메라가 이동해도 텍스처가 고정됨)
        // OFF = 로컬 스프라이트 UV (캐릭터용: 오브젝트와 함께 움직임)
        [Toggle(_USE_WORLD_SPACE)]
        _UseWorldSpace  ("Use World Space UV", Float) = 0

        // ── 내부 (Sprite Renderer 버텍스 컬러 지원) ──────────────────────────
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull  Off
        ZWrite Off

        Pass
        {
            Name "2D_Fluid_Water"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature_local _USE_WORLD_SPACE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── 텍스처 선언 ─────────────────────────────────────────────────
            TEXTURE2D(_MainTex);     SAMPLER(sampler_MainTex);
            TEXTURE2D(_FlowMaskTex); SAMPLER(sampler_FlowMaskTex);
            TEXTURE2D(_NoiseTex);    SAMPLER(sampler_NoiseTex);

            // ── 유니폼 버퍼 (SRP Batcher 호환 필수) ─────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _FlowMaskTex_ST;
                float4 _NoiseTex_ST;
                float4 _BaseColor;
                float4 _RendererColor;
                float2 _FlowSpeed;
                float  _DistortionStrength;
            CBUFFER_END

            // ── 버텍스 입력 ─────────────────────────────────────────────────
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // ── 프래그먼트 입력 ─────────────────────────────────────────────
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 localUV    : TEXCOORD0;  // 원본 메시 UV (0~1, ST 미적용)
                float2 worldXY    : TEXCOORD1;  // 월드 XY 좌표 (웅덩이용)
                float4 color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ── 버텍스 셰이더 ───────────────────────────────────────────────
            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.localUV    = IN.uv;
                OUT.worldXY    = TransformObjectToWorld(IN.positionOS.xyz).xy;
                OUT.color      = IN.color;
                return OUT;
            }

            // ── 프래그먼트 셰이더 ───────────────────────────────────────────
            half4 frag(Varyings IN) : SV_Target
            {
                // ── [1] UV 설정 ─────────────────────────────────────────────
                // UseWorldSpace ON  : 월드 좌표 → 텍스처가 세계에 고정 (웅덩이)
                //                     MainTex Tiling으로 월드 1유닛당 반복 스케일 설정
                // UseWorldSpace OFF : 스프라이트 로컬 UV (캐릭터 몸체)
                #if defined(_USE_WORLD_SPACE)
                    float2 baseUV = IN.worldXY;
                #else
                    float2 baseUV = IN.localUV;
                #endif

                // ── [2] 노이즈 스크롤 ────────────────────────────────────────
                // NoiseTex_ST(Tiling/Offset)로 노이즈 패턴 크기 조절
                // FlowSpeed로 X/Y 방향 흐름 속도 독립 제어
                float2 noiseUV = TRANSFORM_TEX(baseUV, _NoiseTex) + _Time.y * _FlowSpeed;

                // X축 노이즈: 정방향 샘플링
                // Y축 노이즈: UV를 반 오프셋해서 X와 독립적으로 → 대각 방향 쏠림 방지
                float noiseX = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                float noiseY = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV + 0.5).r;

                // ── [3] 양방향 왜곡 벡터 생성 (-0.5 ~ +0.5) ─────────────────
                float2 distortionVec = float2(noiseX - 0.5, noiseY - 0.5);

                // ── [4] 흐름 마스크 적용 ─────────────────────────────────────
                // 마스크는 항상 로컬 UV 기준 (월드 모드에서도 스프라이트 형태 유지)
                float2 maskUV  = TRANSFORM_TEX(IN.localUV, _FlowMaskTex);
                float  maskVal = SAMPLE_TEXTURE2D(_FlowMaskTex, sampler_FlowMaskTex, maskUV).r;

                // ── [5] 최종 UV 계산 ─────────────────────────────────────────
                // finalDistortion = 왜곡벡터 × 강도 × 마스크
                // finalUV = MainTex 공간의 baseUV + 왜곡값
                float2 finalDistortion = distortionVec * _DistortionStrength * maskVal;
                float2 finalUV         = TRANSFORM_TEX(baseUV, _MainTex) + finalDistortion;

                // ── [6] 출력 ─────────────────────────────────────────────────
                // MainTex × BaseColor(HDR) × SpriteRenderer 버텍스 컬러
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
                col *= _BaseColor * IN.color * _RendererColor;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/2D/Sprite-Unlit-Default"
}
