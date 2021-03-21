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
            if (_context.Scripts.Any(x => x.Name == x.Name))
            {
                throw new AppException("Scriptname is already taken");
            }

            _context.Scripts.Add(Script);
            _context.SaveChanges();

            return Script;
        }

        public void Update() { }
        public void Delete(string Name) {
            var script = _context.Scripts.Find(Name);
            if (script != null)
            {
                _context.Scripts.Remove(script);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Script> GetAll()
        {
            return _context.Scripts;
        }
    }
}