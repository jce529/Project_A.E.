# 2D Fluid Water Shader — 디자이너 작업 가이드

> 셰이더 사용을 위해 디자이너가 준비해야 할 에셋 목록

---

## 요약

```
보스 몸체:   스프라이트 (패딩 포함) + 흑백 마스크  →  2장
바닥 웅덩이: 스프라이트                            →  1장
노이즈 텍스처: 에디터 도구로 자동 생성             →  0장
```

---

## Case 1 — 보스 몸체 (FluidWater_Character)

### 필요 에셋

| # | 에셋 | 형식 | 설명 |
|---|------|------|------|
| ① | **메인 스프라이트 텍스처** | PNG (RGBA) | 보스 몸체 스프라이트 |
| ② | **플로우 마스크 텍스처** | PNG 흑백 (R채널) | 왜곡 영역 지정 마스크 |

### ① 메인 스프라이트 텍스처 주의사항

- 몸체 외곽에 **투명 패딩 필수**
  - Distortion Strength가 클수록 UV가 스프라이트 경계 밖을 샘플링하기 때문
  - 패딩 없으면 가장자리가 늘어붙거나 잘리는 아티팩트 발생
- Unity Import 설정: Mesh Type → **`Full Rect`** 으로 변경

### ② 플로우 마스크 텍스처 주의사항

- **흰색 = 왜곡 O** / **검은색 = 고정**
- 단단하게 고정되어야 할 부위 (눈, 코어, 무기 등) → 검게 칠하기
- 흘러야 할 부위 (몸통, 촉수, 액체 부분 등) → 흰색으로 칠하기
- 메인 스프라이트와 **동일한 해상도** 권장
- 보스마다 개별 제작 필요

### 디자이너가 결정할 머티리얼 수치

| 항목 | 기본값 | 설명 |
|------|--------|------|
| Base Color (HDR) | 청록 (0.3, 0.8, 1.0, α0.9) | 틴트 컬러. HDR 값이 높을수록 Bloom 발광 강해짐 |
| Distortion Strength | 0.04 | 0.0 ~ 0.5 범위. 클수록 왜곡 심함 |
| Noise Tiling | (2, 2) | 노이즈 패턴 스케일 |
| Flow Speed | (0.08, 0.05) | 노이즈 흐르는 속도 (X, Y) |

---

## Case 2 — 바닥 웅덩이 (FluidWater_Puddle)

### 필요 에셋

| # | 에셋 | 형식 | 설명 |
|---|------|------|------|
| ① | **웅덩이 스프라이트 텍스처** | PNG (RGBA) | 웅덩이 형태 스프라이트 |
| ~~②~~ | ~~플로우 마스크~~ | 불필요 | 전체 왜곡이 기본값이므로 마스크 없어도 됨 |

### ① 웅덩이 스프라이트 텍스처 주의사항

- `Use World Space UV = ON` 상태이므로 카메라가 이동해도 텍스처가 세계에 고정됨
- MainTex Tiling X/Y로 월드 1유닛당 텍스처 반복 횟수 조절 가능

### 디자이너가 결정할 머티리얼 수치

| 항목 | 기본값 | 설명 |
|------|--------|------|
| Base Color | 진파랑 (0.1, 0.4, 0.9, α0.75) | 투명도(Alpha) 조절로 웅덩이 농도 표현 |
| Distortion Strength | 0.08 | Character보다 강하게 기본 설정됨 |
| Noise Tiling | (3, 3) | 노이즈 패턴 스케일 |
| Flow Speed | (0.05, 0.03) | 천천히 흐르는 웅덩이 표현 |

---

## 노이즈 텍스처 (디자이너 작업 불필요)

Unity 에디터 메뉴에서 자동 생성:
```
Tools → 2D Fluid Shader → Generate Noise Texture
```
생성 후 `Assets/Shaders/Custom/FluidNoiseTexture.png`에 저장됨.

---

## 전체 적용 순서

```
1. 노이즈 텍스처 생성  →  Tools → 2D Fluid Shader → Generate Noise Texture
2. 머티리얼 생성       →  Tools → 2D Fluid Shader → Create Materials
3. 스프라이트 오브젝트의 Sprite Renderer → Material 슬롯에 .mat 파일 드래그
4. (보스 전용) Flow Mask Texture 슬롯에 마스크 텍스처 교체
```

---

## 공통 주의사항

- Noise Texture의 Wrap Mode는 반드시 **Repeat** — 타일링이 끊기지 않아야 함
- HDR Base Color 발광 효과는 프로젝트에 **Bloom Post Processing**이 켜져 있어야 보임
