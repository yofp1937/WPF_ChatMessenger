/*
 * Window 창의 행동을 처리해주는 Interface
 */
namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IWindowControlService
    {
        /// <summary>
        /// 창을 최소화합니다.
        /// </summary>
        void MinimizeWindow();
        /// <summary>
        /// 창을 최대화합니다.
        /// </summary>
        void MaximizeWindow();
        /// <summary>
        /// 창을 닫습니다.
        /// </summary>
        void CloseWindow();
        /// <summary>
        /// 창을 누르고있을시 이동시킵니다.
        /// </summary>
        void DragMoveWindow();
    }
}
