using UnityEngine;
using System.Collections.Generic;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance; // Singleton

    [System.Serializable]
    public class Translation
    {
        public string key;
        public string english = "";
        public string spanish = "";
        public string french = "";
        public string german = "";
    }

    [SerializeField] private ConfigurationManager.Language currentLanguage = ConfigurationManager.Language.English;
    [SerializeField] private List<Translation> translations = new List<Translation>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        var config = FindObjectOfType<ConfigurationManager>();
        if (config != null)
        {
            currentLanguage = config.currentLanguage;
        }
        InitializeTranslations();
    }

    private void InitializeTranslations()
    {
        // Add default translations
        AddTranslation("Mood", "Mood", "Ánimo", "Humeur", "Stimmung");
        AddTranslation("Interactions", "Interactions", "Interacciones", "Interactions", "Interaktionen");
        AddTranslation("Activity", "Activity", "Actividad", "Activité", "Aktivität");
        AddTranslation("Personality", "Personality", "Personalidad", "Personnalité", "Persönlichkeit");
        AddTranslation("Playful", "Playful", "Juguetón", "Joueur", "Spielerisch");
        AddTranslation("Curious", "Curious", "Curioso", "Curieux", "Neugierig");
        AddTranslation("Sleepy", "Sleepy", "Somnoliento", "Endormi", "Schläfrig");
        AddTranslation("Idle", "Idle", "Inactivo", "Inactif", "Untätig");
        AddTranslation("Excited", "Excited", "Emocionado", "Excité", "Aufgeregt");
    }

    public void AddTranslation(string key, string eng, string spa, string fre, string ger)
    {
        var trans = translations.Find(t => t.key == key);
        if (trans == null)
        {
            trans = new Translation { key = key, english = eng, spanish = spa, french = fre, german = ger };
            translations.Add(trans);
        }
        else
        {
            trans.english = eng;
            trans.spanish = spa;
            trans.french = fre;
            trans.german = ger;
        }
    }

    public string GetString(string key)
    {
        var trans = translations.Find(t => t.key == key);
        if (trans == null) return key;

        switch (currentLanguage)
        {
            case ConfigurationManager.Language.Spanish: return trans.spanish;
            case ConfigurationManager.Language.French: return trans.french;
            case ConfigurationManager.Language.German: return trans.german;
            default: return trans.english;
        }
    }

    public void SetLanguage(ConfigurationManager.Language language)
    {
        currentLanguage = language;
    }
}
