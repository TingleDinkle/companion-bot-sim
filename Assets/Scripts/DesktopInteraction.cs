using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

public class DesktopInteraction : MonoBehaviour
{
    [DllImport("user32.dll", EntryPoint = "EnumWindows")]
    private static extern IntPtr EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "GetWindowText")]
    private static extern int GetWindowText(IntPtr hWnd, System.Text.StringBuilder str, int maxCount);

    [DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", EntryPoint = "mouse_event")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, IntPtr dwExtraInfo);

    [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
    private static extern bool SetCursorPos(int X, int Y);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DesktopWindow
    {
        public IntPtr hwnd;
        public string title;
        public RECT rect;
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private ConfigurationManager config;
    private List<DesktopWindow> desktopWindows = new List<DesktopWindow>();
    private bool interactionsEnabled;

    void Start()
    {
        config = FindObjectOfType<ConfigurationManager>();
        interactionsEnabled = config != null ? config.enableWindowInteractions : false;
        if (interactionsEnabled)
        {
            EnumerateWindows();
        }
    }

    private void EnumerateWindows()
    {
        desktopWindows.Clear();
        EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        Debug.Log("Enumerated " + desktopWindows.Count + " desktop windows");
    }

    private bool EnumWindowsCallback(IntPtr hWnd, IntPtr lParam)
    {
        const int nMaxCount = 256;
        System.Text.StringBuilder windowTitle = new System.Text.StringBuilder(nMaxCount);
        GetWindowText(hWnd, windowTitle, nMaxCount);

        if (string.IsNullOrEmpty(windowTitle.ToString()))
            return true;

        DesktopWindow win = new DesktopWindow();
        win.hwnd = hWnd;
        win.title = windowTitle.ToString();

        if (GetWindowRect(hWnd, out RECT rect))
        {
            win.rect = rect;
            desktopWindows.Add(win);
            Debug.Log($"Found window: {win.title} at ({rect.Left},{rect.Top}) size ({rect.Right-rect.Left}x{rect.Bottom-rect.Top})");
        }

        return true;
    }

    public void MoveRandomWindow(int deltaX, int deltaY)
    {
        if (!interactionsEnabled || desktopWindows.Count == 0) return;

        DesktopWindow randomWin = desktopWindows[UnityEngine.Random.Range(0, desktopWindows.Count)];
        SetWindowPos(randomWin.hwnd, IntPtr.Zero,
                    randomWin.rect.Left + deltaX,
                    randomWin.rect.Top + deltaY,
                    randomWin.rect.Right - randomWin.rect.Left,
                    randomWin.rect.Bottom - randomWin.rect.Top,
                    0);

        Debug.Log("Moved window: " + randomWin.title);
        ReenumerateAfterDelay();
    }

    public void SimulateMousePrank(Vector2 screenPos)
    {
        if (!interactionsEnabled) return;

        SetCursorPos((int)screenPos.x, (int)screenPos.y);
        mouse_event(0x0002, 0, 0, 0, IntPtr.Zero); // Left down
        System.Threading.Thread.Sleep(50);
        mouse_event(0x0004, 0, 0, 0, IntPtr.Zero); // Left up

        Debug.Log("Simulated mouse click at " + screenPos);
    }

    public DesktopWindow GetClosestWindowToPoint(Vector2 screenPoint)
    {
        DesktopWindow closest = new DesktopWindow();
        float minDist = float.MaxValue;

        foreach (var win in desktopWindows)
        {
            Vector2 winCenter = new Vector2((win.rect.Left + win.rect.Right) / 2f, (win.rect.Top + win.rect.Bottom) / 2f);
            float dist = Vector2.Distance(winCenter, screenPoint);
            if (dist < minDist)
            {
                minDist = dist;
                closest = win;
            }
        }
        return closest;
    }

    public void PrankChaseWindow(Vector2 targetPos)
    {
        var closest = GetClosestWindowToPoint(targetPos);
        if (closest.hwnd != IntPtr.Zero)
        {
            MoveRandomWindow(UnityEngine.Random.Range(-200, 200), UnityEngine.Random.Range(-200, 200));
        }
    }

    private void ReenumerateAfterDelay()
    {
        Invoke(nameof(EnumerateWindows), 0.5f);
    }

    // Safely enable/disable
    public void SetInteractionsEnabled(bool enable)
    {
        interactionsEnabled = enable;
        if (enable)
        {
            EnumerateWindows();
            Debug.Log("Desktop interactions enabled - CAUTION: Bot can now manipulate real windows!");
        }
        else
        {
            Debug.Log("Desktop interactions disabled");
        }
    }
}
