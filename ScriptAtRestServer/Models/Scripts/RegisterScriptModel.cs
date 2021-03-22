using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class RegisterScriptModel
    {
        [Required]
        public string Name { get; set; }
        public string Content { get; set; }
    }
}