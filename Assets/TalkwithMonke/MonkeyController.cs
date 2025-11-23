using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Monke.AI;

public class MonkeyController : MonoBehaviour, IPointerClickHandler
{
    [Header("Visual States")]
    public Sprite eyesClosed;
    public Sprite eyesOpen;
    public Image monkeyImage;

    [Header("AI Visual Feedback (Optional)")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color listeningColor = new Color(1f, 0.9f, 0.9f); // Slight red tint
    [SerializeField] private Color thinkingColor = new Color(1f, 1f, 0.8f);    // Slight yellow tint
    [SerializeField] private Color speakingColor = new Color(0.9f, 1f, 0.9f);  // Slight green tint

    private bool isAwake = false;

    void Start()
    {
        // Start with eyes closed (sleeping)
        monkeyImage.sprite = eyesClosed;

        // Subscribe to AI state changes if AIManager exists
        if (AIManager.Instance != null)
        {
            AIManager.Instance.OnStateChanged += OnAIStateChanged;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isAwake)
        {
            // First click: Wake up the monkey
            WakeUpMonkey();
        }
        else
        {
            // Subsequent clicks: Start AI conversation if idle
            if (AIManager.Instance != null && AIManager.Instance.CurrentState == AIState.Idle)
            {
                Debug.Log("[MonkeyController] Starting AI conversation...");
                AIManager.Instance.StartListening();
            }
        }
    }

    public void WakeUpMonkey()
    {
        isAwake = true;
        monkeyImage.sprite = eyesOpen;
        Debug.Log("[MonkeyController] Monkey is awake!");
    }

    public void SleepMonkey()
    {
        isAwake = false;
        monkeyImage.sprite = eyesClosed;
        Debug.Log("[MonkeyController] Monkey is sleeping...");
    }

    /// <summary>
    /// Handle AI state changes with visual feedback
    /// </summary>
    private void OnAIStateChanged(AIState newState)
    {
        if (monkeyImage == null) return;

        switch (newState)
        {
            case AIState.Idle:
                // Normal color
                monkeyImage.color = normalColor;
                break;

            case AIState.Listening:
                // Listening - slight red tint (ears perked up)
                monkeyImage.color = listeningColor;
                break;

            case AIState.Processing:
                // Thinking - slight yellow tint (thinking hard)
                monkeyImage.color = thinkingColor;
                break;

            case AIState.Speaking:
                // Speaking - slight green tint (talking)
                monkeyImage.color = speakingColor;
                break;

            case AIState.Error:
                // Error - return to normal
                monkeyImage.color = normalColor;
                break;
        }
    }

    public bool IsAwake
    {
        get { return isAwake; }
    }

    void OnDestroy()
    {
        // Unsubscribe from AI events
        if (AIManager.Instance != null)
        {
            AIManager.Instance.OnStateChanged -= OnAIStateChanged;
        }
    }
}
