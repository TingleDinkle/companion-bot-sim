using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class DesktopOverlay : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static extern IntPtr GetWindowLong(IntPtr hwnd, int _nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "GetActiveWindow")]
    private static extern IntPtr GetActiveWindow();

    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_LAYERED = 0x00080000;
    private const uint LWA_COLORKEY = 0x00000001;
    private const uint LWA_ALPHA = 0x00000002;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
#endif

    [Header("Overlay Settings")]
    [SerializeField] private bool enableAlwaysOnTop = true;
    [SerializeField] private bool enableTransparency = false;
    [SerializeField] private Color transparentColor = Color.black;
    [SerializeField] private float transparencyAlpha = 0.8f;

    [SerializeField] private int windowWidth = 400;
    [SerializeField] private int windowHeight = 300;

    void Start()
    {
#if UNITY_STANDALONE_WIN
        IntPtr windowPtr = GetActiveWindow();

        if (enableAlwaysOnTop)
        {
            SetWindowPos(windowPtr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        if (enableTransparency)
        {
            // Make window layered
            IntPtr exStyle = GetWindowLong(windowPtr, GWL_EXSTYLE);
            SetWindowLong(windowPtr, GWL_EXSTYLE, (IntPtr)((int)exStyle | WS_EX_LAYERED));

            // Set transparent color key (if black background)
            // Note: Requires camera clear flags set to Solid Color with transparentColor
            uint colorKey = ((uint)transparentColor.r << 16) | ((uint)transparentColor.g << 8) | (uint)transparentColor.b;
            // Unity doesn't directly expose SetLayeredWindowAttributes, so this is limited
            // For full transparency, might need custom native plugin
        }

        // Set resolution if desired
        Screen.SetResolution(windowWidth, windowHeight, false);
#endif
    }

    void Update()
    {
        // Screen edge bouncing logic (basic)
        if (Screen.fullScreen) return; // Don't bounce if fullscreen

        IntPtr windowPtr = GetActiveWindow();
        // Note: Position management would need more complex interop
        // For now, assume user positions manually
    }
}
