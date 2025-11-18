using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private string interactionMessage = "Interacted!";
    [SerializeField] private RobotController robotController;

    private void Start()
    {
        if (robotController == null)
        {
            robotController = FindObjectOfType<RobotController>();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log(interactionMessage);
        if (robotController != null)
        {
            robotController.OnInteraction();
        }
        // Add custom interaction logic here
        // For example, play animation/sound
    }
}
