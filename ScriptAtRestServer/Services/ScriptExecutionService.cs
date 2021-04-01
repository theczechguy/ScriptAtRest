using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        Task<ProcessModel> RunScriptById(int id, ScriptParamArray paramArray);
    };

    public class ScriptExecutionService : IScriptExecutionService
    {
        private ILogger<ScriptExecutionService> _logger;
        private SqLiteDataContext _context;
        public ScriptExecutionService(SqLiteDataContext Context ,ILogger<ScriptExecutionService> Logger) {
            _context = Context;
            _logger = Logger;
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
                
            SelectScriptDetails(scriptType, out string scriptSuffix, out string fileName);

            string scriptFilePath = CreateScriptFileWithContent(scriptContent, scriptSuffix);
            _logger.LogDebug("Script file : {fullPath}" , scriptFilePath);
            string processArgs = PrepareScriptArguments(scriptType, scriptFilePath, paramArray);
            _logger.LogDebug("Process arguments : {args}" , processArgs);

            return await RunProcessAsync(processArgs, fileName);
        }

        #region helper methods
        private string PrepareScriptArguments(ScriptEnums.ScriptType scriptType, string scriptFilePath)
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

        private string PrepareScriptArguments(ScriptEnums.ScriptType scriptType, string scriptFilePath , ScriptParamArray paramArray) 
        {
            StringBuilder stringBuilder = new StringBuilder();

            switch (scriptType)
            {
                case ScriptEnums.ScriptType.Shell:
                    stringBuilder.Append(string.Format(" /c {0}" , scriptFilePath)); // /c c:/f/script.cmd
                    break;
                case ScriptEnums.ScriptType.PowerShell:
                    stringBuilder.Append(string.Format(" -f {0}", scriptFilePath)); // -f c:/f/script.ps1
                    break;
            }

            if (paramArray == null)
            {
                return stringBuilder.ToString();
            }

            if (paramArray.Parameters.Count > 0)
            {
                foreach (ScriptParamModel paramModel in paramArray.Parameters)
                {
                    string decodedValue = Base64.DecodeBase64(paramModel.EncodedValue);
                    stringBuilder.Append(string.Format(" -{0} {1}", paramModel.Name, decodedValue));
                }
            }
            return stringBuilder.ToString();
        }

        private void SelectScriptDetails(ScriptEnums.ScriptType scriptType, out string scriptSuffix, out string fileName)
        {
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

        private string CreateScriptFileWithContent(string ScriptContent, string ScriptSuffix)
        {
            string tempFilePath = Path.GetTempFileName();
            tempFilePath = Path.ChangeExtension(tempFilePath, ScriptSuffix);
            File.WriteAllText(tempFilePath, ScriptContent);
            return tempFilePath;
        }

        private async Task<ProcessModel> RunProcessAsync(string processArgs , string fileName)
        {
            return await Task.Run(() =>
            {
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = Path.GetTempPath(),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        FileName = fileName,
                        Arguments = processArgs
                    }
                };
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

        private Process CreateProcess(string processArgs, string fileName)
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