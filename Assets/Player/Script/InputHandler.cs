using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// ������ ��� �Է��� �Ѱ��ϴ� �Ŵ��� Ŭ�����Դϴ�.
/// Unity�� New Input System ��ȣ�� �޾Ƽ�, C# �̺�Ʈ(Action)�� ��ȯ�Ͽ� �ٸ� ��ũ��Ʈ�� �����մϴ�.
/// </summary>
public class InputHandler : MonoBehaviour
{
    // ==================================================================================
    // 1. �̱��� (Singleton) ����
    // ==================================================================================
    // ���� ���� �� �ϳ��� �����ؾ� �ϸ�, ��𼭵�(Player, UI ��) ������ �� �־�� �մϴ�.
    public static InputHandler Instance { get; private set; }

    [Header("Input Settings")]
    // ����Ƽ �����Ϳ��� ���� .inputactions ����(�Ķ� ���� ������)�� ���⿡ �����մϴ�
    public InputActionAsset inputActions;


    // ==================================================================================
    // 2. �̺�Ʈ ���� (Events) - "��� ä��"
    // ==================================================================================
    // �ܺ� ��ũ��Ʈ(PlayerController ��)�� �� �̺�Ʈ���� ����(Subscribe, +=)�Ͽ� �Է��� �����մϴ�.
    public event Action<Vector2> OnMoveEvent;
    public event Action OnJumpEvent;
    public event Action OnPauseEvent;
    public event Action<bool> OnRunEvent;
    public event Action OnDashEvent;

    public event Action OnBasicAttackEvent;
    public event Action OnSkill1Event;
    public event Action OnSkill2Event;
    public event Action OnHealEvent;
    public event Action OnInteractEvent;
    public event Action OnSkillQEvent;

    // ==================================================================================
    // 3. ���� ���� (Internal Variables)
    // ==================================================================================
    // Input Action Asset���� ������ ���� �׼ǵ��� �����صδ� �����Դϴ�.
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    private InputAction runAction;
    private InputAction dashAction;

    private InputAction basicAttack;
    private InputAction skill_1;
    private InputAction skill_2;
    private InputAction heal;
    private InputAction interactAction;
    private InputAction skillQAction;

    // ==================================================================================
    // 4. �ʱ�ȭ (Awake)
    // ==================================================================================
    private void Awake()
    {
        // �̱��� �ʱ�ȭ: �� �ڽ��� ������ ���� ���, �̹� ������(�ߺ�) ���� �ı�.
        if (Instance == null) 
        { 
            Instance = this;
            // ���� �ٲ� �ı����� ���� (����, �Ŵ��� ���� �����Ǿ�� ��)
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
        if (inputActions == null)
            inputActions = Resources.Load<UnityEngine.InputSystem.InputActionAsset>("InputSystem_Actions");

        if (inputActions == null)
        {
            UnityEngine.Debug.LogError("InputHandler: Input Action Asset이 할당되지 않았습니다! Inspector에서 InputSystem_Actions를 연결하거나 Resources 폴더에 넣으세요.");
            return;
        }
        // "Player"��� �̸��� �׼� ��(Map)�� ã���ϴ�. (�����Ϳ��� ���� Map �̸��� ���ƾ� ��)
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            UnityEngine.Debug.LogError("InputHandler: 'Player' �׼� ���� ã�� �� �����ϴ�!");
            return;
        }

        // �����ϰ� ã�� (��� ������ ������ ����)
        moveAction = playerMap.FindAction("Move");
        jumpAction = playerMap.FindAction("Jump");
        pauseAction = playerMap.FindAction("Pause");
        runAction = playerMap.FindAction("Run");
        dashAction = playerMap.FindAction("Dash");

        basicAttack = playerMap.FindAction("BasicAttack");
        skill_1 = playerMap.FindAction("Skill_1");
        skill_2 = playerMap.FindAction("Skill_2");
        heal = playerMap.FindAction("Heal");
        interactAction = playerMap.FindAction("Interact");
        skillQAction = playerMap.FindAction("Action");

        LoadBindingOverrides();
    }

    private void OnEnable()
    {
        if (inputActions != null) inputActions.Enable();

        // null üũ �� ���� (�ϳ��� ��� �������� �۵���)
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
        if (dashAction != null) dashAction.performed += ctx => OnDashEvent?.Invoke();
        if (pauseAction != null) pauseAction.performed += ctx => { Debug.Log("[InputHandler] ESC 키 눌림 - Pause 이벤트 발생"); OnPauseEvent?.Invoke(); };

        if (basicAttack != null) basicAttack.performed += ctx => OnBasicAttackEvent?.Invoke();
        if (skill_1 != null) skill_1.performed += ctx => OnSkill1Event?.Invoke();
        if (skill_2 != null) skill_2.performed += ctx => OnSkill2Event?.Invoke();
        if (heal != null) heal.performed += ctx => OnHealEvent?.Invoke();
        if (interactAction != null) interactAction.performed += ctx => OnInteractEvent?.Invoke();
        if (skillQAction != null) skillQAction.performed += ctx => OnSkillQEvent?.Invoke();
    }

    // ==================================================================================
    // 6. ��Ȱ��ȭ (OnDisable)
    // ==================================================================================
    private void OnDisable()
    {
        if (inputActions != null) inputActions.Disable();
    }

    // ==================================================================================
    // 7. ���� �� �ҷ����� (Save & Load)
    // ==================================================================================
    // ����ڰ� �ٲ� Ű ����(Rebinding)�� JSON ���ڿ��� ��ȯ�� �����մϴ�.
    public void SaveBindingOverrides()
    {
        if (inputActions == null) return;

        // Rebinding result is kept in memory only. It reaches disk when the
        // settings save button calls SaveLoadManager.Instance.SaveSettings().
        SaveLoadManager.CurrentSettings.InputBindingsJson = inputActions.SaveBindingOverridesAsJson();
    }

    // ����� Ű ������ �ҷ��ͼ� �����մϴ�.
    public void LoadBindingOverrides()
    {
        if (inputActions == null) return;

        string json = SaveLoadManager.CurrentSettings.InputBindingsJson;
        if (string.IsNullOrEmpty(json)) return;

        inputActions.LoadBindingOverridesFromJson(json);
    }

    public InputAction GetAction(string actionName)
    {
        // 1. ���� ��ü�� ���� �� �� ���
        if (inputActions == null)
        {
            Debug.LogError("���� �˰�: InputHandler �ν����Ϳ� Input Action Asset�� ���� �� �ƽ��ϴ�!");
            return null;
        }

        // 2. �׼��� ã�ƺ��ϴ�.
        InputAction foundAction = inputActions.FindAction(actionName);

        // 3. �� ã���� ���
        if (foundAction == null)
        {
            Debug.LogError($"���� �˰�: '{actionName}'��� �׼��� ã�� �� �����ϴ�. ��Ÿ�� �ְų� 'Player/{actionName}' ó�� �� �̸��� ���� ��� �� ���� �ֽ��ϴ�.");

            // ������: �����ϴ� ��� �׼� �̸��� ����غ� (�ʿ��� �� �ּ� ����)
            
            foreach (var map in inputActions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    Debug.Log($"�߰ߵ� �׼�: {map.name}/{action.name}");
                }
            }
        }

        return foundAction;
    }
}