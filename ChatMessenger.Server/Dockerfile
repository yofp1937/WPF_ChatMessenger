# =============================================================================
# [1단계]: 실행 환경 설정
# =============================================================================
# /app 에 프로그램을 배치(루트 폴더로 설정)하고 포트 번호는 8080으로 설정한 뒤 aspnet으로 구동하겠다고 설정
# ★ aspnet이란: SDK로 컴파일을 끝낸 dll 파일로 프로그램을 구동시켜주는 엔진
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# =============================================================================
# [2단계]: SDK를 사용해 소스 코드를 컴파일
# =============================================================================
# /src에 컴파일용 SDK를 준비
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 외부 라이브러리 목록을 미리 불러와서 변경사항이 있으면 패키지를 새로 다운로드합니다.
# 만약 외부 라이브러리 목록이 이전 실행 환경과 똑같으면 패키지 다운로드를 건너뛰게 해줍니다.
COPY ["ChatMessenger.Server/ChatMessenger.Server.csproj", "ChatMessenger.Server/"]
COPY ["ChatMessenger.Shared/ChatMessenger.Shared.csproj", "ChatMessenger.Shared/"]
RUN dotnet restore "ChatMessenger.Server/ChatMessenger.Server.csproj"

# 소스 코드들을 COPY 명령어로 현재 디렉터리로 복사해온 뒤
# csproj가 있는 /src/ChatMessenger.Server로 이동해서 dotnet build "ChatMessenger.Server.csproj" 명령어로 컴파일을 실행합니다.
# 컴파일은 Release 모드로 구동하고 결과물은 /app/build 폴더에 배치시킵니다.
COPY . .
WORKDIR "/src/ChatMessenger.Server"
RUN dotnet build "ChatMessenger.Server.csproj" -c Release -o /app/build

# =============================================================================
# [3단계]: 컴파일된 파일에서 프로그램 구동에 필요한 핵심 파일들만 선별
# =============================================================================
# build에서 설정한 내용을 publish로 명명하고 추가 작업을 진행합니다.
# 찌꺼기 파일들은 전부 제외하고 프로그램 실행에 필요한 중요 파일들만 /app/publish로 옮깁니다.
# UseAppHost = false는 aspnet에서 배포 파일을 만들때 실행 파일까지 같이 만드는데 리눅스는 실행 파일이 필요없어서 생성 옵션을 끄는것
FROM build AS publish
RUN dotnet publish "ChatMessenger.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
# [4단계]: 3단계에서 선별한 핵심 파일들로 1단계에서 설정한 실행 환경을 적용해 프로그램 실행 구성
# =============================================================================
# 1단계에서 설정한 base 설정을 불러온 뒤 /app으로 이동하여 /app/publish 폴더의 내용물들을 복사해오고
# docker가 실행될때 명령어 dotnet ChatMessenger.Server.dll을 실행하겠다.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatMessenger.Server.dll"]