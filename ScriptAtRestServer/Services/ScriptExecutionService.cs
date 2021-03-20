using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ScriptAtRestServer.Services
{
    public interface IScriptExecutionService
    {
        Task<ProcessModel> RunScript(ScriptEnums.ScriptType Type, string Name, string Parameters);
    };

    public class ScriptExecutionService : IScriptExecutionService
    {
        public async Task<ProcessModel> RunScript(ScriptEnums.ScriptType Type, string Name, string Parameters) => await Task.Run(() =>
        {
            string processArgs = string.Empty;
            switch (Type)
            {
                case ScriptEnums.ScriptType.Shell:
                    processArgs = $" {Name.TrimEnd()} {Parameters}";
                    break;
                case ScriptEnums.ScriptType.PowerShell:
                    processArgs = $" -f {Name.TrimEnd()} {Parameters}";
                    break;
            }

            Process process = CreateProcess(Type, processArgs);
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return new ProcessModel
            {
                ExitCode = process.ExitCode,
                Output = output,
                ErrorOutput = errorOutput
            };
        });

        private static Process CreateProcess(ScriptEnums.ScriptType Type, string processArgs)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Scripts"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = Type == ScriptEnums.ScriptType.PowerShell ? "powershell.exe" : "cmd /c",
                    Arguments = processArgs
                }
            };
        }
    }
}