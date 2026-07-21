# 맥락 노트 (Architecture Decision Record)

> 이 문서는 ChatMessenger 프로젝트의 코드에서 실제로 관찰되는 기술/패턴 선택에 대해 "왜 이렇게 설계했는가"를 정리한 근거 문서입니다. 각 결정은 코드 상의 구현 사실을 근거로 역추적하여 작성했습니다.

## 목차
* [ADR-1. MVVM + CommunityToolkit.Mvvm 채택](#adr-1-mvvm--communitytoolkitmvvm-채택)
* [ADR-2. ViewModel-First + 암묵적 DataTemplate 네비게이션](#adr-2-viewmodel-first--암묵적-datatemplate-네비게이션)
* [ADR-3. WeakReferenceMessenger 기반 컴포넌트 간 통신](#adr-3-weakreferencemessenger-기반-컴포넌트-간-통신)
* [ADR-4. 리플렉션 기반 DI 자동 등록](#adr-4-리플렉션-기반-di-자동-등록)
* [ADR-5. REST + SignalR 하이브리드 통신 구조](#adr-5-rest--signalr-하이브리드-통신-구조)
* [ADR-6. Controller-Service-Repository 3계층 분리](#adr-6-controller-service-repository-3계층-분리)
* [ADR-7. JWT Bearer 인증](#adr-7-jwt-bearer-인증)
* [ADR-8. EF Core + EnsureCreated (마이그레이션 미사용)](#adr-8-ef-core--ensurecreated-마이그레이션-미사용)

---

## ADR-1. MVVM + CommunityToolkit.Mvvm 채택

**도입 배경**: WPF는 XAML 데이터 바인딩을 전제로 설계된 프레임워크이므로, 코드비하인드(Code-behind)에 로직을 두면 UI와 비즈니스 로직이 강결합되어 테스트와 유지보수가 어려워집니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| Code-behind 직접 구현 | 러닝커브 없음, 빠른 프로토타이핑 | UI/로직 결합, 테스트 불가, 프로젝트 규모 커질수록 유지보수 붕괴 |
| 순수 MVVM(수동 `INotifyPropertyChanged`) | 외부 의존성 없음 | `[ObservableProperty]` 같은 소스 제너레이터 부재로 보일러플레이트 코드량 급증 |
| `CommunityToolkit.Mvvm`(채택) | 소스 제너레이터로 `[ObservableProperty]`/`[RelayCommand]` 자동 생성, MS 공식 유지보수, `WeakReferenceMessenger` 내장 | 외부 NuGet 의존성 1개 추가 |

**최종 결정 및 이유**: `CommunityToolkit.Mvvm`을 채택했습니다. WPF에서 SOLID의 SRP를 지키려면 View는 오직 표시/입력만 담당하고, ViewModel이 상태와 커맨드를 소유해야 합니다. 이 툴킷은 `ObservableObject` 상속만으로 `[ObservableProperty]` 어트리뷰트가 `PropertyChanged` 알림 코드를 컴파일 타임에 생성해 주므로, 보일러플레이트를 줄이면서도 MVVM 원칙(View-ViewModel 분리)을 강제할 수 있습니다. 실제로 `LoginViewModel`, `ChatRoomViewModel` 등 모든 ViewModel이 `[RelayCommand]`로 커맨드를 노출하고 있습니다.

---

## ADR-2. ViewModel-First + 암묵적 DataTemplate 네비게이션

**도입 배경**: WPF의 기본 방식인 `StartupUri`(고정 XAML 경로)나 코드에서 `new XxxView()`로 직접 Window/UserControl을 생성하는 View-First 방식은, View가 어떤 ViewModel을 사용할지 View 스스로 결정하게 되어 **DI 컨테이너를 통한 ViewModel 생성/주입이 어려워집니다.**

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| View-First (`new XxxView()`) | 직관적 | ViewModel을 View가 직접 생성 → DI 미적용, 테스트 어려움 |
| `DataTemplateSelector` 커스텀 구현 | 세밀한 제어 가능 | 코드량 증가, 매핑 규칙이 코드에 흩어짐 |
| 암묵적 `DataTemplate`(`DataType`) + ViewModel-First(채택) | ViewModel만 `ContentControl.Content`에 할당하면 WPF가 자동으로 View를 렌더링, 매핑 규칙이 XAML 리소스에 선언적으로 집중 | 리소스 병합 순서를 주의해야 함 |

**최종 결정 및 이유**: `Styles/Windows/MainWindowStyle.xaml`, `Styles/Pages/MainShellStyle.xaml`에 `DataType`을 지정한 `DataTemplate`을 선언하고, `ContentControl.Content`에 ViewModel 인스턴스만 바인딩하는 방식을 채택했습니다. 이 덕분에 `MainWindowViewModel.CurrentViewModel`을 교체하는 것만으로 화면이 자동 전환되며, ViewModel은 `App.xaml.cs`의 DI 컨테이너에서 생성되므로 생성자 주입이 자연스럽게 이루어집니다. `WindowService.ShowWindow()`가 네이밍 컨벤션(`ViewModels.X` → `Views.X`, 접미사 치환)으로 최상위 Window만 예외적으로 리플렉션 탐색하는 것도 같은 철학의 연장선입니다.

---

## ADR-3. WeakReferenceMessenger 기반 컴포넌트 간 통신

**도입 배경**: `ContentPanelViewModel`, `ChatListViewModel`, `FriendListViewModel` 등은 서로 형제(sibling) 관계이거나 부모-자식 관계가 아니어서, 생성자 주입만으로는 "친구를 선택하면 상세화면이 바뀐다"처럼 서로 다른 트리에 위치한 ViewModel 간 상태 전파가 불가능합니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| 이벤트(C# `event`) 직접 구독 | 표준 언어 기능 | 발행자가 구독자 타입을 알아야 함(결합도 증가), 구독 해제를 누락하면 메모리 누수 |
| 중앙 상태 저장소(싱글톤 State) | 단일 진실 공급원 | 상태 변경 시점을 추적하기 어려움, 규모가 커지면 God Object화 |
| `WeakReferenceMessenger`(채택) | 발행자-구독자가 서로의 타입을 몰라도 됨(느슨한 결합), Weak Reference라 구독 해제를 깜빡해도 GC 대상이 되면 자동 정리 | 메시지 타입이 늘어나면 흐름 추적이 어려워질 수 있음(→ 본 문서와 계획서로 보완) |

**최종 결정 및 이유**: `WeakReferenceMessenger.Default`를 전역으로 사용해, `ContentPanelViewModel`이 `FriendSelectionChangedMessage`, `ChatRoomSelectionChangedMessage` 등 5종 메시지를 구독하는 방식으로 화면 전환을 완전히 메시지 기반으로 구현했습니다. `FriendListViewModel`이 `ContentPanelViewModel`의 존재를 전혀 알 필요가 없으므로 SRP와 낮은 결합도를 동시에 달성합니다. 다만 Weak Reference에만 의존하지 않고 `BaseViewModel.CleanUp()`에서 `UnregisterAll(this)`을 명시적으로 호출하는 규약을 추가해, GC 타이밍에 의존하지 않고 결정적으로(deterministic) 구독을 해제하도록 보완했습니다.

---

## ADR-4. 리플렉션 기반 DI 자동 등록

**도입 배경**: ViewModel/View 개수가 늘어날 때마다 `DependencyInjectionConfig.cs`에 `services.AddTransient<XxxViewModel>()`을 한 줄씩 추가하는 방식은 등록 누락 위험이 있고, 새 화면을 추가할 때마다 DI 설정 파일을 함께 수정해야 하는 번거로움이 있습니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| 개별 수동 등록 | 명시적, 추적 쉬움 | 등록 누락 시 런타임 예외, 파일이 계속 길어짐 |
| 리플렉션 기반 자동 스캔(채택) | `BaseViewModel`/`Window`를 상속하기만 하면 자동 등록, 새 화면 추가 시 DI 설정 수정 불필요 | 리플렉션 스캔 비용(앱 시작 시 1회이므로 실질적 영향 미미), 예외적으로 다른 생명주기가 필요한 타입(`MainWindowViewModel`)은 별도 처리 필요 |

**최종 결정 및 이유**: `DependencyInjectionConfig.AddViewsAndViewModels()`가 실행 어셈블리에서 `BaseViewModel`을 상속하는 non-abstract 클래스를 전부 찾아 `AddTransient`로 일괄 등록합니다. 다만 앱 전역에서 단 하나만 존재해야 하는 `MainWindowViewModel`은 이후 코드에서 `AddSingleton`으로 명시적으로 재등록하여 예외 처리했습니다. 이는 "규칙 기반 자동화 + 예외 상황만 명시적으로 오버라이드"라는 일관된 설계 원칙을 보여줍니다.

---

## ADR-5. REST + SignalR 하이브리드 통신 구조

**도입 배경**: 채팅 메신저는 "메시지 전송"처럼 요청-응답이 명확한 작업과, "상대방이 보낸 메시지를 실시간으로 받는" 것처럼 서버가 클라이언트에 먼저 알려야 하는 작업이 공존합니다. REST(HTTP)만으로는 후자를 폴링(polling)으로 흉내내야 하므로 지연시간과 서버 부하가 늘어납니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| REST 폴링만 사용 | 구현 단순 | 실시간성 저하, 불필요한 요청량 증가 |
| SignalR만 사용(모든 요청을 Hub로) | 단일 채널 | HTTP의 상태코드/캐싱/표준 인증 미들웨어 등 REST 생태계 이점을 못 씀, Hub 메서드가 비대해짐 |
| REST(쓰기/조회) + SignalR(실시간 알림) 하이브리드(채택) | 데이터 변경은 REST로 명확한 트랜잭션 경계를 유지하고, 변경 결과 알림만 SignalR로 Push → 책임 분리 명확 | 두 채널을 함께 운영해야 하는 복잡도 |

**최종 결정 및 이유**: `ChatHub.cs` 상단 주석에 명시된 대로 "DB 저장 로직은 Hub가 직접 수행하지 않고, Controller/Service가 저장한 뒤 그 결과만 Hub를 통해 중계"하는 원칙을 세웠습니다. 즉 `ChatService.SendMessageAsync`가 REST 요청을 받아 DB에 메시지를 저장(트랜잭션 보장)한 뒤, 저장이 성공한 경우에만 `BaseBusinessService.BroadcastToUsersAsync`로 SignalR 이벤트를 발행합니다. 이렇게 하면 "메시지가 DB에는 없는데 실시간으로만 보였다가 새로고침하면 사라지는" 정합성 문제를 원천적으로 방지할 수 있습니다.

---

## ADR-6. Controller-Service-Repository 3계층 분리

**도입 배경**: 컨트롤러에 EF Core 쿼리를 직접 작성하면 인증/유효성 검사/비즈니스 규칙/DB 접근이 한 클래스에 뒤섞여 SRP를 위반하고, DB 접근 방식(EF Core → 다른 ORM 등)을 교체할 때 컨트롤러까지 수정해야 합니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| Controller에서 DbContext 직접 사용 | 코드량 최소 | 계층 간 책임 혼재, 단위 테스트 어려움 |
| Controller-Service-Repository 3계층(채택) | Service는 인터페이스(`IChatService`)에만 의존 → Mock 대체 가능, Repository가 EF Core를 캡슐화해 쿼리 로직이 한 곳에 집중 | 계층이 늘어나 간단한 CRUD도 파일 여러 개를 오가야 함 |

**최종 결정 및 이유**: `Interfaces/Services/`(비즈니스), `Interfaces/Services/Repositories/`(데이터 접근)로 인터페이스를 분리하고, `BaseBusinessService`(트랜잭션/예외/브로드캐스트 공통 로직)와 `BaseRepositoryService`(DbContext 접근 공통 로직)를 부모 클래스로 두어 LSP를 준수하도록 설계했습니다. 특히 `ChatService`처럼 "채팅방 생성 + 참가자 등록 + 시스템 메시지 생성"이 하나의 트랜잭션이어야 하는 복잡한 유스케이스를 `ExecutedTransactionAsync`로 감싸, Repository는 순수 데이터 접근만 담당하고 트랜잭션 경계는 Service가 소유하도록 책임을 명확히 나눴습니다.

---

## ADR-7. JWT Bearer 인증

**도입 배경**: WPF 클라이언트는 세션 쿠키를 자동 관리하는 브라우저가 아니므로, 서버가 세션 상태를 직접 관리하는 전통적 세션 인증보다 클라이언트가 토큰을 직접 보관하고 매 요청에 첨부하는 방식이 더 적합합니다. 또한 REST(HTTP)와 SignalR(WebSocket)이라는 서로 다른 프로토콜에서 동일한 인증 수단을 재사용해야 합니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| 서버 세션(쿠키) | ASP.NET Core 기본 지원 | WPF는 쿠키 저장소가 브라우저처럼 자동화되어 있지 않아 별도 구현 필요, SignalR과 REST에 각각 다른 방식 필요 |
| JWT Bearer(채택) | Stateless(서버가 세션을 저장하지 않아 확장성 유리), REST의 `Authorization` 헤더와 SignalR의 `AccessTokenProvider`에 동일 토큰 재사용 가능 | 토큰 자체에 만료 전까지 강제 폐기(revoke) 메커니즘이 없음(→ `ForceLogoutMessage`로 클라이언트 측 방어 로직 보완) |

**최종 결정 및 이유**: `TokenService.CreateToken`이 로그인 성공 시 8시간짜리 JWT를 발급하고, 클라이언트는 `AuthHeaderHandler`(`DelegatingHandler`)가 모든 REST 요청에 자동으로 `Authorization: Bearer` 헤더를 삽입하도록 했습니다. 동일 토큰을 `ChatHubService.ConnectAsync`의 `AccessTokenProvider`에도 그대로 전달해, REST와 SignalR 양쪽에서 별도 인증 로직 없이 하나의 토큰으로 통합 인증을 구현했습니다. `AuthorizedBaseController`가 `[Authorize]` 어트리뷰트에 더해 `OnActionExecuting`에서 `CurrentUserEmail` 공백 여부를 다시 검증하는 이중 방어를 둔 것도, JWT가 위조/누락된 극단적 상황까지 방어하려는 의도입니다.

---

## ADR-8. EF Core + EnsureCreated (마이그레이션 미사용)

**도입 배경**: 포트폴리오/개발 초기 단계 프로젝트에서는 스키마가 자주 바뀌므로, `dotnet ef migrations add`로 매번 마이그레이션 파일을 생성/적용하는 절차가 개발 속도를 늦출 수 있습니다.

**대안 비교**:
| 대안 | 장점 | 단점 |
|---|---|---|
| EF Core Migrations | 스키마 변경 이력 추적, 운영 배포에 필수적인 점진적 스키마 반영 가능 | 초기 개발 단계에서는 스키마 변경마다 마이그레이션 생성/정리 부담 |
| `EnsureCreatedAsync()`(채택) | DB가 없으면 현재 모델 그대로 즉시 생성, 초기 개발 속도 우선 | 기존 DB에 스키마 변경을 점진적으로 반영할 수 없음(운영 환경에는 부적합) |

**최종 결정 및 이유**: `DbInitializer.cs`가 `EnsureCreatedAsync()`로 DB를 생성하고 테스트 데이터를 시딩하는 방식을 채택했습니다. 이는 현재 프로젝트가 포트폴리오/개발 단계이며 스키마가 빠르게 반복(iterate)되고 있다는 점을 고려한 실용적 선택으로 보입니다. **다만 이 방식은 운영 배포 후 스키마 변경 시 데이터 유실 위험이 있으므로, 서비스가 안정화되는 시점에는 EF Core Migrations로 전환하는 것을 권장합니다.**
