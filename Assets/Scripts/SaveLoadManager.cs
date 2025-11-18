using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[System.Serializable]
public class RobotSaveData
{
    public Vector3 position;
    public Quaternion rotation;
    public BotState currentState;
    public float activityLevel;
    public int interactionCount;
    public Vector3 favoriteSpot;
    public TraitDataSerial[] traits;
    public bool[] stackedCubes;
    public float sessionTime;
    public float lastInteractionTime;

    [System.Serializable]
    public class TraitDataSerial
    {
        public string traitName;
        public float value;
    }
}

public class SaveLoadManager : MonoBehaviour
{
    private const string SaveFile = "robotSaveData.dat";
    public static SaveLoadManager Instance; // Singleton

    private MemorySystem memory;
    private BotStateManager stateManager;
    private PersonalityTraitManager personality;
    private CubeStacker cubeStacker;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        memory = FindObjectOfType<MemorySystem>();
        stateManager = FindObjectOfType<BotStateManager>();
        personality = FindObjectOfType<PersonalityTraitManager>();
        cubeStacker = FindObjectOfType<CubeStacker>();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame(); // Auto-save on lost focus
        }
    }

    public void SaveGame(string slotName = "")
    {
        string fileName = string.IsNullOrEmpty(slotName) ? SaveFile : slotName + ".dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/" + fileName);
        RobotSaveData data = new RobotSaveData();

        var robot = FindObjectOfType<RobotController>();
        if (robot != null)
        {
            data.position = robot.transform.position;
            data.rotation = robot.transform.rotation;
        }

        if (stateManager != null)
            data.currentState = stateManager.State;

        if (memory != null)
        {
            data.activityLevel = memory.activityLevel;
            data.interactionCount = memory.interactionCount;
            data.favoriteSpot = memory.favoriteSpot;
        }

        if (personality != null)
        {
            data.traits = new RobotSaveData.TraitDataSerial[6]; // Number of traits
            int idx = 0;
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Playful", value = personality.GetTraitValue(PersonalityTrait.Playful) };
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Curious", value = personality.GetTraitValue(PersonalityTrait.Curious) };
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Energetic", value = personality.GetTraitValue(PersonalityTrait.Energetic) };
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Calm", value = personality.GetTraitValue(PersonalityTrait.Calm) };
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Adventurous", value = personality.GetTraitValue(PersonalityTrait.Adventurous) };
            data.traits[idx++] = new RobotSaveData.TraitDataSerial { traitName = "Cautious", value = personality.GetTraitValue(PersonalityTrait.Cautious) };
        }

        data.sessionTime = Time.realtimeSinceStartup;
        data.lastInteractionTime = Time.time;

        bf.Serialize(file, data);
        file.Close();
        Debug.Log($"Game saved to {fileName}");
    }

    public void LoadGame(string slotName = "")
    {
        string fileName = string.IsNullOrEmpty(slotName) ? SaveFile : slotName + ".dat";
        string path = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(path))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);
            RobotSaveData data = (RobotSaveData)bf.Deserialize(file);
            file.Close();

            var robot = FindObjectOfType<RobotController>();
            if (robot != null)
            {
                robot.transform.position = data.position;
                robot.transform.rotation = data.rotation;
            }

            if (stateManager != null)
                stateManager.SetState(data.currentState);

            if (memory != null)
            {
                memory.activityLevel = data.activityLevel;
                memory.interactionCount = data.interactionCount;
                memory.favoriteSpot = data.favoriteSpot;
            }

            if (personality != null && data.traits != null)
            {
                foreach (var trait in data.traits)
                {
                    switch (trait.traitName)
                    {
                        case "Playful": personality.AdaptTrait(PersonalityTrait.Playful, trait.value, 1); break;
                        case "Curious": personality.AdaptTrait(PersonalityTrait.Curious, trait.value, 1); break;
                        case "Energetic": personality.AdaptTrait(PersonalityTrait.Energetic, trait.value, 1); break;
                        case "Calm": personality.AdaptTrait(PersonalityTrait.Calm, trait.value, 1); break;
                        case "Adventurous": personality.AdaptTrait(PersonalityTrait.Adventurous, trait.value, 1); break;
                        case "Cautious": personality.AdaptTrait(PersonalityTrait.Cautious, trait.value, 1); break;
                    }
                }
            }

            Debug.Log($"Game loaded from {fileName}");
        }
        else
        {
            Debug.LogWarning($"Save file not found: {path}");
        }
    }

    public void NewGame()
    {
        // Reset all data
        var robot = FindObjectOfType<RobotController>();
        if (robot != null)
        {
            robot.transform.position = Vector3.zero;
            robot.transform.rotation = Quaternion.identity;
        }

        if (memory != null)
        {
            memory.activityLevel = 0.5f;
            memory.interactionCount = 0;
            memory.favoriteSpot = memory.transform.position;
        }

        if (stateManager != null)
            stateManager.SetState(BotState.Idle);

        if (personality != null)
        {
            // Reset traits to defaults
        }

        Debug.Log("New game started");
    }

    public string[] GetSaveSlots()
    {
        string path = Application.persistentDataPath;
        string[] files = Directory.GetFiles(path, "*.dat");
        List<string> slots = new List<string>();
        foreach (string f in files)
        {
            slots.Add(Path.GetFileNameWithoutExtension(f));
        }
        return slots.ToArray();
    }
}
