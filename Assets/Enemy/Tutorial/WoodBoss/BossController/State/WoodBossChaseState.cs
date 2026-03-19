using UnityEngine;

namespace WoodBoss
{
    public class WoodBossChaseState : IBossState
    {
        // 1초당 1발자국 설정을 위한 타이머 변수들
        private float _timer;
        private float _stepCycle = 1.0f;   // 1걸음의 전체 주기 (1초)
        private float _moveDuration = 0.3f; // 실제로 움직이는 시간 (0.3초 동안 쿵! 하고 이동)
        // 나머지 0.7초는 대기(후딜레이)

        private float _moveSpeed = 3.0f;   // 움직일 때의 속도 (짧게 움직이니 조금 빨라야 위협적)

        public void Enter(BossController boss)
        {
            _timer = 0f;
            // 처음 상태 진입 시 바로 움직이게 할지, 대기할지 결정 (여기선 바로 이동 시작)
        }

        public void Execute(BossController boss)
        {
            // 1. 타겟을 놓치면 대기 상태로 복귀
            if (!boss.TargetFound)
            {
                boss.ChangeState(new IdleState());
                return;
            }

            // 2. 공격 범위 안에 들어오면 -> 공격 상태로 전환
            float dist = Vector2.Distance(boss.transform.position, boss.Target.position);
            if (dist <= boss.AttackRange)
            {
                boss.StopMove(); // 공격 전 정지
                boss.ChangeState(new WoodBossAttackState());
                return;
            }

            // 3. 묵직한 이동 로직 (쿵- 멈춤- 쿵- 멈춤)
            ProcessHeavyStep(boss);
        }

        public void Exit(BossController boss)
        {
            boss.StopMove();
        }

        private void ProcessHeavyStep(BossController boss)
        {
            _timer += Time.deltaTime;

            // 사이클(1초)이 지나면 타이머 초기화
            if (_timer >= _stepCycle)
            {
                _timer = 0f;
            }

            // 움직이는 시간(0 ~ 0.3초) 동안만 이동
            if (_timer < _moveDuration)
            {
                // 플레이어 방향 계산
                Vector2 dir = (boss.Target.position - boss.transform.position).normalized;

                // 보스 이동 (Transform 직접 이동 방식, Rigidbody 사용 시 velocity 수정 필요)
                boss.transform.Translate(dir * _moveSpeed * Time.deltaTime);

                if (dir.x != 0) // 움직일 때만 방향 전환
                {
                    // 현재 크기의 절댓값 (예: 크기가 3이면 3, -3이면 3을 가져옴)
                    float sizeX = Mathf.Abs(boss.transform.localScale.x);
                    float sizeY = boss.transform.localScale.y;
                    float sizeZ = boss.transform.localScale.z;

                    if (dir.x > 0)
                    {
                        // 오른쪽 보기 (양수)
                        boss.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
                    }
                    else if (dir.x < 0)
                    {
                        // 왼쪽 보기 (음수)
                        boss.transform.localScale = new Vector3(-sizeX, sizeY, sizeZ);
                    }
                }
                // (팁) 여기에 "쿵!" 하는 발소리나 카메라 흔들림을 넣으면 더 묵직해집니다.
            }
            else
            {
                // 나머지 시간(0.3 ~ 1.0초)은 정지 (무게감을 위해)
                boss.StopMove();
            }
        }
    }
}