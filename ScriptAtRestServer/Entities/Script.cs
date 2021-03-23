using ScriptAtRestServer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Entities
{
    public class Script
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ScriptEnums.ScriptType Type { get; set; }
    }
}