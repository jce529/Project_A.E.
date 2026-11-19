// Assets/Script/Combat/DamageInfo.cs
// 공격의 종류를 구분하여 보스의 피격/회복 로직에 사용합니다.
public enum DamageType
{
    Normal = 0,    // 기본 공격
    WaveSlash = 1, // 파동참 스킬
    Other = 2      // 그 외 (섬광참 등)
}

public struct DamageInfo
{
    public float amount;
    public DamageType type;
}
