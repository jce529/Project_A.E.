# 물 셰이더 적용 가이드

> 셰이더: `Assets/Shaders/Custom/2D_Fluid_Water.shader`  
> 머티리얼: `FluidWater_Character.mat` (보스/캐릭터용), `FluidWater_Puddle.mat` (웅덩이/바닥용)

---

## 핵심 개념 한 줄 요약

| 모드 | 언제 쓰나 | Use World Space UV |
|------|----------|--------------------|
| **Character** | 보스 몸체, 물 몬스터 | OFF (스프라이트 기준) |
| **Puddle** | 바닥 웅덩이, 물웅덩이 | ON (월드 좌표 기준) |

셰이더는 하나지만 머티리얼 세팅만 바꿔서 두 용도로 재사용합니다.

---

## Case 1 — 보스/캐릭터 몸체에 적용 (UV 스크롤링)

### 적용 순서

**1. Sprite Renderer 선택**  
씬에서 보스 오브젝트 선택 → Inspector의 Sprite Renderer 컴포넌트 확인

**2. Material 슬롯에 드래그**  
`Assets/Shaders/Custom/FluidWater_Character.mat` 을  
Sprite Renderer의 `Material` 슬롯에 드래그

**3. Sprite Import 설정 변경** ← 반드시 해야 함  
Project 창에서 보스 스프라이트 PNG 선택 → Inspector  
- `Mesh Type` → `Full Rect` 으로 변경  
- Apply 클릭  
> 안 하면 왜곡 효과가 스프라이트 가장자리에서 잘립니다.

**4. Flow Mask Texture 연결**  
Material Inspector에서 `Flow Mask Texture` 슬롯에 흑백 마스크 텍스처 연결  
- 흰색 영역 = 일렁임 O  
- 검은색 영역 = 고정 (눈, 코어 등 단단한 부위)  
- 마스크가 없으면 흰색 기본값이 적용되어 전체가 일렁임

**5. 수치 조절**

| 프로퍼티 | 기본값 | 설명 |
|----------|--------|------|
| Flow Speed X | 0.08 | 가로 흐름 속도 |
| Flow Speed Y | 0.05 | 세로 흐름 속도 (음수면 위로 흐름) |
| Distortion Strength | 0.04 | 0.0 ~ 0.5, 클수록 왜곡 심함 |
| Base Color (HDR) | 청록 α0.9 | 틴트 컬러, HDR 높이면 Bloom 발광 |

---

## Case 2 — 바닥 웅덩이/물 오브젝트에 적용

### 적용 순서

**1. Sprite Renderer 선택**  
씬에서 웅덩이 오브젝트 선택

**2. Material 슬롯에 드래그**  
`Assets/Shaders/Custom/FluidWater_Puddle.mat` 을  
Sprite Renderer의 `Material` 슬롯에 드래그

**3. 수치 조절**

| 프로퍼티 | 기본값 | 설명 |
|----------|--------|------|
| Flow Speed X | 0.05 | 가로 흐름 속도 |
| Flow Speed Y | 0.03 | 세로 흐름 속도 |
| Distortion Strength | 0.08 | Character보다 강하게 기본 설정됨 |
| MainTex Tiling X/Y | (1, 1) | 월드 1유닛당 텍스처 반복 횟수 |
| Base Color | 진파랑 α0.75 | Alpha로 투명도 조절 |

> `Use World Space UV = ON` 상태이므로 카메라가 움직여도 텍스처가 세계에 고정됩니다.  
> 웅덩이가 카메라와 함께 밀려 보이는 현상이 없습니다.

---

## 새 오브젝트에 동일 효과 추가하는 법 (머티리얼 복사)

같은 설정을 다른 오브젝트에도 쓰고 싶을 때:

1. Project 창에서 `FluidWater_Character.mat` 우클릭 → **Duplicate**
2. 복사본 이름 변경 (예: `FluidWater_WoodBoss.mat`)
3. 복사본을 원하는 오브젝트의 Sprite Renderer Material 슬롯에 연결
4. 복사본의 수치만 따로 조절 → 원본에 영향 없음

---

## 노이즈 텍스처가 없을 때

셰이더가 작동하려면 노이즈 텍스처가 필요합니다.  
`Assets/Shaders/Custom/FluidNoiseTexture.png` 가 없다면:

```
Unity 상단 메뉴 → Tools → 2D Fluid Shader → Generate Noise Texture
```

실행하면 자동 생성됩니다. 머티리얼 생성도 같은 메뉴에서 가능합니다.

```
Tools → 2D Fluid Shader → Create Materials
```

---

## 자주 발생하는 문제

| 증상 | 원인 | 해결 |
|------|------|------|
| 가장자리가 잘리거나 늘어붙음 | Mesh Type이 Tight | 스프라이트 Import → Mesh Type을 `Full Rect`로 변경 |
| 왜곡이 전혀 안 보임 | Distortion Strength가 0 | 값을 0.03 이상으로 올리기 |
| 텍스처 이음새가 보임 | Noise Wrap Mode 문제 | FluidNoiseTexture.png의 Wrap Mode → `Repeat` 확인 |
| Bloom 발광이 안 보임 | Post Processing 꺼짐 | 카메라에 Bloom Post Processing 활성화 필요 |
| 웅덩이가 카메라와 함께 움직임 | WorldSpace UV 꺼짐 | Material Inspector → `Use World Space UV` 체크 |
