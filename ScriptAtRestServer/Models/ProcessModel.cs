using System;

namespace ScriptAtRestServer.Services
{
    public class ProcessModel
    {
        public Int32 ExitCode { get; set; }
        public string Output { get; set; }
        public string ErrorOutput { get; set; }
    }
}