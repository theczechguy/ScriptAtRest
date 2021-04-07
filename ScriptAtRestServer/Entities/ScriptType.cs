using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Entities
{
    public class ScriptType
    {
        public string TypeName { get; set; }
        public string Runner { get; set; }
        public string FileExtension { get; set; }
        public string ScriptArgument { get; set; }
    }
}