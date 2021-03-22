using ScriptAtRestServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class ScriptModel
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public ScriptEnums.ScriptType Type { get; set; }
    }
}