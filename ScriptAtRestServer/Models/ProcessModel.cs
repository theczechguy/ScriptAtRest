using System;

namespace ScriptAtRestServer.Services
{
    public class ProcessModel
    {
        public Int32 ReturnCode { get; set; }
        public string Output { get; set; }
        public string ErrorOutput { get; set; }
    }
}