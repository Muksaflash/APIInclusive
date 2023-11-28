using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IDataRepository
    {
        void AddGeneralTask(GeneralTaskEntity generalTask);

        void UpdateGeneralTask(GeneralTaskEntity generalTask);
        
        void AddParserTask(ParserTaskEntity task);

        void UpdateParserTask(ParserTaskEntity task);

        void AddUser(UserEntity user);

        void UpdateUser(UserEntity user);

        void AddHashtag(HashtagEntity hashtag);

        void UpdateHashtag(HashtagEntity hashtag);

        Task SaveChangesAsync();

        bool DoesFieldExist(string tableName, string fieldName);
        
        bool DoesHashtagExist(string hashtagName);

        Task<TEntity> GetEntityByFieldValueAsync<TEntity>(string tableName, string fieldName, string fieldValue) where TEntity : class;

        Task CheckAndDeleteOldRecordsAsync();

        IQueryable<GeneralTaskEntity> GetNotCompletedGeneralTasks();

        IQueryable<GeneralTaskEntity> GetGeneralTaskEntities();

        GeneralTaskEntity GetGeneralTaskEntityById(string generalTaskId);
    }
}
