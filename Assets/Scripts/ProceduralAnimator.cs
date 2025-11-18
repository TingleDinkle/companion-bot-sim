using UnityEngine;

public class ProceduralAnimator : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingHeight = 0.1f;
    [SerializeField] private float bobbingSpeed = 2f;

    [Header("Spin Settings")]
    [SerializeField] private float spinSpeed = 180f; // Degrees per second

    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceFrequency = 3f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem thinkingParticles;

    private Vector3 initialPosition;
    private float timeOffset;
    private bool isBobbing = false;
    private bool isSpinning = false;
    private bool isBouncing = false;
    private bool isSlowMotion = false;

    void Start()
    {
        initialPosition = transform.localPosition;
        timeOffset = Random.value * Mathf.PI * 2; // Random start phase

        if (thinkingParticles == null)
        {
            GameObject particleObj = new GameObject("ThinkingParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.up * 0.5f; // Above head
            thinkingParticles = particleObj.AddComponent<ParticleSystem>();

            // Simple configuration
            var main = thinkingParticles.main;
            main.startSpeed = 1f;
            main.startSize = 0.05f;
            main.startLifetime = 1f;
            main.maxParticles = 10;

            var emission = thinkingParticles.emission;
            emission.rateOverTime = 5f;

            var shape = thinkingParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
        }

        thinkingParticles.Stop();
    }

    void Update()
    {
        Vector3 posOffset = Vector3.zero;

        if (isBobbing)
        {
            posOffset += Vector3.up * Mathf.Sin((Time.time + timeOffset) * bobbingSpeed) * bobbingHeight;
        }

        if (isBouncing)
        {
            posOffset += Vector3.up * Mathf.Abs(Mathf.Sin((Time.time + timeOffset) * bounceFrequency)) * bounceHeight;
        }

        transform.localPosition = initialPosition + posOffset;

        if (isSpinning)
        {
            float speedMultiplier = isSlowMotion ? 0.3f : 1f;
            transform.Rotate(Vector3.up, spinSpeed * speedMultiplier * Time.deltaTime);
        }

        // Slow motion affects overall speed, but for simplicity, just spin speed
        // Could extend to NavAgent speed if linked
    }

    public void SetBobbing(bool enable)
    {
        isBobbing = enable;
        if (!enable)
        {
            transform.localPosition = initialPosition;
        }
    }

    public void SetSpin(bool enable)
    {
        isSpinning = enable;
    }

    public void SetBounce(bool enable)
    {
        isBouncing = enable;
        if (!enable)
        {
            transform.localPosition = initialPosition;
        }
    }

    public void SetSlowMotion(bool enable)
    {
        isSlowMotion = enable;
    }

    public void SetThinkingParticles(bool enable)
    {
        if (enable)
        {
            if (!thinkingParticles.isPlaying)
                thinkingParticles.Play();
        }
        else
        {
            thinkingParticles.Stop();
        }
    }

    public void Reset()
    {
        isBobbing = false;
        isSpinning = false;
        isBouncing = false;
        isSlowMotion = false;
        SetThinkingParticles(false);
        transform.localPosition = initialPosition;
        transform.localRotation = Quaternion.identity; // Assuming we don't want persistent rotation
    }
}
