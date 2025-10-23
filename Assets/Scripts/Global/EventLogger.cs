using UnityEngine;
using System.IO; // Behövs för att kunna arbeta med filer
using System; // Behövs för att hämta tid och datum

public class EventLogger : MonoBehaviour
{
    // En statisk instans av loggern för att enkelt kunna nå den från andra skript
    public static EventLogger instance;

    // Sökvägen till loggfilen
    private string logFilePath;

    // Håller reda på den nuvarande spelsessionen
    private static int playSession = 0;

    void Awake()
    {
        // Singleton pattern: ser till att det bara finns en instans av denna logger
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Gör att loggern inte förstörs när man byter scen

            // Öka sessions-ID vid varje ny start av spelet
            playSession = PlayerPrefs.GetInt("PlaySession", 0) + 1;
            PlayerPrefs.SetInt("PlaySession", playSession);
            PlayerPrefs.Save(); // Spara PlayerPrefs direkt

            // Sätter sökvägen till filen. Application.persistentDataPath är en säker plats att spara data på tvärs över olika plattformar
            logFilePath = Path.Combine(Application.persistentDataPath, "gamelog.csv");
            Debug.Log($"Loggfilen sparas här: {logFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loggar en händelse med en tidsstämpel till CSV-filen.
    /// </summary>
    /// <param name="eventName">Namnet på händelsen som ska loggas (t.ex. "#enemykilled").</param>
    public void LogEvent(string eventName)
    {
        // Hämta aktuellt datum och tid
        DateTime now = DateTime.Now;
        string date = now.ToString("yyyy-MM-dd");
        string time = now.ToString("HH.mm.ss");

        // Formatera loggraden enligt specifikationen
        string logEntry = $"playsession: {playSession}, date: {date}, time: {time}, event: {eventName}";

        // Använd StreamWriter för att skriva till filen. 'true' som andra parameter betyder att vi lägger till text i slutet av filen (append) istället för att skriva över den.
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
        catch (Exception e)
        {
            // Logga eventuella fel till Unity-konsolen för felsökning
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
}