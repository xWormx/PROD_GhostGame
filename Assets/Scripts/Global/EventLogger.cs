using UnityEngine;
using System.IO; // Beh�vs f�r att kunna arbeta med filer
using System; // Beh�vs f�r att h�mta tid och datum

public class EventLogger : MonoBehaviour
{
    // En statisk instans av loggern f�r att enkelt kunna n� den fr�n andra skript
    public static EventLogger instance;

    // S�kv�gen till loggfilen
    private string logFilePath;

    // H�ller reda p� den nuvarande spelsessionen
    private static int playSession = 0;

    void Awake()
    {
        // Singleton pattern: ser till att det bara finns en instans av denna logger
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // G�r att loggern inte f�rst�rs n�r man byter scen

            // �ka sessions-ID vid varje ny start av spelet
            playSession = PlayerPrefs.GetInt("PlaySession", 0) + 1;
            PlayerPrefs.SetInt("PlaySession", playSession);
            PlayerPrefs.Save(); // Spara PlayerPrefs direkt

            // S�tter s�kv�gen till filen. Application.persistentDataPath �r en s�ker plats att spara data p� tv�rs �ver olika plattformar
            logFilePath = Path.Combine(Application.persistentDataPath, "gamelog.csv");
            Debug.Log($"Loggfilen sparas h�r: {logFilePath}");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Loggar en h�ndelse med en tidsst�mpel till CSV-filen.
    /// </summary>
    /// <param name="eventName">Namnet p� h�ndelsen som ska loggas (t.ex. "#enemykilled").</param>
    public void LogEvent(string eventName)
    {
        // H�mta aktuellt datum och tid
        DateTime now = DateTime.Now;
        string date = now.ToString("yyyy-MM-dd");
        string time = now.ToString("HH.mm.ss");

        // Formatera loggraden enligt specifikationen
        string logEntry = $"playsession: {playSession}, date: {date}, time: {time}, event: {eventName}";

        // Anv�nd StreamWriter f�r att skriva till filen. 'true' som andra parameter betyder att vi l�gger till text i slutet av filen (append) ist�llet f�r att skriva �ver den.
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
        catch (Exception e)
        {
            // Logga eventuella fel till Unity-konsolen f�r fels�kning
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
}