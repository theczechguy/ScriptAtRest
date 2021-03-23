using ScriptAtRestServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class RegisterScriptModel
    {
        [Required]
        public string Name { get; set; }
        public string Content { get; set; }
        [Required]
        public ScriptEnums.ScriptType Type { get; set; }
    }
}