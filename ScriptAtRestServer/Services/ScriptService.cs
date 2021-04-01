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
        Task<Script> Create(Script Script);
        IEnumerable<Script> GetAll();
        Script GetById(int id);
        void Delete(int id);
    }
    public class ScriptService : IScriptService
    {
        private SqLiteDataContext _context;
        public ScriptService(SqLiteDataContext Context)
        {
            _context = Context;
        }
        public async Task<Script> Create(Script Script)
        {
            if (await _context.Scripts.AnyAsync(x => x.Name == Script.Name))
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
    }
}