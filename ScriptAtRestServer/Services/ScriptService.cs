using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Helpers;

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

        public IEnumerable<Script> GetAll()
        {
            return _context.Scripts;
        }
    }
}