using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Enums;
using ScriptAtRestServer.Helpers;
using ScriptAtRestServer.Models.Scripts;

namespace ScriptAtRestServer.Services
{
    public interface IScriptExecutionService
    {
        Task<ProcessModel> RunScript(ScriptEnums.ScriptType Type, string Name, string Parameters);
        Task<ProcessModel> RunScriptById(int id);
    };

    public class ScriptExecutionService : IScriptExecutionService
    {
        private SqLiteDataContext _context;
        public ScriptExecutionService(SqLiteDataContext Context) {
            _context = Context;
        }

        public async Task<ProcessModel> RunScriptById(int id)
        {
            return await Task.Run(() =>
            {
                Script script = _context.Scripts.Find(id);

                string scriptContent = script.Content;
                ScriptEnums.ScriptType scriptType = script.Type;
                string scriptSuffix, fileName;

                SelectScriptDetails(scriptType, out scriptSuffix, out fileName);


                //save script content to temporary file
                //this automatically creates temporary empty file with unique name and returns file path
                string scriptFilePath = CreateScriptFileWithContent(scriptContent, scriptSuffix);
                string processArgs = PrepareScriptArguments(scriptType, scriptFilePath);

                Process process = CreateProcess(processArgs, fileName);
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
        }

        public async Task<ProcessModel> RunScriptById(int id , ScriptParamArray paramArray)
        {
                Script script = _context.Scripts.Find(id);

                string scriptContent = script.Content;
                ScriptEnums.ScriptType scriptType = script.Type;
                
                string scriptSuffix, fileName;
                SelectScriptDetails(scriptType, out scriptSuffix, out fileName);

                //save script content to temporary file
                //this automatically creates temporary empty file with unique name and returns file path
                string scriptFilePath = CreateScriptFileWithContent(scriptContent, scriptSuffix);
                string processArgs = PrepareScriptArguments(scriptType, scriptFilePath);

                Process process = await CreateProcessAsync(processArgs, fileName);
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
        }


        #region helper methods
        private static string PrepareScriptArguments(ScriptEnums.ScriptType scriptType, string scriptFilePath)
        {
            string processArgs = string.Empty;

            switch (scriptType)
            {
                case ScriptEnums.ScriptType.Shell:
                    processArgs = $" /c {scriptFilePath}";
                    break;
                case ScriptEnums.ScriptType.PowerShell:
                    processArgs = $" -f {scriptFilePath}";
                    break;
            }

            return processArgs;
        }

        private static void SelectScriptDetails(ScriptEnums.ScriptType scriptType, out string scriptSuffix, out string fileName)
        {
            scriptSuffix = string.Empty;
            fileName = string.Empty;
            switch (scriptType)
            {
                case ScriptEnums.ScriptType.PowerShell:
                    scriptSuffix = ".ps1";
                    fileName = "powershell.exe";
                    break;
                default:
                    scriptSuffix = ".cmd";
                    fileName = "cmd.exe";
                    break;
            }
        }

        private static string CreateScriptFileWithContent(string ScriptContent, string ScriptSuffix)
        {
            string tempFilePath = Path.GetTempFileName();
            tempFilePath = Path.ChangeExtension(tempFilePath, ScriptSuffix);
            File.WriteAllText(tempFilePath, ScriptContent);
            return tempFilePath;
        }

        private static async Task<Process> CreateProcessAsync(string processArgs , string fileName)
        {
            return await Task.Run(() =>
            {
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Scripts"),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = fileName,
                        Arguments = processArgs
                    }
                };
                return process;
            });
        }

        private static Process CreateProcess(string processArgs, string fileName)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Scripts"),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = fileName,
                    Arguments = processArgs
                }
            };
        }
        #endregion

        #region obsolete

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
        #endregion
    }
}