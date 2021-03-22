using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Enums;

namespace ScriptAtRestServer.Services
{
    public interface IScriptExecutionService
    {
        Task<ProcessModel> RunScript(ScriptEnums.ScriptType Type, string Name, string Parameters);
        Task<ProcessModel> RunScriptById(Script Script);
    };

    public class ScriptExecutionService : IScriptExecutionService
    {

        public async Task<ProcessModel> RunScriptById(Script Script)
        {
            return await Task.Run(() =>
            {
                //load script content
                string scriptContent = Script.Content;
                //decide script type -> script suffix
                ScriptEnums.ScriptType scriptType = Script.Type;

                string scriptSuffix = string.Empty;
                switch (scriptType)
                {
                    case ScriptEnums.ScriptType.Shell:
                        scriptSuffix = ".cmd";
                        break;
                    case ScriptEnums.ScriptType.PowerShell:
                        scriptSuffix = ".ps1";
                        break;
                    default:
                        scriptSuffix = ".cmd";
                        break;
                }

                //save script content to temporary file
                //this automatically creates temporary empty file with unique name and returns file path
                string scriptFilePath = CreateScriptFileWithContent(scriptContent, scriptSuffix);


                //execute
                return new ProcessModel { };
            });
        }

        private static string CreateScriptFileWithContent(string ScriptContent, string ScriptSuffix)
        {
            string tempFilePath = Path.GetTempFileName();
            tempFilePath = Path.ChangeExtension(tempFilePath, ScriptSuffix);
            File.WriteAllText(tempFilePath, ScriptContent);
            return tempFilePath;
        }

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

        private static Process CreateProcess(string processArgs)
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