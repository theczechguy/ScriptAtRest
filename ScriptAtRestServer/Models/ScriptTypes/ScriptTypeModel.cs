namespace ScriptAtRestServer.Models.ScriptTypes
{
    public class ScriptTypeModel
    {
        public string Name { get; set; }

        public string Runner { get; set; }

        public string FileExtension { get; set; }

        public string ScriptArgument { get; set; }
    }
}