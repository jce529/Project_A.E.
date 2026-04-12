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

    public event Action OnBasicAttackEvent;
    public event Action OnSkill1Event;
    public event Action OnSkill2Event;
    public event Action OnHealEvent;
    public event Action OnInteractEvent;

    // ==================================================================================
    // 3. ���� ���� (Internal Variables)
    // ==================================================================================
    // Input Action Asset���� ������ ���� �׼ǵ��� �����صδ� �����Դϴ�.
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction pauseAction;
    private InputAction runAction;

    private InputAction basicAttack;
    private InputAction skill_1;
    private InputAction skill_2;
    private InputAction heal;
    private InputAction interactAction;

    // Ű ���ε� ������ ���� PlayerPrefs Ű �̸�
    private const string SAVE_KEY = "InputBindings";


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
        // Inspector�� ������ ������� �ʾ��� ��� ���� ����
        if (inputActions == null)
        {
            UnityEngine.Debug.LogError("InputHandler: Input Action Asset�� ������� �ʾҽ��ϴ�!");
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

        basicAttack = playerMap.FindAction("BasicAttack");
        skill_1 = playerMap.FindAction("Skill_1");
        skill_2 = playerMap.FindAction("Skill_2");
        heal = playerMap.FindAction("Heal");
        interactAction = playerMap.FindAction("Action");

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
        if (pauseAction != null) pauseAction.performed += ctx => OnPauseEvent?.Invoke();

        if (basicAttack != null) basicAttack.performed += ctx => OnBasicAttackEvent?.Invoke();
        if (skill_1 != null) skill_1.performed += ctx => OnSkill1Event?.Invoke();
        if (skill_2 != null) skill_2.performed += ctx => OnSkill2Event?.Invoke();
        if (heal != null) heal.performed += ctx => OnHealEvent?.Invoke();
        if (interactAction != null) interactAction.performed += ctx => OnInteractEvent?.Invoke();
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

        // ���ε� ������ JSON �ؽ�Ʈ�� ����
        string json = inputActions.SaveBindingOverridesAsJson();

        // PlayerPrefs(������ �����)�� ����
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    // ����� Ű ������ �ҷ��ͼ� �����մϴ�.
    public void LoadBindingOverrides()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            // JSON �ؽ�Ʈ�� �ٽ� ���ε� ������ ��ȯ�Ͽ� �����(Override)
            inputActions.LoadBindingOverridesFromJson(json);
        }
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