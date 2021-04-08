using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScriptAtRestServer.Services
{

    public interface IScriptService {
        Script Create(Script Script);
        IEnumerable<Script> GetAll();
        Script GetById(int id);
        void Delete(int id);

        Task<ScriptType> CreateTypeAsync(ScriptType ScriptType);
        Task<ScriptType> UpdateTypeAsync(int Id);
        void DeleteType(int Id);
        IEnumerable<ScriptType> GetAllTypes();
        ScriptType GetTypeById(int Id);
    }
    public class ScriptService : IScriptService
    {
        private SqLiteDataContext _context;
        public ScriptService(SqLiteDataContext Context)
        {
            _context = Context;
        }
        public Script Create(Script Script)
        {
            if (_context.Scripts.Any(x => x.Name == Script.Name))
            {
                throw new AppException("Scriptname is already taken");
            }

            _context.Scripts.Add(Script);
            _context.SaveChanges();

            return Script;
        }

        public void Update(Script Script) {
            var script = _context.Scripts.Find(Script.Id);
            if (script == null)
            {
                throw new AppException("Script not found");
            }
            script.Content = Script.Content;
            script.Name = Script.Name;

            _context.Scripts.Update(script);
            _context.SaveChanges();
        }

        public void Delete(string Name) {
            var script = _context.Scripts.Find(Name);

            if (script == null)
            {
                throw new AppException("Script not found");
            }

            _context.Scripts.Remove(script);
            _context.SaveChanges();
        }

        public IEnumerable<Script> GetAll()
        {
            return _context.Scripts;
        }

        public Script GetById(int Id) {
            return _context.Scripts.Find(Id);
        }

        public void Delete(int Id)
        {
            Script script = _context.Scripts.Find(Id);
            if (script != null)
            {
                _context.Scripts.Remove(script);
                _context.SaveChanges();
            }
        }

        public async Task<ScriptType> CreateTypeAsync(ScriptType ScriptType) {
            if (_context.ScriptTypes.Any(x => x.Name == ScriptType.Name))
            {
                throw new AppException("Scriptname is already taken");
            }

            _context.ScriptTypes.Add(ScriptType);
            await _context.SaveChangesAsync();

            return ScriptType;
        }

        public Task<ScriptType> UpdateTypeAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public void DeleteType(int Id)
        {
            ScriptType type = _context.ScriptTypes.Find(Id);
            if (type != null)
            {
                _context.ScriptTypes.Remove(type);
                _context.SaveChanges();
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

        public ScriptType GetTypeById(int Id)
        {
            ScriptType type = _context.ScriptTypes.Find();
            if (type == null)
            {
                throw new AppException("Script type with specified id not found");
            }
            return type;
        }
    }
}