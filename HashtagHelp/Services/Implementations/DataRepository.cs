﻿using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HashtagHelp.Services.Implementations
{
    public class DataRepository : IDataRepository
    {
        private readonly AppDbContext _context;

        public DataRepository(AppDbContext context)
        {
            _context = context;
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
    }
}
