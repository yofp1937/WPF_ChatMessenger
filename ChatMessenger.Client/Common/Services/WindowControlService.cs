/*
 * Window의 행동을 처리해주는 Service
 */
using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Utilities;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace ChatMessenger.Client.Common.Services
{
    public class WindowControlService : IWindowControlService
    {
        #region Interface 구현부
        /// <inheritdoc/>
        public void MinimizeWindow()
        {
            Window window = GetWindow();
            if (window == null) return;

            window.WindowState = WindowState.Minimized;
        }
        /// <inheritdoc/>
        public void MaximizeWindow()
        {
            Window window = GetWindow();
            if (window == null) return;

            // 윈도우 창이 최대상태면 일반 상태로 돌리고, 일반 상태면 최대 상태로 변경
            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                SetWindowMaxSize(window);
                window.WindowState = WindowState.Maximized;
            }
        }
        /// <inheritdoc/>
        public void CloseWindow()
        {
            GetWindow()?.Close();
        }
        /// <inheritdoc/>
        public void DragMoveWindow()
        {
            Window window = GetWindow();
            if (window == null) return;

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    // 1. 최대화 상태에서의 마우스 상대 위치 및 전체 너비 저장
                    Point mousePos = Mouse.GetPosition(window);
                    double fullWidth = window.ActualWidth;

                    // 2. 가로 비율 계산 (0.0 ~ 1.0)
                    double xRatio = mousePos.X / fullWidth;

                    // 3. 현재 마우스의 화면상 절대 좌표(물리 픽셀) 가져오기
                    Point screenPoint = window.PointToScreen(mousePos);

                    // 모니터의 배율 정보 읽어오기
                    PresentationSource source = PresentationSource.FromVisual(window);
                    if (source?.CompositionTarget != null)
                    {
                        double dpiX = source.CompositionTarget.TransformToDevice.M11;
                        double dpiY = source.CompositionTarget.TransformToDevice.M22;

                        // 4. 창 상태를 Normal로 변경
                        window.WindowState = WindowState.Normal;

                        // 상태를 Normal로 변경하고 바로 ActualWidth에 접근하면 갱신이 안될수도 있어서
                        // 이전에 Maximized로 바꿀때 저장해둔 이전 상태(RestoreBounds)의 Width값을 꺼내와서 사용
                        double normalWidth = window.RestoreBounds.Width;

                        // [마우스 절대 좌표 * 화면 배율] - [작아진 창의 너비 * 클릭했던 비율]
                        window.Left = (screenPoint.X / dpiX) - (normalWidth * xRatio);
                        window.Top = (screenPoint.Y / dpiY) - mousePos.Y;
                    }
                }
                window.DragMove();
            }
        }
        #endregion
        #region private Method
        /// <summary>
        /// 현재 포커스 돼있거나 활성화된 창을 찾아 반환합니다.
        /// </summary>
        private Window GetWindow()
        {
            // 활성화(IsActive)된 창을 찾아냅니다.
            Window? window = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            // 활성화된 창이 없으면 MainWindow를 반환합니다.
            return window ?? Application.Current.MainWindow;
        }
        /// <summary>
        /// 현재 모니터의 작업 영역(작업표시줄 제외)을 계산하여 창의 최대 크기를 제한합니다.
        /// </summary>
        private void SetWindowMaxSize(Window window)
        {
            // OS가 관리하는 현재 window의 Handle값을 받아옵니다.
            nint windowHandle = new WindowInteropHelper(window).Handle;
            // Handle을 기반으로 현재 어느 모니터와 가까운지 찾습니다.
            // (MONITOR_DEFAULTTONEAREST: 현재 창이 걸쳐있는 모니터중 가까운 모니터 선택)
            nint monitor = Win32Api.MonitorFromWindow(windowHandle, (uint)MonitorOptions.MONITOR_DEFAULTTONEAREST);

            // 최대화 시 화면 밖으로 나가는 약간의 테두리 두께 보정을위해 값 계산
            double horizontalBorder = SystemParameters.WindowResizeBorderThickness.Left + SystemParameters.WindowResizeBorderThickness.Right;
            double verticalBorder = SystemParameters.WindowResizeBorderThickness.Top + SystemParameters.WindowResizeBorderThickness.Bottom;

            // 유효한 모니터 핸들을 찾았으면
            if (monitor != nint.Zero)
            {
                MONITORINFO monitorInfo = new();
                monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
                // 모니터의 가용 영역을 받아옵니다.
                Win32Api.GetMonitorInfo(monitor, ref monitorInfo);

                PresentationSource source = PresentationSource.FromVisual(window);
                if (source?.CompositionTarget != null)
                {
                    // 현재 모니터들의 배율 설정값을 받아옵니다.
                    double matrixX = source.CompositionTarget.TransformToDevice.M11;
                    double matrixY = source.CompositionTarget.TransformToDevice.M22;

                    // 배율 적용, 우측 하단에 미리 계산한 두께 보정값 적용
                    window.MaxHeight = Math.Abs(monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top) / matrixY + verticalBorder;
                    window.MaxWidth = Math.Abs(monitorInfo.rcWork.Right - monitorInfo.rcWork.Left) / matrixX + horizontalBorder;
                }
                else
                {
                    // 모니터를 찾지못했으면 기본 배율 적용해서 최대화
                    window.MaxHeight = Math.Abs(monitorInfo.rcWork.Bottom - monitorInfo.rcWork.Top) + verticalBorder;
                    window.MaxWidth = Math.Abs(monitorInfo.rcWork.Right - monitorInfo.rcWork.Left) + horizontalBorder;
                }
            }
        }
        #endregion
    }
}
