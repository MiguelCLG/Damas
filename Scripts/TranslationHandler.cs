using Godot;
using Godot.Collections;
using Newtonsoft.Json;

public partial class TranslationHandler : Node
{
    private HTTPRequest httpRequest;
    Dictionary<string, Translation> translations = new Dictionary<string, Translation>();

    public override void _Ready()
    {
        LoadTranslationLocally("en");

        /*
        httpRequest = new HTTPRequest();
        AddChild(httpRequest);
        httpRequest.Connect("request_completed", this, nameof(OnRequestCompleted));

        string url = "http://localhost:8080/en.json"; // Replace with your endpoint or local file server
        httpRequest.Request(url);*/
    }

    public void LoadTranslationLocally(string locale)
    {
        string jsonFilePath = $"{locale}.json";
        string jsonContent = ReadJsonFile(jsonFilePath);

        // Deserialize the JSON content into a Dictionary<string, string>
        var translationData = DeserializeTranslationJson(jsonContent);

        // Create the translation resource and add the messages
        Translation language = CreateTranslation(locale, translationData);
        if (!translations.ContainsKey(locale))
        {
            translations.Add(locale, language);
            AddTranslationToServer(language);
        }
        SetServerLocale(language.Locale);
    }

    private void OnRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode == 200)  // OK
        {
            // Convert the byte array to string
            string jsonContent = System.Text.Encoding.UTF8.GetString(body);

            // Deserialize the JSON content into a Dictionary<string, string>
            var translationData = DeserializeTranslationJson(jsonContent);

            // Create the translation resource and add the messages
            Translation language = CreateTranslation("en", translationData);
            AddTranslationToServer(language);
            SetServerLocale(language.Locale);
        }
        else
        {
            GD.PrintErr($"Failed to retrieve translation. HTTP Response Code: {responseCode}");
        }
    }

    public string ReadJsonFile(string path)
    {
        // Read the file content (for testing purposes, simulating GET request)
        return System.IO.File.ReadAllText(path);
    }

    public Dictionary<string, string> DeserializeTranslationJson(string jsonContent)
    {
        // Deserialize the JSON into a dictionary
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
    }

    public Translation CreateTranslation(string lang, Dictionary<string, string> translationData)
    {
        // Create new translation for the language
        Translation language = new Translation();
        language.Locale = lang;

        // Loop through the dictionary and add the translations as messages
        foreach (var entry in translationData)
        {
            language.AddMessage(entry.Key, entry.Value);
        }

        return language;
    }

    public void SetServerLocale(string locale)
    {
        // Set the current display language
        TranslationServer.SetLocale(locale);
    }

    public void AddTranslationToServer(Translation language)
    {
        // Add new translation to the server
        TranslationServer.AddTranslation(language);
    }
}
