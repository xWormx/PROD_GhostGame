using System.Reflection;
using UnityEditor;
using UnityEngine;
public class ConsoleUtility : MonoBehaviour
{
    // Singleton pattern
    public static ConsoleUtility Instance;
    private void Awake()
    {
        if (ConsoleUtility.Instance != null && ConsoleUtility.Instance != this)
        {
            Destroy(this);
        }
        else
        {
            ConsoleUtility.Instance = this;
        }
    }
    // End of Singleton pattern

    [SerializeField] private bool bEnabled = true;
    public void ClearConsole()
    {
        if (!bEnabled) return;

        // Access the internal LogEntries class and invoke its Clear method
        var assembly = Assembly.GetAssembly(typeof(Editor));
        var logEntries = assembly.GetType("UnityEditor.LogEntries");
        var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}