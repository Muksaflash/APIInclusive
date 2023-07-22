using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IDataRepository
    {
        IGoogleApiRequestService GoogleApiRequestService { get; set; }

        void AddGeneralTask(GeneralTaskEntity generalTask);

        void UpdateGeneralTask(GeneralTaskEntity generalTask);
        
        void AddParserTask(ParserTaskEntity task);

        void UpdateParserTask(ParserTaskEntity task);

        void AddUser(UserEntity user);

        void UpdateUser(UserEntity user);

        void AddHashtag(HashtagEntity hashtag);

        Task SaveChangesAsync();

        bool DoesFieldExist(string tableName, string fieldName);

        Task<TEntity> GetEntityByFieldValueAsync<TEntity>(string tableName, string fieldName, string fieldValue) where TEntity : class;

        Task CheckAndDeleteOldRecordsAsync();
    }
}
