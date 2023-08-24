using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HashtagHelp.Services.Implementations
{
    public class DataRepository : IDataRepository
    {

        private readonly AppDbContext _context;

        private readonly IGoogleApiRequestService? _googleApiRequestService;

        public DataRepository(AppDbContext? context, IGoogleApiRequestService? googleApiRequestService)
        {
            _context = context;
            _googleApiRequestService = googleApiRequestService;
        }

        public void AddGeneralTask(GeneralTaskEntity generalTask)
        {
            _context.GeneralTasks.Add(generalTask);
        }

        public void UpdateGeneralTask(GeneralTaskEntity generalTask)
        {
            _context.GeneralTasks.Update(generalTask);
        }

        public void AddParserTask(ParserTaskEntity task)
        {
            _context.Tasks.Add(task);
        }

        public void UpdateParserTask(ParserTaskEntity task)
        {
            _context.Tasks.Update(task);
        }

        public void AddUser(UserEntity user)
        {
            _context.Users.Add(user);
        }

        public void UpdateUser(UserEntity user)
        {
            _context.Users.Update(user);
        }

        public void AddHashtag(HashtagEntity hashtag)
        {
            _context.Hashtags.Add(hashtag);
        }

        public void UpdateHashtag(HashtagEntity hashtag)
        {
            _context.Hashtags.Update(hashtag);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public bool DoesFieldExist(string tableName, string fieldName)
        {
            var table = _context.Model.FindEntityType(tableName);
            if (table != null)
            {
                var property = table.FindProperty(fieldName);
                return property != null;
            }
            return false;
        }
        public bool DoesHashtagExist(string hashtagName)
        {
            return _context.Hashtags.Any(h => h.Name == hashtagName);
        }

        public async Task<TEntity> GetEntityByFieldValueAsync<TEntity>(string tableName, string fieldName, string fieldValue)
            where TEntity : class
        {
            var table = _context.Model.FindEntityType(tableName);
            if (table != null)
            {
                var property = table.FindProperty(fieldName);
                if (property != null)
                {
                    var parameter = Expression.Parameter(typeof(TEntity), "e");
                    var condition = Expression.Equal(Expression.Property(parameter, property.PropertyInfo), Expression.Constant(fieldValue));
                    var lambda = Expression.Lambda<Func<TEntity, bool>>(condition, parameter);
                    var entity = await _context.Set<TEntity>().FirstOrDefaultAsync(lambda);
                    if (entity == null)
                    {
                        throw new InvalidOperationException("Entity not found.");
                    }
                    return entity;
                }
            }
            throw new InvalidOperationException("Invalid table or field name.");
        }

        public async Task CheckAndDeleteOldRecordsAsync()
        {
            DateTime now = DateTime.Now;
            DateTime thresholdDate = now.AddDays(-30);
            var lastDeletionTime = DateTime.Parse(await _googleApiRequestService.GetParameterAsync("G2"));
            var pastAfterDeletionHours = now - lastDeletionTime;
            if (pastAfterDeletionHours > TimeSpan.FromHours(24))
            {
                await DeleteOldRecordsAsync(thresholdDate);
                await SetDeletionDateAsync();
            }
        }

        public IQueryable<GeneralTaskEntity> GetNotCompletedGeneralTasks()
        {
            return _context.GeneralTasks
                .Include(r => r.CollectionTask)
                    .ThenInclude(ct => ct.ResearchedUsers)
                .Include(r => r.FiltrationTask)
                .Include(r => r.User)
                .Where(r => r.Status != StatusTaskEnum.Error && r.Status != StatusTaskEnum.Filtrated);
        }

        private async Task SetDeletionDateAsync()
        {
            await _googleApiRequestService.SetParameterAsync("G2", DateTime.Now.ToString());
        }

        private async Task DeleteOldRecordsAsync(DateTime thresholdDate)
        {
            if (_context.Hashtags.Any())
            {
                var oldRecords = _context.Hashtags.Where(r => r.CreatedDate < thresholdDate);
                _context.Hashtags.RemoveRange(oldRecords);
                await _context.SaveChangesAsync();
            }
        }
    }
}
