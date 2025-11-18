using UnityEngine;

public class MaterialExpression : MonoBehaviour
{
    [SerializeField] private Renderer botRenderer;
    [SerializeField] private BotState currentState;

    private Material botMaterial;
    private Color originalColor;

    void Start()
    {
        if (botRenderer == null)
        {
            botRenderer = GetComponent<Renderer>();
        }

        if (botRenderer != null)
        {
            botMaterial = botRenderer.material;
            originalColor = botMaterial.color;
        }
    }

    public void UpdateExpression(BotState state)
    {
        if (botMaterial == null) return;

        currentState = state;

        switch (state)
        {
            case BotState.Idle:
                SetColor(Color.gray);
                break;
            case BotState.Curious:
                SetColor(Color.blue);
                StartCoroutine(BlinkEffect(Color.cyan, 0.5f));
                break;
            case BotState.Excited:
                SetColor(Color.yellow);
                StartCoroutine(PulseEffect(Color.red, 0.3f));
                break;
            case BotState.Sleepy:
                SetColor(Color.cyan);
                break;
            case BotState.Playful:
                SetColor(Color.green);
                StartCoroutine(RainbowEffect());
                break;
        }
    }

    void SetColor(Color color)
    {
        if (botMaterial != null)
        {
            botMaterial.color = color;
        }
    }

    System.Collections.IEnumerator BlinkEffect(Color blinkColor, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            botMaterial.color = Color.Lerp(originalColor, blinkColor, Mathf.PingPong(t * 4, 1));
            yield return null;
        }
        botMaterial.color = originalColor;
    }

    System.Collections.IEnumerator PulseEffect(Color pulseColor, float speed)
    {
        float timer = 0f;
        while (currentState == BotState.Excited) // Continue while in state
        {
            botMaterial.color = Color.Lerp(originalColor, pulseColor, Mathf.PingPong(timer * speed, 1));
            timer += Time.deltaTime;
            yield return null;
        }
        botMaterial.color = originalColor;
    }

    System.Collections.IEnumerator RainbowEffect()
    {
        float hue = 0f;
        while (currentState == BotState.Playful)
        {
            Color rainbow = Color.HSVToRGB(hue % 1, 1, 1);
            botMaterial.color = rainbow;
            hue += Time.deltaTime * 0.5f; // Slow hue change
            yield return null;
        }
        botMaterial.color = originalColor;
    }

    public void Reset()
    {
        StopAllCoroutines();
        SetColor(originalColor);
    }
}
