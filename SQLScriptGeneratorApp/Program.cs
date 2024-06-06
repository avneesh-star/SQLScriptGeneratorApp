using System;
using System.Linq;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using SMO = Microsoft.SqlServer.Management.Smo;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.HadrModel;
using System.Text;

namespace SQLScriptGeneratorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = builder.Build();

            var fileName         = @"D:\Dev-Pract\DotNet\csharp-sql-script-generator-master\csharp-sql-script-generator-master\SQLScriptGeneratorApp\backup.sql";
            var connectionString = config.GetConnectionString("SqlConnection");
            var databaseName     = "TestDb";
            var schemaName       = "dbo";

            if (File.Exists(fileName))
                File.Delete(fileName);

            try
            {
                var server    = new SMO.Server(new ServerConnection(new SqlConnection(connectionString)));
                var options   = new SMO.ScriptingOptions();
                var databases = server.Databases[databaseName];

                options.FileName                = fileName;
                options.EnforceScriptingOptions = true;
                options.WithDependencies        = true;
                options.IncludeHeaders          = true;
                options.ScriptDrops             = false;
                options.AppendToFile            = true;
                options.ScriptSchema            = true;
                options.ScriptData              = true;
                options.Indexes                 = true;

                var tableEnum     = databases.Tables.Cast<SMO.Table>().Where(i => i.Schema == schemaName);
                var viewEnum      = databases.Views.Cast<SMO.View>().Where(i => i.Schema == schemaName);
                var procedureEnum = databases.StoredProcedures.Cast<SMO.StoredProcedure>()
                    .Where(i => i.Schema == schemaName);

                Console.WriteLine("SQL Script Generator");

                //Console.WriteLine("\nTable Scripts:");
                //foreach (SMO.Table table in tableEnum)
                //{
                //    databases.Tables[table.Name, schemaName].EnumScript(options);
                //    Console.WriteLine(table.Name);
                //}

                options.ScriptData       = false;
                options.WithDependencies = false;

                //Console.WriteLine("\nView Scripts:");
                //foreach (SMO.View view in viewEnum)
                //{
                //    databases.Views[view.Name, schemaName].Script(options);
                //    Console.WriteLine(view.Name);
                //}

                options.ScriptDrops = false;
                options.IncludeIfNotExists = false;
                options.IncludeDatabaseContext = false;
                options.IncludeHeaders = false;
                options.ScriptForCreateOrAlter = true;

                
               
                Console.WriteLine("\nStored Procedure Scripts:");
                foreach (SMO.StoredProcedure procedure in procedureEnum)
                {
                    
                    //string renameScript = $"EXEC sp_rename '{procedure}', '{procedure}_bkp_{DateTime.Now.Ticks}'";
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine($"IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{procedure}') AND type in (N'P', N'PC'))");
                    stringBuilder.AppendLine($"BEGIN");
                    stringBuilder.AppendLine($"\tEXEC sp_rename '{procedure}', '{procedure}_bkp_{DateTime.Now.Ticks}'");
                    stringBuilder.AppendLine($"END");
                    stringBuilder.AppendLine($"GO");

                    string renameScript = stringBuilder.ToString();

                    //File.WriteAllText(fileName,$"sp_rename '{procedure.Name}','{procedure.Name}_bkp_{DateTime.Now.Ticks}'");
                    //  File.WriteAllText(fileName, string.Join(Environment.NewLine, script));
                    StringCollection script = procedure.Script(options);
                    //databases.StoredProcedures[procedure.Name, schemaName].Script(options);
                    using (StreamWriter writer = new StreamWriter(fileName, false))
                    {
                        // Write the rename script first
                        writer.WriteLine(renameScript);

                        // Write the procedure script
                        foreach (string line in script)
                        {
                            writer.WriteLine(line);
                        }
                        writer.WriteLine("GO");
                    }
                   // databases.StoredProcedures[procedure.Name, schemaName].Script(options);
                    Console.WriteLine(procedure.Name);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured: " + ex.Message);
            }
        }
    }
}
