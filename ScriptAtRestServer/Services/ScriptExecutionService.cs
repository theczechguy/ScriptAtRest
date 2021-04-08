using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ScriptAtRestServer.Entities;
using ScriptAtRestServer.Enums;
using ScriptAtRestServer.Helpers;
using ScriptAtRestServer.Models.Scripts;

namespace ScriptAtRestServer.Services
{
    public interface IScriptExecutionService
    {
        Task<ProcessModel> RunScriptById(int id, ScriptParamArray paramArray);
    };

    public class ScriptExecutionService : IScriptExecutionService
    {
        private ILogger<ScriptExecutionService> _logger;
        private SqLiteDataContext _context;
        protected readonly IConfiguration _configuration;
        private ScriptService _scriptService;
        public ScriptExecutionService(
            SqLiteDataContext Context ,
            ILogger<ScriptExecutionService> Logger,
            IConfiguration Configuration,
            ScriptService ScriptService) {
            _context = Context;
            _logger = Logger;
            _configuration = Configuration;
            _scriptService = ScriptService;
        }

        public async Task<ProcessModel> RunScriptById(int id , ScriptParamArray paramArray)
        {
            Script script = _context.Scripts.Find(id);
            if (script == null)
            {
                throw new AppException("Script not found in database");
            }

            ScriptType scriptType = _scriptService.GetTypeById(script.Type);

            string scriptFilePath = CreateScriptFileWithContent(script.Content, scriptType.FileExtension);
            _logger.LogDebug("Script file : {fullPath}" , scriptFilePath);

            string processArgs = PrepareScriptArguments(scriptType, scriptFilePath, paramArray);
            _logger.LogDebug("Process arguments : {args}" , processArgs);

            try
            {
                ProcessModel processModel = await RunProcessAsync(processArgs, fileName);
                _logger.LogDebug("Process result : {@model}" , processModel);
                return processModel;
            }
            finally {
                await DeleteScriptFileAsync(scriptFilePath);
            }
        }

        #region helper methods
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

        private Task DeleteScriptFileAsync(string FilePath) 
        {
            bool deleteScript = _configuration.GetValue<bool>("ScriptExecution:DeleteScriptFilesAfterExecution");
            if (deleteScript)
            {
                _logger.LogDebug("Deleting file : {file}" , FilePath);
                return Task.Run(() => File.Delete(FilePath));
            }
            else
            {
                _logger.LogWarning("File will not be deleted as requested by configuration option 'DeleteScriptFilesAfterExecution'");
                return Task.CompletedTask;
            }
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
                        Arguments = processArgs,
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
        #endregion
    }
}