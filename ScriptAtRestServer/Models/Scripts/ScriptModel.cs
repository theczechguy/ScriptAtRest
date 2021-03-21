using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class ScriptModel
    {
        [Required]
        public string Name { get; set; }
        public string Content { get; set; }
    }
}