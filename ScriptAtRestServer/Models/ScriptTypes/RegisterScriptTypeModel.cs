using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ScriptAtRestServer.Models.ScriptTypes
{
    public class RegisterScriptTypeModel
    {
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Runner { get; set; }
        
        [Required]
        public string FileExtension { get; set; }
        
        public string ScriptArgument { get; set; }
    }
}