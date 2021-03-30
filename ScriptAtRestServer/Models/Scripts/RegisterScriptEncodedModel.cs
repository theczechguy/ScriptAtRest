using ScriptAtRestServer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Models.Scripts
{
    public class RegisterScriptEncodedModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string EncodedContent { get; set; }
        
        [Required]
        public ScriptEnums.ScriptType Type { get; set; }
    }
}