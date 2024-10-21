using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public class PerformanceMonitor : EditorWindow
{
    private float deltaTime = 0.0f;
    private int targetFPS = 60;

    private PerformanceMonitorUI ui;

    [MenuItem("Tools/Performance Monitor")]
    public static void ShowWindow()
    {
        GetWindow<PerformanceMonitor>("Performance Monitor").Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateData;
        ui = new PerformanceMonitorUI(this);
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateData;
    }

    private void UpdateData()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        Repaint();
    }

    private void OnGUI()
    {
        ui.InitializeStyles();
        GUILayout.Label("Performance Monitoring", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetFPS = EditorGUILayout.IntField(
            new GUIContent("Target FPS", "The desired frame rate for smooth performance."),
            targetFPS
        );

        // Calculate metrics
        float cpuThreshold = 1000f / targetFPS;
        float fps = 1.0f / deltaTime;
        long memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (1024 * 1024);
        float cpuUsage = Time.deltaTime * 1000.0f;
        float gpuTime = GetGPUFrameTime();

        //Draw Metrics
        ui.DrawFPS(fps, targetFPS);
        ui.DrawMemory(memoryUsage);
        ui.DrawCPU(cpuUsage, cpuThreshold);
        ui.DrawGPU(gpuTime, cpuThreshold);
    }

    private float GetGPUFrameTime()
    {
        FrameTiming[] frameTimings = new FrameTiming[1];
        FrameTimingManager.CaptureFrameTimings();
        FrameTimingManager.GetLatestTimings(1, frameTimings);

        return (float)frameTimings[0].gpuFrameTime;
    }
}
