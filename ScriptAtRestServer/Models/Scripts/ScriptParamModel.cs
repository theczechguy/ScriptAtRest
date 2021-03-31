using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.Scripts
{
    public class ScriptParamModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string EncodedValue { get; set; }
    }
}