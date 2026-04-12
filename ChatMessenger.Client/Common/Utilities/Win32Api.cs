using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ChatMessenger.Client.Common.Utilities
{
    public static class Win32Api
    {
        #region Constants & Enums
        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000, // null
            MONITOR_DEFAULTTOPRIMARY = 0x00000001, // 주 모니터
            MONITOR_DEFAULTTONEAREST = 0x00000002 // 창과 가장 가까운 모니터
        }
        #endregion

        #region Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // 모니터 정보 구조체
        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize; // 메모리 크기
            public RECT rcMonitor; // 모니터의 전체 해상도 (Ex: 1920x1080)
            public RECT rcWork; // 작업 표시줄을 제외한 실제 사용 가능 영역
            public int dwFlags; // 모니터 상태 값(기본 모니터인지 등)
        }
        #endregion

        #region User32.dll Methods
        /// <summary>
        /// 지정된 모니터에 대한 상세 정보(전체 해상도 및 작업 영역 크기 등)를 가져옵니다.
        /// </summary>
        /// <param name="hMonitor">조회할 모니터의 핸들</param>
        /// <param name="lpmi">모니터 정보를 담을 MONITORINFO 구조체 (ref 키워드를 통해 값을 채워옴)</param>
        /// <returns>성공 시 true, 실패 시 false 반환</returns>
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// 지정된 윈도우 창이 포함되어있는 모니터 혹은 가장 가까운 모니터의 Handle 값을 반환합니다.
        /// </summary>
        /// <param name="windowHandle">모니터의 위치를 찾고자하는 Window의 핸들</param>
        /// <param name="flags">윈도우가 어느 모니터에도 걸쳐있지 않을 때의 반환 옵션</param>
        /// <returns>모니터의 핸들(nint). 실패 시 nint.Zero 반환</returns>
        [DllImport("user32.dll")]
        public static extern nint MonitorFromWindow(nint windowHandle, uint flags); [DllImport("user32.dll")]


        public static extern nint SendMessage(nint hWnd, uint Msg, nint wParam, nint lParam);

        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint SC_SIZE = 0xF000;

        // 크기 조절 방향 상수 (wParam)
        public enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8
        }
        #endregion
    }
}
