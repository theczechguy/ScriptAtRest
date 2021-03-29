using System.Collections.Generic;

namespace ScriptAtRestServer.Models.Scripts
{
    public class ScriptParamModel
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
    }

    public class ScriptParamArray { 
        public List<ScriptParamModel> Parameters { get; set; }
    }
}