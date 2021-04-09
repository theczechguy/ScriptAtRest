using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ScriptAtRestServer.Services
{

    public interface IScriptService {
        Task<Script> CreateAsync(Script Script);
        IEnumerable<Script> GetAll();
        Task<Script> GetByIdAsync(int Id);
        Task DeleteAsync(int id);

        Task<ScriptType> CreateTypeAsync(ScriptType ScriptType);
        Task<ScriptType> UpdateTypeAsync(int Id);
        IEnumerable<ScriptType> GetAllTypes();
        Task<ScriptType> GetTypeByIdAsync(int Id);
        Task<ScriptType> UpdateTypeById(int Id, ScriptType UpdatedType);
        Task DeleteTypeAsync(int Id);
    }
    public class ScriptService : IScriptService
    {
        private SqLiteDataContext _context;
        public ScriptService(SqLiteDataContext Context)
        {
            _context = Context;
        }
        public async Task<Script> CreateAsync(Script Script)
        {
            if (await _context.Scripts.AnyAsync(x => x.Name == Script.Name))
            {
                throw new AppException("Scriptname is already taken");
            }

            if (await _context.ScriptTypes.FindAsync(Script.Type) == null)
            {
                throw new AppException("Script type is not registered");
            }

            _ = _context.Scripts.Add(Script);
            _ = await _context.SaveChangesAsync();

            return Script;
        }

        public async Task UpdateAsync(Script Script) {
            var script = await _context.Scripts.FindAsync(Script.Id);
            if (script == null)
            {
                throw new AppException("Script not found");
            }
            script.Content = Script.Content;
            script.Name = Script.Name;

            _ =_context.Scripts.Update(script);
            _ = await _context.SaveChangesAsync();
        }

        public IEnumerable<Script> GetAll()
        {
            return _context.Scripts;
        }

        public async Task<Script> GetByIdAsync(int Id) {
            //return _context.Scripts.Find(Id);

            Script type = await _context.Scripts.FindAsync(Id);
            if (type == null)
            {
                throw new AppException("Script type with specified id not found");
            }
            return type;
        }

        public async Task DeleteAsync(int Id)
        {
            Script script = await _context.Scripts.FindAsync(Id);
            if (script != null)
            {
                _ = _context.Scripts.Remove(script);
                _ = await _context.SaveChangesAsync();
            }
            else
            {
                throw new AppException("Script with requested id not found");
            }
        }

        public async Task<ScriptType> CreateTypeAsync(ScriptType ScriptType) {
            if (await _context.ScriptTypes.AnyAsync(x => x.Name == ScriptType.Name))
            {
                throw new AppException("Scriptname is already taken");
            }

            _context.ScriptTypes.Add(ScriptType);
            _ = await _context.SaveChangesAsync();

            return ScriptType;
        }

        public Task<ScriptType> UpdateTypeAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTypeAsync(int Id)
        {
            ScriptType type = await _context.ScriptTypes.FindAsync(Id);
            if (type != null)
            {
                _ = _context.ScriptTypes.Remove(type);
                _ = await _context.SaveChangesAsync();
            }
            else 
            {
                throw new AppException("Script type with requested id not found");
            }
        }

        public IEnumerable<ScriptType> GetAllTypes()
        {
            return _context.ScriptTypes;
        }

        public async Task<ScriptType> GetTypeByIdAsync(int Id)
        {
            ScriptType type = await _context.ScriptTypes.FindAsync(Id);
            if (type == null)
            {
                throw new AppException("Script type with specified id not found");
            }
            return type;
        }

        public async Task<ScriptType> UpdateTypeById(int Id, ScriptType UpdatedType)
        {
            ScriptType currentType = await _context.ScriptTypes.FindAsync(Id);
            if (currentType == null)
            {
                throw new AppException($"Script type with id {Id} not found in database");
            }

            if (string.IsNullOrWhiteSpace(UpdatedType.FileExtension))
            {
                throw new AppException("FileExtension field must not be empty !");
            }

            if (string.IsNullOrWhiteSpace(UpdatedType.Name))
            {
                throw new AppException("Name field must not be empty !");
            }

            if (string.IsNullOrWhiteSpace(UpdatedType.Runner))
            {
                throw new AppException("Runner field must not be empty !");
            }

            if (string.IsNullOrWhiteSpace(UpdatedType.ScriptArgument))
            {
                throw new AppException("ScriptArgument field must not be empty !");
            }
            currentType.ScriptArgument = UpdatedType.ScriptArgument;
            currentType.Runner = UpdatedType.Runner;
            currentType.FileExtension = UpdatedType.FileExtension;

            if (currentType.Name != UpdatedType.Name)
            {
                if (await _context.ScriptTypes.AnyAsync(x => x.Name == UpdatedType.Name))
                {
                    throw new AppException("New script type name is already taken !");
                }
                currentType.Name = UpdatedType.Name;
            }

            _context.ScriptTypes.Update(currentType);
            _ = await _context.SaveChangesAsync();

            return currentType;
        }
    }
}