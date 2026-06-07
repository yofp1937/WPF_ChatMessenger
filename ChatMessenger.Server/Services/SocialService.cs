using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services
{
    /// <summary>
    /// FriendController의 요청에따라 친구와 관련된 요청(검색, 추가, 상태 변경 등)을 처리해주는 Service입니다.
    /// </summary>
    public class SocialService : BaseBusinessService, ISocialService
    {
        private IFriendshipRepository _friendshipRepository;
        private IUserRepositoryService _userRepository;

        public SocialService(IFriendshipRepository friendshipRepository, IUserRepositoryService userService)
        {
            _friendshipRepository = friendshipRepository;
            _userRepository = userService;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<ServiceResult<List<FriendResponse>>> GetFriendResponseListAsync(string myEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail))
                    return ServiceResult<List<FriendResponse>>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 친구로 추가한 유저들의 FriendResponse 추출
                //    아이디를 이제 생성한 경우 FriendResponse가 0일 수 있음
                List<FriendResponse> result = await _friendshipRepository.GetFriendResponseListAsync(myEmail);
                // 3. 처리 결과 반환
                return ServiceResult<List<FriendResponse>>.Success(result);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<FriendResponse>> GetFriendResponseAsync(string myEmail, string friendEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(friendEmail))
                    return ServiceResult<FriendResponse>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 친구 관계 확인
                Friendship? friendship = await _friendshipRepository.GetFriendshipEntityAsync(myEmail, friendEmail);
                if (friendship == null)
                    return ServiceResult<FriendResponse>.Failed("해당 유저와 친구 관계가 아닙니다.", ServiceResultType.BadRequest);
                // 3. FriendResponse로 매핑하여 반환
                FriendResponse response = friendship.Friend.MapToFriendResponse(friendship, myEmail == friendEmail);
                return ServiceResult<FriendResponse>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<FriendResponse>> AddFriendAsync(string myEmail, string friendEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(friendEmail))
                    return ServiceResult<FriendResponse>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 이미 등록된 친구 관계 있는지 확인
                Friendship? friendship = await _friendshipRepository.GetFriendshipEntityAsync(myEmail, friendEmail);
                if (friendship != null)
                    return ServiceResult<FriendResponse>.Failed("이미 추가된 친구입니다.", ServiceResultType.BadRequest);
                // 3. 친구 추가 진행
                bool isSuccess = await _friendshipRepository.AddFriendshipEntityAsync(myEmail, friendEmail);
                if (!isSuccess)
                    return ServiceResult<FriendResponse>.Failed("친구 추가에 실패했습니다.", ServiceResultType.InternalServerError);
                // 4. 등록한 객체 찾아서 반환
                return await GetFriendResponseAsync(myEmail, friendEmail);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> DeleteFriendAsync(string myEmail, string friendEmail)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(friendEmail))
                    return ServiceResult<bool>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 친구로 등록 돼있는지 확인
                Friendship? friendship = await _friendshipRepository.GetFriendshipEntityAsync(myEmail, friendEmail);
                if (friendship == null)
                    return ServiceResult<bool>.Failed("친구 데이터를 찾을 수 없습니다.", ServiceResultType.NotFound);
                // 3. 친구 삭제 시도
                bool isRemoved = await _friendshipRepository.RemoveFriendshipEntityAsync(friendship);
                if (!isRemoved)
                    return ServiceResult<bool>.Failed("친구 삭제에 실패했습니다.", ServiceResultType.InternalServerError);
                return ServiceResult<bool>.Success(isRemoved);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> UpdateFavoriteAsync(string myEmail, FriendStatusRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(request.Email))
                    return ServiceResult<bool>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 등록된 친구가 맞는지 확인
                Friendship? friendship = await _friendshipRepository.GetFriendshipEntityAsync(myEmail, request.Email);
                if (friendship == null)
                    return ServiceResult<bool>.Failed("친구 데이터를 찾을 수 없습니다.", ServiceResultType.NotFound);
                // 3. 즐겨찾기 변경 성공했으면 return
                bool isUpdated = await _friendshipRepository.UpdateFriendshipAsync(friendship, f =>
                {
                    f.IsFavorite = request.IsFavorite;
                });
                if (!isUpdated)
                    return ServiceResult<bool>.Failed("즐겨찾기 변경에 실패했습니다.", ServiceResultType.InternalServerError);
                return ServiceResult<bool>.Success(isUpdated);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> UpdateBlockAsync(string myEmail, FriendStatusRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 입력 값 검사
                if (string.IsNullOrEmpty(myEmail) || string.IsNullOrEmpty(request.Email))
                    return ServiceResult<bool>.Failed("잘못된 요청 데이터입니다.", ServiceResultType.BadRequest);
                // 2. 등록된 친구인지 확인
                Friendship? friendship = await _friendshipRepository.GetFriendshipEntityAsync(myEmail, request.Email);
                // 3. 조건에 따라 처리
                bool result;
                // [분기 1]: 상대와 친구 관계가 아닌 경우
                if(friendship == null)
                {
                    // 모르는 대상을 차단하려하는 경우 차단 상태의 새로운 관계 생성
                    if (request.IsBlocked)
                        result = await _friendshipRepository.AddFriendshipEntityAsync(myEmail, request.Email, false, true);
                    // Friendship Entity가 없을땐 차단 해제가 불가능하니 failed
                    else
                        return ServiceResult<bool>.Failed("친구 데이터를 찾을 수 없습니다.", ServiceResultType.NotFound);
                }
                // [분기 2]: 등록된 Friendship이 있는 경우
                else
                {
                    // 친구를 차단하려는 경우 즐겨찾기, 차단 상태 변경
                    if(request.IsBlocked)
                        result = await _friendshipRepository.UpdateFriendshipAsync(friendship, f =>
                        {
                            f.IsBlocked = true;
                            f.IsFavorite = false;
                        });
                    // 차단 해제하면 Friendship 자체를 삭제
                    else
                        result = await _friendshipRepository.RemoveFriendshipEntityAsync(friendship);
                }
                if (!result)
                    return ServiceResult<bool>.Failed("차단 상태 변경에 실패했습니다.", ServiceResultType.InternalServerError);
                return ServiceResult<bool>.Success(result);
            });
        }
        #endregion public Method
    }
}
