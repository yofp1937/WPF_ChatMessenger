/*
 * Win32Api 통신에 사용되는 Natvie Struct 모음 Class 입니다.
 */
using System.Runtime.InteropServices;

namespace ChatMessenger.Client.Common.Utilities
{
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
}
