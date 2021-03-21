using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Helpers;

namespace ScriptAtRestServer.Services
{

    public interface IScriptService {
        Script Create(string Name , string Content);
    }
    public class ScriptService : IScriptService
    {
        private SqLiteDataContext _context;
        public ScriptService(SqLiteDataContext Context)
        {
            _context = Context;
        }
        public Script Create(string Name , string Content)
        {
            throw new NotImplementedException();
        }
    }
}