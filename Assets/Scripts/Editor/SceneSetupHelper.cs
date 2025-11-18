using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class SceneSetupHelper : EditorWindow
{
    [MenuItem("Robot Companion/Complete Setup")]
    static void SetupScene()
    {
        SetupRobot();
        SetupEnvironment();
        SetupNavMesh();
        SetupCamera();
        Debug.Log("Scene setup complete! Robot companion is ready.");
    }

    static void SetupRobot()
    {
        // Find or create robot
        GameObject robotObj = GameObject.Find("Robot");
        if (robotObj == null)
        {
            robotObj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Cozmo.prefab"));
            if (robotObj == null)
            {
                robotObj = new GameObject("Robot");
                // If Cozmo model exists, it should be manually dragged or add mesh filter
            }
            robotObj.name = "Robot";
        }

        // Ensure components
        if (!robotObj.GetComponent<Rigidbody>()) robotObj.AddComponent<Rigidbody>();
        if (!robotObj.GetComponent<CapsuleCollider>()) robotObj.AddComponent<CapsuleCollider>();
        if (!robotObj.GetComponent<NavMeshAgent>()) robotObj.AddComponent<NavMeshAgent>();
        if (!robotObj.GetComponent<MasterCoordinator>()) robotObj.AddComponent<MasterCoordinator>();

        // Remove old pattern
        Object.DestroyImmediate(robotObj.GetComponent<BotStateManager>(), true);
        Object.DestroyImmediate(robotObj.GetComponent<RobotController>(), true);
        WaitForFrame(() => {}); // Delay to ensure destroy
        // Wait for next frame if needed, but in editor, re-add via MasterCoordinator Awake
    }

    static void SetupEnvironment()
    {
        // Create or find ground
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(10, 10, 10);
        }

        // Mark as NavMesh static
        ground.isStatic = true;
        var navMeshModifier = ground.GetComponent<NavMeshModifier>() ?? ground.AddComponent<NavMeshModifier>();
        navMeshModifier.overrideArea = false;
        navMeshModifier.area = NavArea.Walkable;
    }

    static void SetupNavMesh()
    {
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
        Debug.Log("NavMesh baked");
    }

    static void SetupCamera()
    {
        Camera.main.transform.position = new Vector3(0, 10, -10);
        Camera.main.transform.LookAt(new Vector3(0, 1, 0));
    }

    static void WaitForFrame(System.Action action)
    {
        EditorApplication.delayCall += action;
    }
}
