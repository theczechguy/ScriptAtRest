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
        private IScriptService _scriptService;
        public ScriptExecutionService(
            SqLiteDataContext Context,
            ILogger<ScriptExecutionService> Logger,
            IConfiguration Configuration,
            IScriptService ScriptService) {
            _context = Context;
            _logger = Logger;
            _configuration = Configuration;
            _scriptService = ScriptService;
        }

        public async Task<ProcessModel> RunScriptById(int id , ScriptParamArray paramArray)
        {
            Script script = await _context.Scripts.FindAsync(id);
            if (script == null)
            {
                throw new AppException($"Script with id {id} not found in database");
            }

            ScriptType scriptType = await _scriptService.GetTypeByIdAsync(script.Type);

            string scriptFilePath = CreateScriptFileWithContent(script.Content, scriptType.FileExtension);

            string processArgs = PrepareScriptArguments(scriptType, scriptFilePath, paramArray);
            _logger.LogDebug("Process arguments : {args}", processArgs);

            try
            {
                ProcessModel processModel = await RunProcessAsync(processArgs, scriptType.Runner);
                _logger.LogDebug("Process result : {@model}", processModel);
                return processModel;
            }
            finally {
                await DeleteScriptFileAsync(scriptFilePath);
            }
        }

        #region helper methods
        private string PrepareScriptArguments(ScriptType ScriptType, string scriptFilePath , ScriptParamArray paramArray) 
        {
            _logger.LogDebug("Preparing script arguments");
            StringBuilder stringBuilder = new StringBuilder();
            
            if (ScriptType.ScriptArgument != null)
            {
                stringBuilder.AppendFormat(" {0} {1}", ScriptType.ScriptArgument, scriptFilePath);
            }
            else
            {
                stringBuilder.AppendFormat(" {0}" , scriptFilePath);
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

        private string CreateScriptFileWithContent(string ScriptContent, string ScriptSuffix)
        {
            string tempFilePath = Path.GetTempFileName();
            _logger.LogDebug("New temporary file : {file}", tempFilePath);
            
            tempFilePath = Path.ChangeExtension(tempFilePath, ScriptSuffix);
            _logger.LogDebug("Temporary file extension set to : {extension}", ScriptSuffix);

            File.WriteAllText(tempFilePath, ScriptContent);
            _logger.LogDebug("Script content stored in temp file");

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

        private async Task<ProcessModel> RunProcessAsync(string processArgs, string fileName)
        {
            return await Task.Run(() =>
            {
                _logger.LogDebug("Process runner : {runner}" , fileName);

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