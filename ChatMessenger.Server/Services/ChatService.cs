using Azure;
using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Hubs;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Constants;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using ChatMessenger.Shared.Enums;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChatMessenger.Server.Services
{
    /// <summary>
    /// ChatController의 요청에따라 채팅과 관련된 요청(방 목록 요청, 방 생성, 방 입장 등)을 처리해주는 Service입니다.
    /// </summary>
    public class ChatService : BaseBusinessService, IChatService
    {
        private readonly IHubContext<ChatHub> _chatHubContext;
        private IChatRoomRepositoryService _chatRoomRepository;
        private IChatParticipantRepositoryService _chatParticipantRepository;
        private IChatMessageRepositoryService _chatMessageRepository;
        private IUserRepositoryService _userRepository;

        public ChatService(IHubContext<ChatHub> chatHubContext, IChatRoomRepositoryService chatRoomRepository, IChatParticipantRepositoryService chatParticipantRepository,
            IChatMessageRepositoryService chatMessageRepository, IUserRepositoryService userRepository)
        {
            _chatHubContext = chatHubContext;
            _chatRoomRepository = chatRoomRepository;
            _chatParticipantRepository = chatParticipantRepository;
            _chatMessageRepository = chatMessageRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// 채팅방에 접근할 권한이 있는지 확인하기위해 ChatParticipant Entity를 찾아서 반환하는 메서드입니다.
        /// </summary>
        /// <remarks>
        /// 해당 메서드는 ChatParticipant 객체를 찾지 못할시 throw를 던지기때문에<br/>
        /// 반드시 try-catch문을 사용하는 BaseBusinessService의 ExecutedBusinessLogicAsync 메서드 내부에서 실행되어야합니다.
        /// </remarks>
        /// <param name="roomId">접근하려는 채팅방의 식별 번호</param>
        /// <param name="userEmail">권한을 확인하려는 User의 Email</param>
        /// <param name="isTracking">Entity 추적 사용 여부<br/>
        /// true: Entity의 상태 변경(Update, Remove 등)이 Db에 반영되도록 추적<br/>
        /// false: 조회 전용(readonly)으로 성능 최적화</param>
        /// <returns>ChatParticipant Entity</returns>
        private async Task<ChatParticipant> GetValidatedParticipantAsync(Guid roomId, string userEmail, bool isTracking = false)
        {
            // 1. ChatParticipant 객체 요청
            ChatParticipant? participant = isTracking
                ? await _chatParticipantRepository.GetTrackingParticipantEntityAsync(roomId, userEmail)
                : await _chatParticipantRepository.GetParticipantEntityAsync(roomId, userEmail);
            // 2. 없으면 throw (BaseBusinessService의 ExecutedBusinessLogicAsync 내부 catch에서 예외 처리)
            if (participant == null || participant.IsLeft)
                throw new UnauthorizedAccessException("해당 채팅방에 접근 권한이 없습니다.");
            // 3. 있으면 반환
            return participant;
        }
        #region ChatRoom 관련
        /// <inheritdoc/>
        public async Task<ServiceResult<ChatRoomSummaryResponse>> GetChatRoomSummaryResponseAsync(Guid roomId, string myEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (roomId == Guid.Empty || string.IsNullOrEmpty(myEmail))
                    return ServiceResult<ChatRoomSummaryResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(roomId, myEmail);
                // 3. Response 생성에 필요한 DTO 추출
                ChatRoomSummaryDTO? result = await _chatRoomRepository.GetChatRoomSummaryDTOAsync(participant);
                if (result == null)
                    return ServiceResult<ChatRoomSummaryResponse>.Failed("ChatRoomSummaryDTO 생성에 실패했습니다.", ServiceResultType.InternalServerError);
                // 4. DTO를 Response로 변환하여 반환
                ChatRoomSummaryResponse response = ChatMapper.ToSummaryResponse(result);
                return ServiceResult<ChatRoomSummaryResponse>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<List<ChatRoomSummaryResponse>>> GetChatRoomSummaryResponseListAsync(string myEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (string.IsNullOrEmpty(myEmail))
                    return ServiceResult<List<ChatRoomSummaryResponse>>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. Response 생성에 필요한 DTO 추출 (방금 가입한 유저는 가입된 ChatRoom이 없으니 List가 0개여도 정상)
                List<ChatRoomSummaryDTO> result = await _chatRoomRepository.GetChatRoomSummaryDTOListAsync(myEmail);
                // 3. DTO를 Response로 매핑하고 List화하여 반환
                List<ChatRoomSummaryResponse> response = result.Select(ChatMapper.ToSummaryResponse).ToList();
                return ServiceResult<List<ChatRoomSummaryResponse>>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<ChatRoomDetailResponse>> GetChatRoomDetailResponseAsync(Guid roomId, string myEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (roomId == Guid.Empty || string.IsNullOrEmpty(myEmail))
                    return ServiceResult<ChatRoomDetailResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(roomId, myEmail);
                // 3. Response 생성에 필요한 채팅방 참가자들 정보 추출
                List<ChatParticipantProjection> projectionResult = await _chatParticipantRepository.GetParticipantProjectionListAsync(roomId);
                if (projectionResult.Count == 0)
                    return ServiceResult<ChatRoomDetailResponse>.Failed("ChatParticipantProjection 생성에 실패했습니다.", ServiceResultType.InternalServerError);
                // 4. Response 생성에 필요한 채팅방 최근 메세지 정보 추출
                List<ChatMessage> messagesResult = await _chatMessageRepository.GetLastFiftyMessageListAsync(roomId, participant.EntryMessageId);
                // 5. 추출한 Data들로 Response로 매핑하여 반환
                ChatRoomDetailResponse response = ChatMapper.ToChatRoomDetailResponse(participant, projectionResult, messagesResult);
                return ServiceResult<ChatRoomDetailResponse>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<Guid>> CreateGroupChatRoomAsync(string myEmail, CreateGroupChatRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(request.Title) || request.TargetEmails.Count == 0)
                    return ServiceResult<Guid>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 그룹 채팅방을 만들고 참가자들을 초대한 뒤 시스템 메세지까지 등록
                JoinAndLeaveChatRoomDTO transactionResult = await CreateGroupChatRoomWithParticipantsInternalAsync(myEmail, request);
                if (transactionResult.SystemMessage == null)
                    throw new InvalidOperationException("SystemMessage가 누락됐습니다.");
                // 3. 참가자들에게 입장 SystemMessage 전송
                bool sendMessageResult = await SendJoinAndLeaveSystemMessageInternalAsync(transactionResult, true);
                if(!sendMessageResult)
                    throw new InvalidOperationException("SystemMessage 전송에 실패했습니다.");
                // 4. 결과 반환
                return ServiceResult<Guid>.Success(transactionResult.RoomId);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<Guid>> GetOrCreatePrivateChatAsync(string myEmail, CreatePrivateChatRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(request.TargetEmail))
                    return ServiceResult<Guid>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. myEmail과 request.TargetEmail 사이의 채팅방 검색
                ChatRoom? room = await _chatParticipantRepository.GetPrivateChatRoomEntityAsync(myEmail, request.TargetEmail);
                // 3. 기존 채팅방 없으면 생성
                if (room == null)
                    room = await CreatePrivateChatRoomInternalAsync(myEmail, request.TargetEmail);
                // 4. 존재하면 참여 상태 확인
                else
                    await EnsureParticipantActiveAsync(room.Id, myEmail);
                // 5. 채팅방 식별 번호 반환
                return ServiceResult<Guid>.Success(room.Id);
            });
        }
        #region ChatRoom private 메서드
        /// <summary>
        /// 신규 그룹 채팅방을 개설하고, 참가자들을 추가한 뒤, 시스템 메세지까지 등록합니다.
        /// </summary>
        /// <param name="myEmail">방 개설자의 Email</param>
        /// <param name="request">방 개설 정보</param>
        /// <returns>JoinAndLeaveRoom DTO</returns>
        private async Task<JoinAndLeaveChatRoomDTO> CreateGroupChatRoomWithParticipantsInternalAsync(string myEmail, CreateGroupChatRequest request)
        {
            return await ExecuteTransactionAsync(_chatRoomRepository, async () =>
            {
                // 1. 새로운 그룹 채팅방 생성하고, result에 RoomId 삽입
                ChatRoom? roomResult = await _chatRoomRepository.CreateChatRoomAsync(request.Title, request.ProfileImageURL, true);
                if (roomResult == null)
                    throw new InvalidOperationException("새로운 채팅방 생성에 실패 했습니다.");
                // 2. 참가자 등록, 
                JoinAndLeaveChatRoomDTO result = await AddParticipantsAndCreateMessageInternalAsync(roomResult.Id, myEmail, request.TargetEmails);
                // 3. result 반환
                return result;
            });
        }
        /// <summary>
        /// 두 유저 사이의 1대1 채팅방을 생성합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="targetEmail">채팅 상대 User의 Email</param>
        /// <returns>생성된 ChatRoom Entity</returns>
        private async Task<ChatRoom> CreatePrivateChatRoomInternalAsync(string myEmail, string targetEmail)
        {
            return await ExecuteTransactionAsync(_chatRoomRepository, async () =>
            {
                ChatRoom? room = await _chatRoomRepository.CreateChatRoomAsync(null, null, false);
                if (room == null)
                    throw new InvalidOperationException("1대1 채팅방 생성에 실패했습니다.");
                List<string> participantList = new() { myEmail, targetEmail };
                bool particiResult = await _chatParticipantRepository.AddParticipantsToRoomAsync(room.Id, participantList, 0);
                if (!particiResult)
                    throw new InvalidOperationException("1대1 채팅방 참가자 등록에 실패했습니다.");
                return room;
            });
        }
        #endregion ChatRoom private 메서드
        #endregion ChatRoom 관련
        #region ChatParticipant 관련
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> RemoveParticipantAndCreateLeaveMessageAsync(Guid roomId, string userEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (roomId == Guid.Empty || string.IsNullOrEmpty(userEmail))
                    return ServiceResult<bool>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(roomId, userEmail, true);
                bool isGroupChat = participant.ChatRoom.IsGroupChat;
                // 3. 참가자 정보 삭제, 조건부 채팅방 삭제, 조건부 퇴장 메세지 등록을 transaction으로 실행하고,
                //    퇴장 메세지 전송을 위한 결과값 반환받음
                JoinAndLeaveChatRoomDTO transactionResult = await RemoveParticipantAndCreateMessageInternalAsync(roomId, participant);
                // 4. transactionResult.SystemMessage가 null이면 채팅방이 삭제된거라 메세지 전송 스킵
                if(transactionResult.SystemMessage != null && isGroupChat)
                {
                    // 5. 참가자들에게 퇴장 SystemMessage 전송
                    bool sendMessageResult = await SendJoinAndLeaveSystemMessageInternalAsync(transactionResult, false, userEmail);
                    if (!sendMessageResult)
                        throw new InvalidOperationException("SystemMessage 전송에 실패했습니다.");
                }
                // 6. 결과 반환
                return ServiceResult<bool>.Success(true);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> AddParticipantsToRoomAsync(Guid roomId, string myEmail, IEnumerable<string> emails)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if(roomId == Guid.Empty || string.IsNullOrEmpty(myEmail) || emails.Count() == 0)
                    return ServiceResult<bool>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(roomId, myEmail);
                // 3. 참가자 등록, 입장 메세지 생성하여 반환받음
                JoinAndLeaveChatRoomDTO transactionResult = await AddParticipantsAndCreateMessageInternalAsync(roomId, myEmail, emails);
                if (transactionResult.SystemMessage == null)
                    throw new InvalidOperationException("SystemMessage가 누락됐습니다.");
                // 4. 참가자들에게 입장 SystemMessage 전송
                bool sendMessageResult = await SendJoinAndLeaveSystemMessageInternalAsync(transactionResult, true);
                if (!sendMessageResult)
                    throw new InvalidOperationException("SystemMessage 전송에 실패했습니다.");
                // 6. 결과 반환
                return ServiceResult<bool>.Success(true);
            });
        }
        /// <summary>
        /// 누군가 채팅방에 입장하거나 퇴장하면 SystemMessage를 생성하여 채팅방 참가자들에게 전송합니다.
        /// </summary>
        /// <remarks>
        /// 퇴장 이벤트의 경우, 이미 방을 나간 탈퇴자에게는 SignalR 이벤트를 전송하지 않도록 전송 대상 목록에서 제외합니다.
        /// </remarks>
        /// <param name="dto">입퇴장 트랜잭션 결과가 담긴 DTO</param>
        /// <param name="isJoined">입장시 true, 퇴장시 false</param>
        /// <param name="leaverEmail">퇴장한 User의 이메일(퇴장 이벤트일때 필수 입력해야함)</param>
        /// <returns>입퇴장 시스템 메세지 전송 성공 여부</returns>
        private async Task<bool> SendJoinAndLeaveSystemMessageInternalAsync(JoinAndLeaveChatRoomDTO dto, bool isJoined, string? leaverEmail = null)
        {
            // 1. 입력 값 검증
            if (dto.RoomId == Guid.Empty || dto.SystemMessage == null)
                throw new InvalidOperationException("JoinAndLeaveChatRoomDTO의 값이 정상적으로 설정되지 않았습니다.");
            // 1. 참가자들에게 전송할 response 생성을 위한 Data 세팅
            ChatMessageResponse msgResponse = ChatMapper.ToSystemMessageResponse(dto.SystemMessage);
            List<FriendResponse> userResponse;
            // 2. 누군가 입장했을때
            if (isJoined)
            {
                // 3. _chatHubContext를 통해 전송할 response 생성을 위해 데이터 추출
                List<ChatParticipantProjection> projections = await _chatParticipantRepository.GetParticipantProjectionListAsync(dto.RoomId);
                userResponse = projections
                    .Where(p => dto.RemainingUsersEmailList.Contains(p.User.Email))
                    .Select(p => FriendMapper.MapToFriendResponse(p.User))
                    .ToList();
            }
            // 4. 누군가 퇴장했을때
            else
            {
                // 5. _chatHubContext를 통해 전송할 response 생성을 위해 데이터 추출
                if (leaverEmail == null || string.IsNullOrEmpty(leaverEmail))
                    throw new InvalidOperationException("leaverEmail이 정상적으로 넘어오지 않았습니다.");
                User? user = await _userRepository.GetUserByEmailAsync(leaverEmail);
                if(user == null)
                    throw new InvalidOperationException("User 객체를 찾지못했습니다.");
                userResponse = new() { FriendMapper.MapToFriendResponse(user) };
            }
            // 6. ChatHub를 통해 채팅방 참가자들에게 전송
            ChatParticipantStatusResponse response = 
                ChatMapper.ToParticipantStatusResponse(msgResponse, dto.RemainingUsersEmailList.Count, userResponse, isJoined);
            await BroadcastToUsersAsync(_chatHubContext, dto.RemainingUsersEmailList, ChatHubEvents.ChatHubResponseEvent.UpdateParticipantStatus, response);
            return true;
        }
        #region ChatParticipant private 메서드
        /// <summary>
        /// 원자성을 보장하기위해 transaction을 사용해 ChatParticipant 삭제와 퇴장 SystemMessage 등록을 진행합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="participant">채팅방 탈퇴자의 ChatParticipant Entity</param>
        /// <returns>JoinAndLeaveRoom DTO</returns>
        private async Task<JoinAndLeaveChatRoomDTO> RemoveParticipantAndCreateMessageInternalAsync(Guid roomId, ChatParticipant participant)
        {
            // 1. 내부에서 필요한 데이터 미리 선언
            JoinAndLeaveChatRoomDTO result = new() { RoomId = roomId };
            string nickname = participant.User.Nickname;
            bool isGroupChat = participant.ChatRoom.IsGroupChat;
            return await ExecuteTransactionAsync(_chatParticipantRepository, async () =>
            {
                // 2. 사용자 요청에 따라 Db에서 ChatParticipant 수정
                bool removeParticipantResult = await _chatParticipantRepository.UpdateChatParticipantAsync(participant, p =>
                {
                    p.IsLeft = true;
                });
                if (!removeParticipantResult)
                    throw new InvalidOperationException("참가자 채팅방 탈퇴 처리 중 오류가 발생했습니다.");
                // 3. 채팅방에 남아있는 사람들의 Email과 Nickname 추출하고, Email만 result에 삽입
                List<ChatParticipantDTO> participantDTO = await _chatParticipantRepository.GetParticipantDTOListAsync(roomId);
                result.RemainingUsersEmailList = participantDTO.Select(p => p.Email).ToList();
                // 4. 채팅방에 아무도 없으면 채팅방 제거
                if (participantDTO.Count == 0)
                {
                    bool removeRoomResult = await _chatRoomRepository.RemoveRoomAsync(participant.ChatRoom);
                    if (!removeRoomResult)
                        throw new InvalidOperationException("채팅방 삭제 중 오류가 발생했습니다.");
                }
                // 5. 누군가 남아있고, 그룹 채팅이면 퇴장 메세지 등록 후 ChatHub를 통해 전송
                else if (participantDTO.Count > 0 && isGroupChat)
                {
                    // 4-1. 퇴장 메세지 등록
                    ChatMessage exitMessageResult = await AddJoinAndExitSystemMessageAsync([nickname], roomId, false);
                    // 4-2. 등록된 메세지 result에 삽입
                    result.SystemMessage = exitMessageResult;
                }
                return result;
            });
        }
        /// <summary>
        /// 원자성 보장을 위하여 transaction을 사용해 참가자들을 채팅방에 초대하고, 입장 메세지를 등록한 뒤 JoinAndLeaveChatRoomDTO를 반환해줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">참가자들을 초대한 User의 Email</param>
        /// <param name="emails">초대 대상들의 Email</param>
        /// <returns>JoinAndLeaveChatRoomDTO DTO</returns>
        private async Task<JoinAndLeaveChatRoomDTO> AddParticipantsAndCreateMessageInternalAsync(Guid roomId, string userEmail, IEnumerable<string> emails)
        {
            // 1. 내부에서 필요한 데이터 미리 선언
            JoinAndLeaveChatRoomDTO result = new() { RoomId = roomId };
            return await ExecuteTransactionAsync(_chatParticipantRepository, async () =>
            {
                // 2. 입장 메세지 등록을 위해 참가자들의 Nickname 요청
                List<string> nicknameResult = await _userRepository.GetNicknamesByEmailsAsync(emails);
                if (nicknameResult.Count == 0)
                    throw new InvalidOperationException("등록된 채팅방 참가자를 찾을 수 없습니다.");
                // 3. 입장 메세지 등록
                ChatMessage messageResult = await AddJoinAndExitSystemMessageAsync(nicknameResult, roomId, true);
                result.SystemMessage = messageResult;
                // 4. 참가자들 등록 시도
                bool addResult = await _chatParticipantRepository.AddParticipantsToRoomAsync(roomId, emails, messageResult.Id);
                if (!addResult)
                    throw new InvalidOperationException("채팅방 참가자 등록 중 오류가 발생했습니다.");
                // 5. 참가자들 Email result에 삽입
                List<ChatParticipantDTO> particiDTO = await _chatParticipantRepository.GetParticipantDTOListAsync(roomId);
                List<string> particiEmailList = particiDTO.Select(p => p.Email).ToList();
                result.RemainingUsersEmailList = particiEmailList;
                // 6. result 반환
                return result;
            });
        }
        /// <summary>
        /// 유저가 해당 채팅방에서 나간 상태(IsLeft)라면 참여 상태로 변경하고 진입 시점 메세지 식별 번호를 갱신합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">채팅방 참가하려는 User의 이메일</param>
        /// <returns></returns>
        private async Task EnsureParticipantActiveAsync(Guid roomId, string userEmail)
        {
            ChatParticipant? participant = await _chatParticipantRepository.GetTrackingParticipantEntityAsync(roomId, userEmail);
            if (participant != null && participant.IsLeft)
            {
                long lastMessageId = await _chatMessageRepository.GetLastMessageIdAsync(roomId);
                long entryMessageId = lastMessageId;
                // 1대1 채팅방의 경우 입퇴장 메세지가 없기때문에 entryMessageId는 LastMessageId보다 1 커야함
                if(!participant.ChatRoom.IsGroupChat)
                    entryMessageId += 1;
                await _chatParticipantRepository.UpdateChatParticipantAsync(participant, p =>
                {
                    p.IsLeft = false;
                    p.EntryMessageId = entryMessageId;
                    p.LastReadMessageId = lastMessageId;
                });
            }
        }
        #endregion ChatParticipant private 메서드
        #endregion ChatParticipant 관련
        #region ChatMessage 관련
        /// <inheritdoc/>
        public async Task<ServiceResult<UserReadUpdateResponse>> UpdateLastReadedMessageAsync(string myEmail, UpdateLastReadedMessageRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if(string.IsNullOrEmpty(myEmail) || request.RoomId == Guid.Empty || request.LastReadMessageId < 0)
                    return ServiceResult<UserReadUpdateResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(request.RoomId, myEmail, true);
                // 3. 이전에 마지막으로 읽었던 메세지 번호 저장
                long previouseId = participant.LastReadMessageId;
                // 4. 내 ChatParticipant Entity의 값을 수정하고 Db에 적용 요청
                bool isSuccess = await _chatParticipantRepository.UpdateChatParticipantAsync(participant, p =>
                {
                    Console.WriteLine($"전달받은 LastReadMessageId: {request.LastReadMessageId}, Db에 기록된 LastReadMessageId: {p.LastReadMessageId}");
                    p.LastReadMessageId = request.LastReadMessageId;
                });
                if (!isSuccess)
                    return ServiceResult<UserReadUpdateResponse>.Failed("변경 사항이 없거나, 저장에 실패했습니다.", ServiceResultType.InternalServerError);
                // 4. 결과 Data들 Response로 매핑
                UserReadUpdateResponse response = ChatMapper.ToReadUpdateResponse(request.RoomId, myEmail, request.LastReadMessageId, previouseId);
                // 5. 업데이트 성공했으면 ChatHub를 통해 내가 메세지를 읽었으니 View 업데이트하라고 브로드 캐스트 전송
                await BroadcastToRoomAsync(_chatHubContext, request.RoomId.ToString(), ChatHubEvents.ChatHubResponseEvent.UserReadMessage, response);
                // 6. Response 반환
                return ServiceResult<UserReadUpdateResponse>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<ChatMessageResponse>> SendMessageAsync(string myEmail, SendMessageRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검증
                if (string.IsNullOrEmpty(myEmail) || request.RoomId == Guid.Empty || string.IsNullOrEmpty(request.Content))
                    return ServiceResult<ChatMessageResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 채팅방 접근 권한 확인
                ChatParticipant participant = await GetValidatedParticipantAsync(request.RoomId, myEmail, true);
                // 3. 메세지 전송하고 나의 LastReadedMessageId 업데이트
                ChatMessage messageResult = await AddMessageAndUpdateLastReadedMessageInternalAsync(myEmail, request, participant);
                // 4. 메세지 전송을 위해 채팅방의 모든 참가자 정보 받아와서 Email 추출
                List<ChatParticipantDTO> participantDTO = await _chatParticipantRepository.GetParticipantDTOListAsync(request.RoomId);
                if (participantDTO.Count == 0)
                    return ServiceResult<ChatMessageResponse>.Failed("채팅방 참가자 정보를 받아오는데 실패했습니다.", ServiceResultType.InternalServerError);
                List<string> emails = participantDTO.Select(p => p.Email).ToList();
                // 5. 결과 Data들 Response로 매핑하여 반환
                ChatMessageResponse response = ChatMapper.ToMessageResponse(messageResult, participant.User, participantDTO.Count - 1);
                // 6. ChatHub를 통해 참가자들에게 메세지 전송하고 Response 반환
                await BroadcastToUsersAsync(_chatHubContext, emails, ChatHubEvents.ChatHubResponseEvent.ReceiveMessage, response);
                return ServiceResult<ChatMessageResponse>.Success(response);
            });
        }
        #region ChatMessage private 메서드
        /// <summary>
        /// 원자성을 보장하기위해 transaction을 사용해 메세지를 Db에 등록하고, 작성자의 LastReadedMessageId를 신규 메세지 Id로 변경합니다.
        /// </summary>
        /// <param name="myEmail">메세지 작성자의 Email</param>
        /// <param name="request">메세지 정보가 담긴 Request</param>
        /// <param name="myParticipant">메세지 작성자의 ChatParticipant Entity</param>
        /// <returns>등록한 ChatMessage Entity</returns>
        private async Task<ChatMessage> AddMessageAndUpdateLastReadedMessageInternalAsync(string myEmail, SendMessageRequest request,
            ChatParticipant myParticipant)
        {
            return await ExecuteTransactionAsync(_chatMessageRepository, async () =>
            {
                // 1. 사용자 요청에따라 Db에 메세지 등록
                ChatMessage? messageResult = await _chatMessageRepository.AddMessageAsnyc(myEmail, request);
                if (messageResult == null)
                    throw new InvalidOperationException("메세지 저장 중 오류가 발생했습니다.");
                // 2. 1대1 채팅이면 상대방의 IsLeft와 상관없이 메세지를 전송할수있게 검사 로직 실행
                if(!myParticipant.ChatRoom.IsGroupChat)
                {
                    await _chatParticipantRepository.ActivateParticipantIsLeftStatusIfPrivateAsync(request.RoomId, myEmail, messageResult.Id);
                }
                // 3. 메세지가 성공적으로 등록됐으면 LastReadMessagId를 업데이트해줘야함
                bool isUpdated = await _chatParticipantRepository.UpdateChatParticipantAsync(myParticipant, p =>
                {
                    p.LastReadMessageId = messageResult.Id;
                });
                if (!isUpdated)
                    throw new InvalidOperationException("메세지 등록 후 LastReadMessageId 수정에 실패했습니다.");
                // 4. 저장된 ChatMessage를 ServiceResult에 담아서 반환
                return messageResult;
            });
        }
        /// <summary>
        /// 채팅방에 시스템 메시지(입장/퇴장 등)를 등록합니다.
        /// </summary>
        /// <remarks>
        /// 해당 메서드는 ChatMessage 객체를 찾지 못할시 throw를 던지기때문에<br/>
        /// 반드시 try-catch문을 사용하는 BaseBusinessService의 ExecutedBusinessLogicAsync 메서드 내부에서 실행되어야합니다.
        /// </remarks>
        /// <param name="nicknames">메세지에 표시될 유저들의 Nickname List</param>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="isJoin">ture: 입장 메세지, false: 퇴장 메세지</param>
        /// <returns>등록된 ChatMessage 객체, 실패 시 null</returns>
        private async Task<ChatMessage> AddJoinAndExitSystemMessageAsync(IEnumerable<string> nicknames, Guid roomId, bool isJoin)
        {
            // 1. 입장 or 퇴장 SystemMessage 객체 생성
            SendMessageRequest tempReq = new()
            {
                RoomId = roomId,
                MessageType = ChatMessageType.System,
                Content = ChatMapper.CreateSystemMessagesContent(nicknames, isJoin)
            };
            // 2. 메세지 Db에 등록
            ChatMessage? messageResult = await _chatMessageRepository.AddMessageAsnyc(null, tempReq);
            if (messageResult == null)
                throw new InvalidOperationException("메세지 등록에 실패했습니다.");
            // 3. 처리 결과 반환
            return messageResult;
        }
        #endregion ChatMessage private 메서드
        #endregion ChatMessage 관련
    }
}
