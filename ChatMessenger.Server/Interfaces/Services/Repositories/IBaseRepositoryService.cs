using Microsoft.EntityFrameworkCore.Storage;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    public interface IBaseRepositoryService
    {
        /// <summary>
        /// 외부에서 Transaction을 사용할 수 있도록 Transaction 시작 기능을 제공합니다.
        /// </summary>
        /// <returns></returns>
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
