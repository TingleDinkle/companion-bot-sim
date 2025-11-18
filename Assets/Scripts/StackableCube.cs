using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StackableCube : MonoBehaviour
{
    [SerializeField] private float stabilityThreshold = 0.1f; // How much tilt allows stability
    [SerializeField] private float fallTimer = 2f; // Time before falling when unstable
    [SerializeField] private LayerMask placementLayer;

    private Rigidbody rb;
    private bool isPlaced = false;
    private float stabilityTimer = 0f;
    private Quaternion initialRotation;

    private Color originalColor;
    private Renderer rend;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (isPlaced && !IsStable())
        {
            stabilityTimer += Time.deltaTime;
            if (stabilityTimer >= fallTimer)
            {
                Fall();
            }
        }
        else
        {
            stabilityTimer = 0f;
        }
    }

    private bool IsStable()
    {
        // Check if rotation is close to initial
        return Quaternion.Angle(transform.rotation, initialRotation) < stabilityThreshold;
    }

    private void Fall()
    {
        isPlaced = false;
        rb.isKinematic = false;
        rb.AddTorque(Random.insideUnitSphere * 10f);
        SetHighlight(false);

        // Notify stacker if exists
        var stacker = FindObjectOfType<CubeStacker>();
        if (stacker != null)
        {
            stacker.OnStackBroken(this);
        }
    }

    public void PlaceOnSurface(Vector3 position)
    {
        transform.position = position;
        transform.rotation = initialRotation;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isPlaced = true;
        SetHighlight(true);
        stabilityTimer = 0f;
    }

    public bool IsPlaced => isPlaced;

    private void SetHighlight(bool highlight)
    {
        if (rend != null)
        {
            rend.material.color = highlight ? Color.green : originalColor;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // If hit hard while placed, might fall
        if (isPlaced && collision.relativeVelocity.magnitude > 0.5f)
        {
            Fall();
        }
    }
}
