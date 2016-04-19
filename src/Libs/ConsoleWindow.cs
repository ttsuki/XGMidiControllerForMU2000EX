using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tsukikage.Util
{
    /// <summary>
    /// Console window utility
    /// </summary>
    public class ConsoleWindow
    {

        private static object syncRoot = new object();
        private static TraceListener listener = null;

        /// <summary>
        /// Console is allocated?
        /// </summary>
        public static bool Allocated { get { return NativeMethods.GetConsoleWindow() != IntPtr.Zero; } }

        /// <summary>
        /// Displayed?
        /// </summary>
        public static bool Displayed { get; private set; }

        /// <summary>
        /// Allocate Console.
        /// </summary>
        public static void Allocate()
        {
            lock (syncRoot)
            {
                NativeMethods.AllocConsole();
                Show();
                if (listener == null)
                {
                    Debug.Listeners.Add(listener = new ConsoleTraceListener(true));
                }
            }
        }

        /// <summary>
        /// Set icon for console window.
        /// </summary>
        /// <param name="icon">the icon for console window.</param>
        public static void SetIcon(System.Drawing.Icon icon)
        {
            lock (syncRoot)
            {
                NativeMethods.SetConsoleIcon(icon.Handle);
            }
        }

        /// <summary>
        /// Show allocated console window.
        /// </summary>
        public static void Show()
        {
            lock (syncRoot)
            {
                if (Allocated)
                {
                    NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), 5);
                    Displayed = true;
                }
            }
        }

        /// <summary>
        /// Hide allocated console window.
        /// </summary>
        public static void Hide()
        {
            lock (syncRoot)
            {
                if (Allocated)
                {
                    NativeMethods.ShowWindow(NativeMethods.GetConsoleWindow(), 0);
                    Displayed = false;
                }
            }
        }

        private class NativeMethods
        {
            [DllImport("Kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("Kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeConsole();

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleIcon(IntPtr hIcon);
        }
    }
}
