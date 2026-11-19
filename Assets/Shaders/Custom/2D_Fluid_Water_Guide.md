# 2D Fluid Water Shader — 작업 정리

> 환경: Unity 6 / URP 17.3.x / 2D Renderer  
> 작성일: 2026-06-04

---

## 개요

보스 몸체 일렁임 + 바닥 물 웅덩이를 **하나의 셰이더**로 처리하는 공용 2D 유체 셰이더 시스템.  
셰이더 1개, 에디터 유틸리티 스크립트 2개, 머티리얼 2개로 구성된다.

---

## 생성 파일 목록

| 파일 | 경로 | 설명 |
|------|------|------|
| `2D_Fluid_Water.shader` | `Assets/Shaders/Custom/` | 메인 셰이더 |
| `FluidNoiseTexture.png` | `Assets/Shaders/Custom/` | 생성된 노이즈 텍스처 |
| `FluidWater_Character.mat` | `Assets/Shaders/Custom/` | 보스 몸체용 머티리얼 |
| `FluidWater_Puddle.mat` | `Assets/Shaders/Custom/` | 바닥 웅덩이용 머티리얼 |
| `FluidNoiseTextureGenerator.cs` | `Assets/Editor/Tools/` | 노이즈 텍스처 생성 스크립트 |
| `FluidMaterialCreator.cs` | `Assets/Editor/Tools/` | 머티리얼 생성 스크립트 |

---

## 셰이더 인스펙터 프로퍼티

### [Base]
| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| `Main Texture` | Texture2D | 기본 스프라이트 텍스처 |
| `Flow Mask Texture` | Texture2D | 흑백 마스크 (흰색=왜곡, 검은색=고정) |
| `Base Color` | Color (HDR) | 틴트 컬러. HDR 값으로 Bloom 발광 가능 |

### [Flow & Distortion]
| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| `Noise Texture` | Texture2D | 심리스 그레이스케일 노이즈 |
| `Flow Speed (X, Y)` | Vector2 | 노이즈 스크롤 속도 |
| `Distortion Strength` | Float (0.0 ~ 0.5) | 왜곡 강도 |

### [System]
| 프로퍼티 | 타입 | 설명 |
|----------|------|------|
| `Use World Space UV` | Boolean | ON=월드 좌표 UV(웅덩이), OFF=로컬 UV(캐릭터) |

---

## 셰이더 핵심 로직

```
[버텍스]
  localUV (메시 UV 0~1)
  worldXY (월드 좌표 XY)

[프래그먼트]
  ① UseWorldSpace? → baseUV = worldXY  or  localUV
  ② noiseUV = TRANSFORM_TEX(baseUV, NoiseTex) + Time × FlowSpeed
     noiseX  = NoiseTex.r(noiseUV)
     noiseY  = NoiseTex.r(noiseUV + 0.5)   ← X/Y 독립 샘플링으로 대각 쏠림 방지
  ③ distortionVec = (noiseX - 0.5,  noiseY - 0.5)   // ±0.5 양방향
  ④ maskVal  = FlowMaskTex.r(localUV)               // 항상 로컬 UV
  ⑤ finalUV  = TRANSFORM_TEX(baseUV, MainTex) + distortionVec × Strength × maskVal
  ⑥ output   = MainTex(finalUV) × BaseColor × VertexColor
```

> **원본 스펙과의 차이점**  
> 스펙은 노이즈 X/Y에 동일한 값을 사용했으나, 동일 값 사용 시 왜곡이 대각선으로만 쏠리는 문제가 있어  
> Y축은 `noiseUV + 0.5` 위치에서 별도 샘플링하도록 변경함.

---

## 노이즈 텍스처 생성

### 실행 방법
```
Tools → 2D Fluid Shader → Generate Noise Texture
```

### 생성 스펙
| 항목 | 값 | 이유 |
|------|----|------|
| 해상도 | 256 × 256 | 노이즈는 고해상도 불필요 |
| 포맷 | R8 그레이스케일 | R채널만 사용 |
| Wrap Mode | Repeat | 타일링 필수 |
| sRGB | Off | 수치 데이터이므로 선형 공간 |
| Mipmap | Off | 2D 셰이더에서 불필요 |
| Compression | Uncompressed | R채널 정밀도 손실 방지 |

### 심리스 알고리즘
```
2D UV (u, v)
    → 4D 토러스 좌표 (cos·u, cos·v, sin·u, sin·v) × scale
    → 2회 PerlinNoise 샘플링 후 평균
    → 옥타브(4회) 합산 → 정규화 (0~1)
```
텍스처 가장자리가 수학적으로 반대쪽과 완벽히 이어지는 심리스 노이즈.

### 파라미터 조절 (코드 상단 수정)
```csharp
const float scale       = 4f;   // 클수록 패턴 촘촘
const int   octaves     = 4;    // 많을수록 디테일 풍부 (성능 비용↑)
const float persistence = 0.5f; // 클수록 고주파 성분 강함 (거칠어짐)
```

---

## 머티리얼 생성

### 실행 방법
```
① Tools → 2D Fluid Shader → Generate Noise Texture   (노이즈 텍스처 먼저 생성)
② Tools → 2D Fluid Shader → Create Materials
```

### 머티리얼 기본값

| 프로퍼티 | FluidWater_Character | FluidWater_Puddle |
|---|---|---|
| Use World Space | OFF | ON |
| Base Color | 청록 반투명 (0.3, 0.8, 1.0, α0.9) | 진파랑 반투명 (0.1, 0.4, 0.9, α0.75) |
| Noise Tiling | (2, 2) | (3, 3) |
| Flow Speed | (0.08, 0.05) | (0.05, 0.03) |
| Distortion Strength | 0.04 | 0.08 |
| Flow Mask | 흰색 기본 → 실제 마스크로 교체 필요 | 흰색 (전체 왜곡) |

---

## 스프라이트 적용 방법

1. 스프라이트 오브젝트의 **Sprite Renderer** 선택
2. `Material` 슬롯에 `.mat` 파일 드래그
3. (Character 전용) `Flow Mask Texture` 슬롯에 흑백 마스크 텍스처 교체

---

## 주의사항

### 공통
- `NoiseTexture`는 Wrap Mode가 **Repeat**이어야 타일링이 끊기지 않음
- HDR `BaseColor` 발광 효과는 프로젝트에 **Bloom Post Processing**이 활성화된 상태에서만 표시됨
- SRP Batcher 호환: `CBUFFER_START(UnityPerMaterial)` 사용으로 자동 활성화됨

### Character (보스 몸체) 전용
- **스프라이트 여백 필수**: `DistortionStrength`가 클수록 UV가 스프라이트 경계 밖을 샘플링함  
  → 스프라이트 원본 이미지에 투명 패딩 확보, Mesh Type을 `Full Rect`로 설정
- `FlowMaskTex` 기본값이 흰색(전체 왜곡)이므로, 고정 부위가 있다면 반드시 마스크 텍스처 교체

### Puddle (바닥 웅덩이) 전용
- `MainTex` Tiling X/Y로 월드 1유닛당 텍스처 반복 횟수 조절
- `NoiseTex` Tiling으로 노이즈 패턴 스케일 독립 조절 가능
- 카메라가 이동해도 텍스처가 세계에 고정되어 자연스러운 웅덩이 표현 가능
