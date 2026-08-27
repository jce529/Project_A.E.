using UnityEngine;
using System.Collections.Generic;

// Phase 6 — Stage 2 메인 상태머신
// 사이클: 분신 생성 → 일반 패턴 N회 → 헤비콤보 (은신+돌진) → 그로기 + 분신 삭제 → (그로기 해제 시 재진입)
public class Stage2CombatState : SpiritCombatState
{
    // D-09a: 분신 SpiritController 리스트 (진짜 보스가 보유)
    private readonly List<SpiritController> _clones = new List<SpiritController>();

    // 사이클 카운터 (Discretion: N=3 회 일반 패턴 후 헤비콤보)
    private int _patternsExecuted = 0;
    private const int PatternsBeforeHeavyCombo = 3;

    // 헤비콤보 상태 추적 (Execute 가 중복 트리거하지 않도록)
    private bool _heavyComboInProgress = false;
    private float _heavyComboElapsed = 0f;
    // SpiritController.HeavyComboRoutine 의 총 소요시간 = StealthDuration + 0.1 + ChargeWindup + 2.0
    // 안전 버퍼 포함 추정 시간 (실제 종료는 _heavyComboInProgress 플래그로 추적)

    public override void Enter(BossController boss)
    {
        base.Enter(boss);

        if (!(boss is SpiritController spirit))
        {
            Debug.LogError("[Stage2CombatState] boss 가 SpiritController 가 아닙니다!");
            return;
        }

        // 분신은 Stage 2 사이클을 보유하지 않음 (분신 자신이 Stage2CombatState 진입 시도하지 않도록)
        if (spirit.IsDummy)
        {
            return;
        }

        SpawnClones(spirit);

        // 사이클 카운터 초기화
        _patternsExecuted = 0;
        _heavyComboInProgress = false;
        _heavyComboElapsed = 0f;
    }

    private void SpawnClones(SpiritController spirit)
    {
        if (spirit.DummyPrefab == null)
        {
            Debug.LogError("[Stage2CombatState] DummyPrefab 이 할당되지 않음 — 분신 스폰 실패");
            return;
        }

        // D-06: CombatSpawner.SpawnClone 사용
        // 스폰 위치: 보스 좌/우 ±2 유닛 (Discretion)
        Vector3[] offsets = new Vector3[] { new Vector3(-2f, 0f, 0f), new Vector3(2f, 0f, 0f) };

        for (int i = 0; i < 2; i++)
        {
            Vector3 spawnPos = spirit.transform.position + offsets[i];
            var cloneCtrl = CombatSpawner.SpawnClone(spirit.DummyPrefab, spawnPos, true);

            if (cloneCtrl != null)
            {
                _clones.Add(cloneCtrl);
            }
        }

    }

    public override void Execute(BossController boss)
    {
        // Bug 4 해결: 타이머 대신 실제 코루틴 진행 상태(IsHeavyComboInProgress)를 확인
        if (_heavyComboInProgress)
        {
            if (boss is SpiritController spiritBoss)
            {
                if (!spiritBoss.IsHeavyComboInProgress)
                {
                    OnHeavyComboFinished(boss);
                }
            }
            return;
        }

        // 일반 패턴 단계 — 부모(SpiritCombatState) 의 Execute 가 SelectAttackStrategy 호출 → 거리 기반 3종 패턴
        // 부모 Execute 가 _decisionTimer 를 관리하므로 한 사이클(공격 수행 + 쿨다운 종료) 후 다시 진입
        // 패턴 카운터는 SelectAttackStrategy 오버라이드에서 증가시킴
        base.Execute(boss);
    }

    // D-10a: SelectAttackStrategy 거리 분기는 부모(SpiritCombatState) 그대로 사용.
    //        다만 본 메서드가 호출될 때 패턴 카운터를 증가시키고, 임계 도달 시 헤비콤보로 전환.
    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        // 헤비콤보 임계 도달 → 부모 호출 대신 헤비콤보 트리거
        if (_patternsExecuted >= PatternsBeforeHeavyCombo)
        {
            TriggerHeavyComboCycle(boss);
            return null;  // 부모 Execute 의 _isAttacking=true 진입을 막음 (null 반환 시 다음 프레임 재시도)
        }

        // 일반 패턴 — 부모 SpiritCombatState 의 거리 기반 분기 그대로
        var strategy = base.SelectAttackStrategy(boss, dist);
        if (strategy != null)
        {
            _patternsExecuted++;
        }
        return strategy;
    }

    // D-09b/c: 헤비콤보 사이클 트리거 — 진짜+분신 모두에게 TriggerHeavyCombo 호출
    private void TriggerHeavyComboCycle(BossController boss)
    {
        if (_heavyComboInProgress) return;
        _heavyComboInProgress = true;
        _heavyComboElapsed = 0f;


        if (boss is SpiritController realBoss)
        {
            realBoss.TriggerHeavyCombo();
        }

        // null 분신(이미 파괴됨)은 건너뜀
        foreach (var clone in _clones)
        {
            if (clone != null)
            {
                clone.TriggerHeavyCombo();
            }
        }
    }

    // D-08a/b: 헤비콤보 종료 → 분신 전체 Destroy → GroggyState 전환
    private void OnHeavyComboFinished(BossController boss)
    {

        // 분신 전체 Destroy
        foreach (var clone in _clones)
        {
            if (clone != null && clone.gameObject != null)
            {
                Object.Destroy(clone.gameObject);
            }
        }
        _clones.Clear();

        _heavyComboInProgress = false;

        // 그로기 전환 — 5초 후 GroggyState 가 plain CombatState 로 전환 → SpiritController.Update 인터셉트가
        // IsStage2=true 이므로 Stage2CombatState 새 인스턴스로 교체 → Enter() 에서 분신 재스폰 (사이클 반복)
        boss.ChangeState(new GroggyState());
    }

    public override void Exit(BossController boss)
    {
        base.Exit(boss);
        
        // Bug 1 해결: 상태 종료 시 잔여 분신 정리
        if (boss is SpiritController spirit)
        {
            spirit.CleanupClones();
        }
    }
}
