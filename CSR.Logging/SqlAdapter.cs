using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CSR.Logging
{
    public class SqlAdapter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public SqlAdapter(bool? isAutoDispose, string sqlConnectionString)
        {
            log4net.GlobalContext.Properties["LogName"] = "CSR.Logging";

            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            try
            {
                string path = Variable.mapPath;
                if (string.IsNullOrEmpty(path))
                {
                    try
                    {
                        path = Variable.mapPath = HttpContext.Current.Server.MapPath(@"~\");
                    }
                    catch (Exception ex)
                    {
                        path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    }
                }

                log4net.Config.XmlConfigurator.Configure(new FileInfo(path + "log4net.config"));
                log.Info("Starting SqlAdapter.");
                DateTime dateTime = DateTime.Now;
                string month = Utils.FullDayOrMonthOrYear(dateTime.Month + "", 1, "0");
                Variable.connectionString = sqlConnectionString;
                Variable.tableName = String.Format("Logging_{0}_{1}", dateTime.Year, month);
                List<JToken> jTokens = Utils.AppSettingArray("Table", "logging", path);
                List<ObjLogging> objLoggings = new List<ObjLogging>();
                foreach (JToken jToken in jTokens)
                {
                    objLoggings.Add(jToken.ToObject<ObjLogging>());
                }
                Variable.commandTable = objLoggings;

                jTokens = Utils.AppSettingArray("Stored", "logging", path);
                objLoggings = new List<ObjLogging>();
                foreach (JToken jToken in jTokens)
                {
                    objLoggings.Add(jToken.ToObject<ObjLogging>());
                }
                Variable.commandStored = objLoggings;

                jTokens = Utils.AppSettingArray("DefineColumn", "logging", path);
                List<ObjDefineColumn> objDefineColumns = new List<ObjDefineColumn>();
                foreach (JToken jToken in jTokens)
                {
                    objDefineColumns.Add(jToken.ToObject<ObjDefineColumn>());
                }
                Variable.columns = objDefineColumns;
                Variable.columnsDefineBuilded = BuildStringColumns();
                Variable.columnsInsertStoredBuilded = BuildInsertStringColumns();
                Variable.columnsInsertStoredParaBuilded = BuildInsertStringParaColumns();
                Variable.columnsInsertStoredParaValueBuilded = BuildInsertStringParaValueColumns();
                Variable.SqlConnect = null;
                Variable.isAutoDispose = isAutoDispose == null ? false : (bool)isAutoDispose;
            }
            catch (Exception ex)
            {
                log.Info("Starting SqlAdapter: " + ex.Message);
            }
        }

        #region Manual Dispose
        private static ReturnValueInfo ManualDisposeConnectionString(ReturnValueInfo returnValueInfo)
        {
            log.Info("ManualDisposeConnectionString Start");
            SqlConnection sqlConn = null;
            try
            {
                if (Variable.SqlConnect != null)
                {
                    sqlConn = Variable.SqlConnect;
                }
                else
                {
                    sqlConn = new SqlConnection(Variable.connectionString);
                }
                try
                {
                    if (Variable.SqlConnect == null)
                    {
                        sqlConn.Open();
                        Variable.SqlConnect = sqlConn;
                    }
                    returnValueInfo.status = true;
                    returnValueInfo.message = "Success";
                    log.Info("ManualDisposeConnectionString Done");
                }
                catch (Exception ex)
                {
                    if (sqlConn != null)
                    {
                        sqlConn.Dispose();
                        sqlConn.Close();
                    }
                    if (Variable.SqlConnect != null)
                    {
                        Variable.SqlConnect.Dispose();
                        Variable.SqlConnect.Close();
                        Variable.SqlConnect = null;
                    }
                    returnValueInfo.status = false;
                    returnValueInfo.message = string.Format("Error: {0}", ex.Message);
                    log.Info("ManualDisposeConnectionString Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                if (sqlConn != null)
                {
                    sqlConn.Dispose();
                    sqlConn.Close();
                }
                if (Variable.SqlConnect != null)
                {
                    Variable.SqlConnect.Dispose();
                    Variable.SqlConnect.Close();
                    Variable.SqlConnect = null;
                }
                returnValueInfo.status = false;
                returnValueInfo.message = string.Format("Error: {0}", ex.Message);
                log.Info("ManualDisposeConnectionString Error: " + ex.Message);
            }
            return returnValueInfo;
        }

        private static ReturnValueInfo ManualDisposeSqlStoreCommand(string sqlCmdName, List<ObjData> objDatas)
        {
            log.Info("ManualDisposeSqlStoreCommand Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            SqlConnection sqlConn = null;
            string key = "";
            try
            {
                if (Variable.SqlConnect != null)
                {
                    sqlConn = Variable.SqlConnect;
                }
                else
                {
                    sqlConn = new SqlConnection(Variable.connectionString);
                }
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCmdName, sqlConn))
                    {
                        try
                        {
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            foreach (ObjData objData in objDatas)
                            {
                                key = String.Format("@{0}", objData.Key);
                                var type = Enum.GetValues(typeof(SqlDbType)).Cast<SqlDbType>().Where(x => (x + "").ToLower() == objData.Type.ToLower()).FirstOrDefault();
                                if (type != null)
                                {
                                    sqlCmd.Parameters.Add(new SqlParameter(key, type)).Value = objData.Value;
                                }
                            }
                            if (Variable.SqlConnect == null)
                            {
                                sqlConn.Open();
                                Variable.SqlConnect = sqlConn;
                            }
                            sqlCmd.ExecuteNonQuery();
                            returnValueInfo.status = true;
                            returnValueInfo.message = "Success";
                            log.Info("ManualDisposeSqlStoreCommand Done");
                        }
                        catch (Exception ex)
                        {
                            if (sqlConn != null)
                            {
                                sqlConn.Dispose();
                                sqlConn.Close();
                            }
                            if (Variable.SqlConnect != null)
                            {
                                Variable.SqlConnect.Dispose();
                                Variable.SqlConnect.Close();
                                Variable.SqlConnect = null;
                            }
                            returnValueInfo.status = false;
                            returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                            log.Info("ManualDisposeSqlStoreCommand Error: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (sqlConn != null)
                    {
                        sqlConn.Dispose();
                        sqlConn.Close();
                    }
                    if (Variable.SqlConnect != null)
                    {
                        Variable.SqlConnect.Dispose();
                        Variable.SqlConnect.Close();
                        Variable.SqlConnect = null;
                    }
                    returnValueInfo.status = false;
                    returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                    log.Info("ManualDisposeSqlStoreCommand Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                if (sqlConn != null)
                {
                    sqlConn.Dispose();
                    sqlConn.Close();
                }
                if (Variable.SqlConnect != null)
                {
                    Variable.SqlConnect.Dispose();
                    Variable.SqlConnect.Close();
                    Variable.SqlConnect = null;
                }
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("ManualDisposeSqlStoreCommand Error: " + ex.Message);
            }
            return returnValueInfo;
        }

        private static ReturnValueInfo ManualDisposeCreateWithSQLCommand(string sqlCmdName, bool isQuery)
        {
            log.Info("ManualDisposeCreateWithSQLCommand Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            DataTable table = new DataTable();
            string jsonData = "";
            SqlConnection sqlConn = null;
            try
            {
                if (Variable.SqlConnect != null)
                {
                    sqlConn = Variable.SqlConnect;
                }
                else
                {
                    sqlConn = new SqlConnection(Variable.connectionString);
                }
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCmdName, sqlConn))
                    {
                        try
                        {

                            if (Variable.SqlConnect == null)
                            {
                                sqlConn.Open();
                                Variable.SqlConnect = sqlConn;
                            }
                            if (isQuery == true)
                            {
                                using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
                                {
                                    adapter.Fill(table);
                                    jsonData = JsonConvert.SerializeObject(table);
                                    returnValueInfo.jsonData = jsonData;
                                    log.Info("ManualDisposeCreateWithSQLCommand Done With Data:" + jsonData);
                                }
                            }
                            else
                            {
                                sqlCmd.ExecuteNonQuery();
                                log.Info("ManualDisposeCreateWithSQLCommand Done");
                            }
                            sqlCmd.ExecuteNonQuery();
                            returnValueInfo.status = true;
                            returnValueInfo.message = "Success";

                        }
                        catch (Exception ex)
                        {
                            if (sqlConn != null)
                            {
                                sqlConn.Dispose();
                                sqlConn.Close();
                            }
                            if (Variable.SqlConnect != null)
                            {
                                Variable.SqlConnect.Dispose();
                                Variable.SqlConnect.Close();
                                Variable.SqlConnect = null;
                            }
                            returnValueInfo.status = false;
                            returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                            log.Info("ManualDisposeCreateWithSQLCommand Error: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (sqlConn != null)
                    {
                        sqlConn.Dispose();
                        sqlConn.Close();
                    }
                    if (Variable.SqlConnect != null)
                    {
                        Variable.SqlConnect.Dispose();
                        Variable.SqlConnect.Close();
                        Variable.SqlConnect = null;
                    }
                    returnValueInfo.status = false;
                    returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                    log.Info("ManualDisposeCreateWithSQLCommand Error: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                if (sqlConn != null)
                {
                    sqlConn.Dispose();
                    sqlConn.Close();
                }
                if (Variable.SqlConnect != null)
                {
                    Variable.SqlConnect.Dispose();
                    Variable.SqlConnect.Close();
                    Variable.SqlConnect = null;
                }
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("ManualDisposeCreateWithSQLCommand Error: " + ex.Message);
            }
            return returnValueInfo;
        }


        #endregion

        #region Auto Dispose
        private static ReturnValueInfo AutoDisposeConnection(ReturnValueInfo returnValueInfo)
        {
            log.Info("AutoDisposeConnection Start");
            using (SqlConnection sqlCnn = new SqlConnection(Variable.connectionString))
            {
                try
                {
                    sqlCnn.Open();
                    sqlCnn.Close();
                    returnValueInfo.status = true;
                    returnValueInfo.message = "Success";
                    log.Info("AutoDisposeConnection Done");
                }
                catch (Exception ex)
                {
                    returnValueInfo.status = false;
                    returnValueInfo.message = string.Format("Error: {0}", ex.Message);
                    log.Info("AutoDisposeConnection Error: " + ex.Message);
                }
                return returnValueInfo;
            }
        }

        private static ReturnValueInfo AutoDisposeSqlStoreCommand(string sqlCmdName, List<ObjData> objDatas)
        {
            log.Info("AutoDisposeSqlStoreCommand Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            string key = "";
            using (SqlConnection sqlConn = new SqlConnection(Variable.connectionString))
            {
                try
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCmdName, sqlConn))
                    {
                        try
                        {
                            sqlCmd.CommandType = CommandType.StoredProcedure;
                            foreach (ObjData objData in objDatas)
                            {
                                key = String.Format("@{0}", objData.Key);
                                var type = Enum.GetValues(typeof(SqlDbType)).Cast<SqlDbType>().Where(x => (x + "").ToLower() == objData.Type.ToLower()).FirstOrDefault();
                                if (type != null)
                                {
                                    sqlCmd.Parameters.Add(new SqlParameter(key, type)).Value = objData.Value;
                                }
                            }
                            sqlConn.Open();
                            sqlCmd.ExecuteNonQuery();
                            log.Info("AutoDisposeSqlStoreCommand Done");
                            returnValueInfo.status = true;
                            returnValueInfo.message = "Success";
                        }
                        catch (Exception ex)
                        {
                            returnValueInfo.status = false;
                            returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                            log.Info("AutoDisposeSqlStoreCommand Error: " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    returnValueInfo.status = false;
                    returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                    log.Info("AutoDisposeSqlStoreCommand Error: " + ex.Message);
                }
            }
            return returnValueInfo;
        }

        private static ReturnValueInfo AutoDisposeCreateWithSQLCommand(string sqlCmdName, bool isQuery)
        {
            log.Info("AutoDisposeCreateWithSQLCommand Start");
            DataTable table = new DataTable();
            string jsonData = "";
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            try
            {
                using (SqlConnection sqlConn = new SqlConnection(Variable.connectionString))
                {
                    try
                    {
                        using (SqlCommand sqlCmd = new SqlCommand(sqlCmdName, sqlConn))
                        {
                            try
                            {
                                sqlConn.Open();

                                if (isQuery == true)
                                {
                                    using (SqlDataAdapter adapter = new SqlDataAdapter(sqlCmd))
                                    {
                                        adapter.Fill(table);
                                        jsonData = JsonConvert.SerializeObject(table);
                                        returnValueInfo.jsonData = jsonData;
                                        log.Info("AutoDisposeCreateWithSQLCommand Done With Data: " + jsonData);
                                    }
                                }
                                else
                                {
                                    sqlCmd.ExecuteNonQuery();
                                    log.Info("AutoDisposeCreateWithSQLCommand Done");
                                }
                                returnValueInfo.status = true;
                                returnValueInfo.message = "Success";
                            }
                            catch (Exception ex)
                            {
                                returnValueInfo.status = false;
                                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                                log.Info("AutoDisposeCreateWithSQLCommand Error: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        returnValueInfo.status = false;
                        returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                        log.Info("AutoDisposeCreateWithSQLCommand Error: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("AutoDisposeCreateWithSQLCommand Error: " + ex.Message);
            }
            return returnValueInfo;
        }
        #endregion
        #region Private Primary Function

        private static string BuildStringColumns()
        {
            log.Info("BuildStringColumns Start");
            try
            {
                List<string> lstStringColumns = new List<string>();
                foreach (ObjDefineColumn columnDefine in Variable.columns)
                {
                    if (columnDefine.Number != "0")
                        lstStringColumns.Add(String.Format("[{0}] {1} ({2})", columnDefine.Key, columnDefine.Type, columnDefine.Number));
                    else
                        lstStringColumns.Add(String.Format("[{0}] {1}", columnDefine.Key, columnDefine.Type));
                }
                return String.Join(", ", lstStringColumns);
            }
            catch (Exception ex)
            {
                log.Info("BuildStringColumns Error: " + ex.Message);
                return "";
            }
        }

        private static string BuildInsertStringColumns()
        {
            log.Info("BuildInsertStringColumns Start");
            try
            {
                List<string> lstStringColumns = new List<string>();
                foreach (ObjDefineColumn columnDefine in Variable.columns)
                {
                    lstStringColumns.Add(String.Format("{0}", columnDefine.Key));
                }
                return String.Join(", ", lstStringColumns);
            }
            catch (Exception ex)
            {
                log.Info("BuildInsertStringColumns Error: " + ex.Message);
                return "";
            }
        }

        private static string BuildInsertStringParaColumns()
        {
            log.Info("BuildInsertStringParaColumns: Start");
            try
            {
                List<string> lstStringColumns = new List<string>();
                foreach (ObjDefineColumn columnDefine in Variable.columns)
                {
                    if (columnDefine.Number != "0")
                        lstStringColumns.Add(String.Format("@{0} {1} ({2})", columnDefine.Key, columnDefine.Type, columnDefine.Number));
                    else
                        lstStringColumns.Add(String.Format("@{0} {1}", columnDefine.Key, columnDefine.Type));
                }
                return String.Join(", ", lstStringColumns);
            }
            catch (Exception ex)
            {
                log.Info("BuildInsertStringParaColumns Error: " + ex.Message);
                return "";
            }
        }

        private static string BuildInsertStringParaValueColumns()
        {
            log.Info("BuildInsertStringParaValueColumns: Start");
            try
            {
                List<string> lstStringColumns = new List<string>();
                foreach (ObjDefineColumn columnDefine in Variable.columns)
                {
                    lstStringColumns.Add(String.Format("@{0}", columnDefine.Key));
                }
                return String.Join(", ", lstStringColumns);
            }
            catch (Exception ex)
            {
                log.Info("BuildInsertStringParaValueColumns Error: " + ex.Message);
                return "";
            }
        }

        private static ReturnValueInfo SqlConnection(bool isAutoDispose)
        {
            log.Info("SqlConnection: Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            try
            {
                if (isAutoDispose == false)
                {
                    return ManualDisposeConnectionString(returnValueInfo);
                }
                else
                {
                    return AutoDisposeConnection(returnValueInfo);
                }
            }
            catch (Exception ex)
            {
                log.Info("SqlConnection Error: " + ex.Message);
                returnValueInfo.status = false;
                returnValueInfo.message = ex.Message;
                return returnValueInfo;
            }
        }
        private static ReturnValueInfo SqlStoreCommand(string sqlCmdName, List<ObjData> objDatas)
        {
            log.Info("SqlStoreCommand: Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            try
            {
                if (Variable.isAutoDispose == true)
                {
                    returnValueInfo = AutoDisposeSqlStoreCommand(sqlCmdName, objDatas);
                }
                else
                {
                    returnValueInfo = ManualDisposeSqlStoreCommand(sqlCmdName, objDatas);
                }
            }
            catch (Exception ex)
            {
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("Error SqlStoreCommand: " + ex.Message);
            }
            return returnValueInfo;
        }

        private static ReturnValueInfo CreateWithSQLCommand(string sqlCmdName, bool isQuery)
        {
            log.Info("CreateWithSQLCommand: Start");
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            try
            {
                if (Variable.isAutoDispose == true)
                {
                    returnValueInfo = AutoDisposeCreateWithSQLCommand(sqlCmdName, isQuery);
                }
                else
                {
                    returnValueInfo = ManualDisposeCreateWithSQLCommand(sqlCmdName, isQuery);
                }
            }
            catch (Exception ex)
            {
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("CreateWithSQLCommand Error: " + ex.Message);
            }
            return returnValueInfo;
        }

        #endregion

        #region Public Primary Function

        public static string BuildQueryString(List<ObjQuery> objQuerys)
        {
            log.Info("BuildQueryString: Start");
            string returnBuildQuery = string.Empty;
            try
            {
                List<string> stringQuery = new List<string>();
                foreach (ObjQuery objQuery in objQuerys)
                {
                    var columns = Variable.columns.Where(x => x.Key.ToLower() == objQuery.key.ToLower()).FirstOrDefault();
                    if (columns != null)
                        stringQuery.Add(String.Format("{3} {0} {2} {1}", columns.Key, objQuery.value, objQuery.condition, objQuery.operation));
                }
                returnBuildQuery = String.Join(" ", stringQuery);
            }
            catch (Exception ex)
            {
                log.Info("BuildQueryString Error: " + ex.Message);
            }
            return returnBuildQuery;
        }

        public static ReturnValueInfo LoggingFunction(Command cmd, List<ObjData> objDatas, bool? isTableDelete, string queryString)
        {
            string sqlCommand = "";
            bool isTableDeleteFunc = isTableDelete == null ? false : (bool)isTableDelete;
            ReturnValueInfo returnValueInfo = new Logging.ReturnValueInfo();
            List<ObjLogging> objTableLoggings = Variable.commandTable;
            List<string> lstCmd = new List<string>();
            var objLogging = objTableLoggings.Where(x => x.Key == (cmd + "")).FirstOrDefault();
            string stringQueryOpts = queryString;
            //foreach (Appvity.eOffice.Logging.ObjLogging objLogging in objTableLoggings)
            //{
            if (objLogging != null)
            {
                switch (objLogging.Key)
                {
                    case "Check":
                        {
                            log.Info("LoggingFunction Check: Start.");
                            sqlCommand = String.Format(objLogging.Command, Variable.tableName);
                            returnValueInfo = SqlAdapter.CreateWithSQLCommand(sqlCommand, false);
                            log.Info("LoggingFunction Check: Done.");
                        }
                        break;
                    case "Create":
                        {
                            log.Info("LoggingFunction Create: Start.");
                            returnValueInfo = LoggingFunction(Command.Check, new List<ObjData>(), null, "");
                            if (returnValueInfo.status == false)
                            {
                                sqlCommand = String.Format(objLogging.Command, Variable.tableName, Variable.columnsDefineBuilded);
                                returnValueInfo = Logging.SqlAdapter.CreateWithSQLCommand(sqlCommand, false);
                                log.Info("LoggingFunction Create: Done.");
                            }
                            else
                            {
                                returnValueInfo.status = false;
                                returnValueInfo.message = String.Format("Error: {0}", "The table is exists.");
                                log.Info("LoggingFunction Create Error: {0}." + returnValueInfo.message);
                            }
                        }
                        break;
                    case "Insert":
                        {
                            log.Info("LoggingFunction Insert: Start.");
                            returnValueInfo = LoggingFunction(Command.Check, new List<ObjData>(), null, "");
                            if (returnValueInfo.status == true && returnValueInfo.message == "Success")
                            {
                                if (Variable.columns.Count == objDatas.Count)
                                {
                                    List<string> lstValue = new List<string>();
                                    foreach (ObjData objData in objDatas)
                                    {
                                        if (objData.Type.ToLower() == "nvarchar" || objData.Type.ToLower() == "ntext")
                                            lstValue.Add(string.Format("N'{0}'", objData.Value));
                                        else
                                            lstValue.Add(string.Format("'{0}'", objData.Value));
                                    }
                                    Variable.columnsInsertStoredParaValueBuilded = String.Join(", ", lstValue);
                                    sqlCommand = String.Format(objLogging.Command, Variable.tableName, Variable.columnsInsertStoredBuilded, Variable.columnsInsertStoredParaValueBuilded);
                                    returnValueInfo = SqlAdapter.CreateWithSQLCommand(sqlCommand, false);
                                    log.Info("LoggingFunction Insert: Done.");
                                }
                                else
                                {
                                    returnValueInfo.status = false;
                                    returnValueInfo.message = String.Format("Error: {0}", "The number of field and value isnot same.");
                                    log.Info("LoggingFunction Insert Error: {0}." + returnValueInfo.message);
                                }
                            }
                            else
                            {
                                returnValueInfo.status = false;
                                returnValueInfo.message = returnValueInfo.message;
                                log.Info("LoggingFunction Insert Error: {0}." + returnValueInfo.message);
                            }
                        }
                        break;
                    case "Delete":
                        {
                            log.Info("LoggingFunction Delete: Start.");
                            // Check Table
                            returnValueInfo = LoggingFunction(Command.Check, new List<ObjData>(), null, "");
                            if (returnValueInfo.status == true && returnValueInfo.message == "Success")
                            {
                                if (isTableDeleteFunc == true)
                                {
                                    sqlCommand = String.Format(objLogging.Command, Variable.tableName);
                                    returnValueInfo = SqlAdapter.CreateWithSQLCommand(sqlCommand, false);
                                    log.Info("LoggingFunction Delete: Done.");
                                }
                                else
                                {
                                    returnValueInfo.status = false;
                                    returnValueInfo.message = String.Format("Error: {0}", "Parameter delete table is false.");
                                    log.Info("LoggingFunction Delete Error: " + returnValueInfo.message);
                                }
                            }
                            else
                            {
                                returnValueInfo.status = false;
                                returnValueInfo.message = returnValueInfo.message;
                                log.Info("LoggingFunction Delete Error: " + returnValueInfo.message);
                            }
                        }
                        break;
                    case "Query":
                        {
                            log.Info("LoggingFunction Query: Start");
                            returnValueInfo = LoggingFunction(Command.Check, new List<ObjData>(), null, "");
                            if (returnValueInfo.status == true && returnValueInfo.message == "Success")
                            {
                                sqlCommand = String.Format(objLogging.Command, Variable.tableName, stringQueryOpts);
                                returnValueInfo = SqlAdapter.CreateWithSQLCommand(sqlCommand, true);
                                log.Info("LoggingFunction Query: Done");
                            }
                            else
                            {
                                returnValueInfo.status = false;
                                returnValueInfo.message = returnValueInfo.message;
                                log.Info("LoggingFunction Query Error : " + returnValueInfo.message);
                            }
                        }
                        break;
                }
            }
            else
            {
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", "The command is incorrect.");
                log.Info("LoggingFunction Error: " + returnValueInfo.message);
            }
            //}
            return returnValueInfo;
        }

        public static ReturnValueInfo InitConnectSQL()
        {
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            returnValueInfo = SqlConnection(Variable.isAutoDispose);
            return returnValueInfo;
        }

        public static ReturnValueInfo DisposeConnection()
        {
            log.Info("DisposeConnection Start");
            Variable.mapPath = string.Empty;
            ReturnValueInfo returnValueInfo = new ReturnValueInfo();
            try
            {
                if (Variable.SqlConnect != null)
                {
                    Variable.SqlConnect.Dispose();
                    Variable.SqlConnect.Close();
                }
                returnValueInfo.status = true;
                returnValueInfo.message = "Success";
                log.Info("DisposeConnection Done");
            }
            catch (Exception ex)
            {
                returnValueInfo.status = false;
                returnValueInfo.message = String.Format("Error: {0}", ex.Message);
                log.Info("DisposeConnection Error: " + ex.Message);
            }
            return returnValueInfo;
        }
        #endregion
    }
}
