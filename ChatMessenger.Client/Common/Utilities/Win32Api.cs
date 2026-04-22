/*
 * Window OS의 기능을 직접 호출하기위한 메서드 모음 클래스입니다.
 */
using System.Runtime.InteropServices;

namespace ChatMessenger.Client.Common.Utilities
{
    public static class Win32Api
    {
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
        public static extern nint MonitorFromWindow(nint windowHandle, uint flags);
        #endregion
    }
}
