using UnityEditor;
using UnityEngine;

public class PerformanceMonitorUI
{
    private GUIStyle goodStyle;
    private GUIStyle badStyle;
    Color goodColor = Color.green;
    Color badColor = Color.red;

    private PerformanceMonitor performanceMonitor;

    public PerformanceMonitorUI(PerformanceMonitor performanceMonitor)
    {
        this.performanceMonitor = performanceMonitor;
    }

    public void InitializeStyles()
    {
        if (goodStyle == null)
        {
            goodStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = goodColor },
                hover = { textColor = goodColor },
                active = { textColor = goodColor },
                focused = { textColor = goodColor }
            };
        }

        if (badStyle == null)
        {
            badStyle = new GUIStyle(GUI.skin.label)
            {
                normal = { textColor = badColor },
                hover = { textColor = badColor },
                active = { textColor = badColor },
                focused = { textColor = badColor }
            };
        }
    }


    public void DrawFPS(float fps, int targetFPS)
    {
        GUIStyle fpsStyle = fps >= targetFPS ? goodStyle : badStyle;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("FPS", "Frames Per Second. The number of frames rendered every second."), GUILayout.Width(120));
        GUILayout.Label($"{Mathf.Ceil(fps)}", fpsStyle);
        EditorGUILayout.EndHorizontal();
    }

    public void DrawMemory(long memoryUsage)
    {
        GUIStyle memoryStyle = memoryUsage < 500 ? goodStyle : badStyle;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Memory", "Total memory used by the application (in MB)."), GUILayout.Width(120));
        GUILayout.Label($"{memoryUsage} MB", memoryStyle);
        EditorGUILayout.EndHorizontal();
    }

    public void DrawCPU(float cpuUsage, float cpuThreshold)
    {
        GUIStyle cpuStyle = cpuUsage < cpuThreshold ? goodStyle : badStyle;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("CPU Time (ms)", "Time the CPU takes to process tasks per frame."), GUILayout.Width(120));
        GUILayout.Label($"{cpuUsage:F2} ms", cpuStyle);
        EditorGUILayout.EndHorizontal();
    }

    public void DrawGPU(float gpuTime, float cpuThreshold)
    {
        GUIStyle gpuStyle = gpuTime < cpuThreshold ? goodStyle : badStyle;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("GPU Time (ms)", "Time the GPU takes to render the frame."), GUILayout.Width(120));
        GUILayout.Label($"{gpuTime:F2} ms", gpuStyle);
        EditorGUILayout.EndHorizontal();
    }
}
