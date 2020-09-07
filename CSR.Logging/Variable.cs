using System.Collections.Generic;
using System.Data.SqlClient;

namespace CSR.Logging
{
    public static class Variable
    {
        public static string connectionString { get; set; }
        public static string tableName { get; set; }
        public static string storedName { get; set; }
        public static List<ObjLogging> commandTable { get; set; }
        public static List<ObjLogging> commandStored { get; set; }
        public static List<ObjDefineColumn> columns { get; set; }
        public static List<ObjData> columnsValue { get; set; }
        public static SqlConnection SqlConnect { get; set; }
        public static bool isAutoDispose { get; set; }
        public static string columnsDefineBuilded { get; set; }
        public static string columnsValueBuilded { get; set; }
        public static string columnsInsertStoredBuilded { get; set; }
        public static string columnsInsertStoredParaBuilded { get; set; }
        public static string columnsInsertStoredParaValueBuilded { get; set; }

        public static string mapPath { get; set; } = string.Empty;
    }

    public enum Command
    {
        Check = 0,
        Create = 1,
        Insert = 2,
        Delete = 3,
        Query = 4,
        Update = 5
    }

    public enum QueryOption
    {
        None = 0,
        Title = 1,
        ActionDate = 2
    }
}