using System;
using UnityEngine;

public static class Debug
{
    public static bool isDebugBuild
    {
        get { return UnityEngine.Debug.isDebugBuild; }
    }

    private static string FormatMessage(object message, string color)
    {
        return $"<color={color}>[{DateTime.Now:HH:mm:ss}] {message}</color>";
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.Log(FormatMessage(message, "white")); // �Ϲ� �޽���: ���
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Log(object message, UnityEngine.Object context)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.Log(FormatMessage(message, "white"), context); // �Ϲ� �޽���: ���
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarning(object message)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.LogWarning(FormatMessage(message, "yellow")); // ��� �޽���: �����
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogWarning(object message, UnityEngine.Object context)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.LogWarning(FormatMessage(message, "yellow"), context); // ��� �޽���: �����
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogError(object message)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.LogError(FormatMessage(message, "red")); // ���� �޽���: ������
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogError(object message, UnityEngine.Object context)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.LogError(FormatMessage(message, "red"), context); // ���� �޽���: ������
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void LogCritical(object message)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.LogError(FormatMessage(message, "magenta")); // �ɰ��� ����: ��ȫ��
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void DrawLine(Vector3 start, Vector3 end, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void DrawRay(Vector3 start, Vector3 dir, Color color = default(Color), float duration = 0.0f, bool depthTest = true)
    {
#if (DEBUG_MODE)
        UnityEngine.Debug.DrawRay(start, dir, color, duration, depthTest);
#endif
    }

    [System.Diagnostics.Conditional("DEBUG_MODE")]
    public static void Assert(bool condition)
    {
#if (DEBUG_MODE)
        if (!condition) throw new Exception();
#endif
    }
}
