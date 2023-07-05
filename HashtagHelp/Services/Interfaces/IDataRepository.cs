using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IDataRepository
    {
        void AddGeneralTask(GeneralTaskEntity task);

        void UpdateGeneralTask(GeneralTaskEntity generalTaskEntity);

        void AddUser(UserEntity user);

        void AddHashtag(HashtagEntity hashtag);

        Task SaveChangesAsync();

        bool DoesFieldExist(string tableName, string fieldName);

        Task<TEntity> GetEntityByFieldValueAsync<TEntity>(string tableName, string fieldName, string fieldValue) where TEntity : class;
    }
}
