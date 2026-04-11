/*
 * View-ViewModel간 결합도를 낮추기 위한 Window 관리 클래스입니다.
 * ViewModel을 기반으로 적절한 WindowView를 찾아 화면에 표시해주는 UI 관리 서비스입니다.
 * 
 * [매핑 규칙]
 * ChatMessenger.Client.ViewModels.{Name}ViewModel -> ChatMessenger.Client.Views.{Name}View
 * 위 형식으로 이름이 매핑되어야 자동으로 이를 감지할 수 있습니다.
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;

namespace ChatMessenger.Client.Common.Services
{
    public class WindowService : IWindowService
    {
        private readonly IServiceProvider _serviceProvider;

        public WindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // TODO: 메세지 구독 해야함
        }

        public void ShowWindow(BaseViewModel viewModel)
        {
            var viewType = GetViewForViewModel(viewModel.GetType());
            if (viewType == null) return;

            Window? window = _serviceProvider.GetRequiredService(viewType) as Window;
            if (window == null) return;

            window.DataContext = viewModel;
            window.Show();
        }

        private Type? GetViewForViewModel(Type viewModelType)
        {
            string? viewModelName = viewModelType.FullName;
            if (string.IsNullOrEmpty(viewModelName)) return null;

            // 파일 이름중 ".ViewModels."를 ".Views."로 변경하고 "ViewModel"을 "View"로 바꿔서
            // ViewModel과 View를 매핑시킵니다.
            string viewName = viewModelName.Replace(".ViewModels.", ".Views.").Replace("ViewModel", "View");
            string typeString = $"{viewName}, {viewModelType.Assembly.FullName}";

            Debug.WriteLine($"[WindowService] Mapping: {viewModelType.Name} -> {viewName}");
            return Type.GetType(typeString);
        }
    }
}
