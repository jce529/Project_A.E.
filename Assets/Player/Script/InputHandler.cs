using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 게임의 모든 입력을 총괄하는 매니저 클래스입니다.
/// Unity의 New Input System 신호를 받아서, C# 이벤트(Action)로 변환하여 다른 스크립트에 전달합니다.
/// </summary>
public class InputHandler : MonoBehaviour
{
    // ==================================================================================
    // 1. 싱글톤 (Singleton) 패턴
    // ==================================================================================
    // 게임 내에 단 하나만 존재해야 하며, 어디서든(Player, UI 등) 접근할 수 있어야 합니다.
    public static InputHandler Instance { get; private set; }

    [Header("Input Settings")]
    // 유니티 에디터에서 만든 .inputactions 파일(파란 번개 아이콘)을 여기에 연결합니다
    public InputActionAsset inputActions;


    // ==================================================================================
    // 2. 이벤트 정의 (Events) - "방송 채널"
    // ==================================================================================
    // 외부 스크립트(PlayerController 등)는 이 이벤트들을 구독(Subscribe, +=)하여 입력을 감지합니다.
    public event Action<Vector2> OnMoveEvent;
    public event Action OnJumpEvent;
    public event Action OnPauseEvent;
    public event Action<bool> OnRunEvent;

    public event Action OnBasicAttackEvent;
    public event Action OnSkill1Event;
    public event Action OnSkill2Event;
    public event Action OnHealEvent;

    // ==================================================================================
    // 3. 내부 변수 (Internal Variables)
    // ==================================================================================
    // Input Action Asset에서 가져온 개별 액션들을 저장해두는 변수입니다.
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    private InputAction runAction;

    private InputAction basicAttack;
    private InputAction skill_1;
    private InputAction skill_2;
    private InputAction heal;

    // 키 바인딩 저장을 위한 PlayerPrefs 키 이름
    private const string SAVE_KEY = "InputBindings";


    // ==================================================================================
    // 4. 초기화 (Awake)
    // ==================================================================================
    private void Awake()
    {
        // 싱글톤 초기화: 나 자신이 없으면 나를 등록, 이미 있으면(중복) 나를 파괴.
        if (Instance == null) 
        { 
            Instance = this;
            // 씬이 바뀌어도 파괴되지 않음 (사운드, 매니저 등은 유지되어야 함)
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
        // Inspector에 에셋이 연결되지 않았을 경우 에러 방지
        if (inputActions == null)
        {
            UnityEngine.Debug.LogError("InputHandler: Input Action Asset이 연결되지 않았습니다!");
            return;
        }
        // "Player"라는 이름의 액션 맵(Map)을 찾습니다. (에디터에서 만든 Map 이름과 같아야 함)
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            UnityEngine.Debug.LogError("InputHandler: 'Player' 액션 맵을 찾을 수 없습니다!");
            return;
        }

        // 안전하게 찾기 (없어도 게임이 멈추지 않음)
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        pauseAction = playerMap.FindAction("Pause");
        runAction = playerMap.FindAction("Run");

        basicAttack = playerMap.FindAction("BasicAttack");
        skill_1 = playerMap.FindAction("Skill_1");
        skill_2 = playerMap.FindAction("Skill_2");
        heal = playerMap.FindAction("Heal");

        LoadBindingOverrides();
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();

        // null 체크 후 연결 (하나가 없어도 나머지는 작동함)
        if (moveAction != null)
        {
            moveAction.performed += ctx => OnMoveEvent?.Invoke(ctx.ReadValue<Vector2>());
            moveAction.canceled += ctx => OnMoveEvent?.Invoke(Vector2.zero);
        }
        if (jumpAction != null) jumpAction.performed += ctx => OnJumpEvent?.Invoke();
        if (runAction != null)
        {
            runAction.performed += ctx => OnRunEvent?.Invoke(true);
            runAction.canceled += ctx => OnRunEvent?.Invoke(false);
        }
        if (pauseAction != null) pauseAction.performed += ctx => OnPauseEvent?.Invoke();

        if (basicAttack != null) basicAttack.performed += ctx => OnBasicAttackEvent?.Invoke();
        if (skill_1 != null) skill_1.performed += ctx => OnSkill1Event?.Invoke();
        if (skill_2 != null) skill_2.performed += ctx => OnSkill2Event?.Invoke();
        if (heal != null) heal.performed += ctx => OnHealEvent?.Invoke();
    }

    // ==================================================================================
    // 6. 비활성화 (OnDisable)
    // ==================================================================================
    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
    }

    // ==================================================================================
    // 7. 저장 및 불러오기 (Save & Load)
    // ==================================================================================
    // 사용자가 바꾼 키 설정(Rebinding)을 JSON 문자열로 변환해 저장합니다.
    public void SaveBindingOverrides()
    {
        if (inputActions == null) return;

        // 바인딩 정보를 JSON 텍스트로 추출
        string json = inputActions.SaveBindingOverridesAsJson();

        // PlayerPrefs(간단한 저장소)에 저장
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    // 저장된 키 설정을 불러와서 적용합니다.
    public void LoadBindingOverrides()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            // JSON 텍스트를 다시 바인딩 정보로 변환하여 덮어씌움(Override)
            inputActions.LoadBindingOverridesFromJson(json);
        }
    }

    public InputAction GetAction(string actionName)
    {
        // 1. 에셋 자체가 연결 안 된 경우
        if (inputActions == null)
        {
            Debug.LogError("범인 검거: InputHandler 인스펙터에 Input Action Asset이 연결 안 됐습니다!");
            return null;
        }

        // 2. 액션을 찾아봅니다.
        InputAction foundAction = inputActions.FindAction(actionName);

        // 3. 못 찾았을 경우
        if (foundAction == null)
        {
            Debug.LogError($"범인 검거: '{actionName}'라는 액션을 찾을 수 없습니다. 오타가 있거나 'Player/{actionName}' 처럼 맵 이름을 같이 써야 할 수도 있습니다.");

            // 참고용: 존재하는 모든 액션 이름을 출력해봄 (필요할 때 주석 해제)
            
            foreach (var map in inputActions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    Debug.Log($"발견된 액션: {map.name}/{action.name}");
                }
            
        }
    }

        return foundAction;
    }
}