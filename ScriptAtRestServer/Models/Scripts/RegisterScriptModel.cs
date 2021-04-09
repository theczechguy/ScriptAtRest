using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Enums;
using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class RegisterScriptModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Type { get; set; }

        [Required]
        public string EncodedContent { get; set; }
    }
}