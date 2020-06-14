using System;
using System.Data.SqlClient;
using System.Linq;
using OrcaSql.Core.Engine;

namespace OrcaSql.Scripting
{
    public class RestorePageScriptWriter
    {
        private readonly Database _database;

        public RestorePageScriptWriter(Database database)
        {
            _database = database;
        }

        public string CreateRestoreScriptWithRollback(string connectionString, PagePointer pagePointer)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DBCC READPAGE (@dbName, @fileId, @pageId, 'V8192', 1)";
                    command.Parameters.AddWithValue("@dbName", connection.Database);
                    command.Parameters.AddWithValue("@fileId", pagePointer.FileID);
                    command.Parameters.AddWithValue("@pageId", pagePointer.PageID);

                    var reader = command.ExecuteReader();

                    reader.Read();

                    var pageBytes = (byte[])reader["ValueDump"];

                    var backupPage = _database.GetPage(pagePointer).RawBytes;

                    var rolloutScript =
                        "-- PAGE UPDATE SCRIPT\r\n\r\n" +
                        $"DBCC WRITEPAGE('{connection.Database}', {pagePointer.FileID}, {pagePointer.PageID}, 0, 8000, 0x{BitConverter.ToString(backupPage.Take(8000).ToArray()).Replace("-", string.Empty)}, 1);\r\n" +
                        $"DBCC WRITEPAGE('{connection.Database}', {pagePointer.FileID}, {pagePointer.PageID}, 8000, 192, 0x{BitConverter.ToString(backupPage.Skip(8000).ToArray()).Replace("-", string.Empty)}, 1);";

                    var rollBackScript =
                        "-- ROLLBACK SCRIPT\r\n\r\n" +
                        $"-- DBCC WRITEPAGE('{connection.Database}', {pagePointer.FileID}, {pagePointer.PageID}, 0, 8000, 0x{BitConverter.ToString(pageBytes.Take(8000).ToArray()).Replace("-", string.Empty)}, 1);\r\n" +
                        $"-- DBCC WRITEPAGE('{connection.Database}', {pagePointer.FileID}, {pagePointer.PageID}, 8000, 192, 0x{BitConverter.ToString(pageBytes.Skip(8000).ToArray()).Replace("-", string.Empty)}, 1);";


                    var resultScript =
                        $"\r\nUSE [{connection.Database}]\r\nGO\r\nALTER DATABASE [{connection.Database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;\r\nGO\r\n" +
                        rolloutScript + "\r\n\r\n" + rollBackScript +
                        $"\r\n\r\nALTER DATABASE [{connection.Database}] SET MULTI_USER\r\nGO";
                    return resultScript;
                }
            }
        }
    }
}
