using QuizGraphEditor;
using UnityEditor;
using UnityEngine;

public class AnalyticsExporter : Editor
{
    [MenuItem("Tools/Export Analytics/To JSON")]
    private static void ExportAnalyticsToJson()
    {
        var analytics = Selection.activeObject as StudentPerformanceAnalytics;
        if (analytics == null)
        {
            Debug.LogError("Please select a StudentPerformanceAnalytics asset to export.");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Analytics as JSON", "", "StudentAnalytics.json", "json");
        if (!string.IsNullOrEmpty(path))
        {
            analytics.ExportToJson(path);
        }
    }

    [MenuItem("Tools/Export Analytics/To CSV")]
    private static void ExportAnalyticsToCsv()
    {
        var analytics = Selection.activeObject as StudentPerformanceAnalytics;
        if (analytics == null)
        {
            Debug.LogError("Please select a StudentPerformanceAnalytics asset to export.");
            return;
        }

        string path = EditorUtility.SaveFilePanel("Export Analytics as CSV", "", "StudentAnalytics.csv", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            analytics.ExportToCsv(path);
        }
    }
}
