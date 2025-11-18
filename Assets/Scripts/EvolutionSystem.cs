using UnityEngine;

public class EvolutionSystem : MonoBehaviour
{
    [SerializeField] private float evolutionRate = 0.001f;
    [SerializeField] private float maxSize = 1.5f;
    private MemorySystem memory;
    private Vector3 initialScale;

    void Start()
    {
        memory = FindObjectOfType<MemorySystem>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (memory != null && memory.interactionCount > 0)
        {
            // Grow based on interactions
            float growFactor = 1f + (memory.interactionCount * evolutionRate);
            growFactor = Mathf.Clamp(growFactor, 1f, maxSize);
            transform.localScale = initialScale * growFactor;

            // Maybe unlock features at milestones
            if (memory.interactionCount % 100 == 0)
            {
                Debug.Log("Evolution milestone! Robot grew.");
            }
        }
    }
}
