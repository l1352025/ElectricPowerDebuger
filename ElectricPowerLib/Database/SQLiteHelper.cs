using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ElectricPowerDebuger.Database
{
    class SQLiteHelper
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        private static string _connectionString;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_connectionString">连接SQLite库字符串</param>
        public SQLiteHelper(string connStr)
        {
            _connectionString = connStr;
        }

        public SQLiteHelper(string datasource, string version, string password)
            :this(string.Format("Data Source={0};Version={1};password={2}",datasource, version, password))
        {
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="dbName">数据库名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public bool CreateDb(string dbName, string password = "")
        {
            if (string.IsNullOrEmpty(dbName)) return false;

            try
            {
                if (false == File.Exists(dbName))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dbName));
                    SQLiteConnection.CreateFile(dbName);
                }

                if(false == string.IsNullOrEmpty(password))
                {
                    SQLiteConnection con = new SQLiteConnection("data source=" + dbName);
                    con.SetPassword(password);
                    con.Close();
                }
            }
            catch (Exception) { throw; }

            return true;
        }

        /// <summary>
        /// 执行SQL命令 - 增、删、改
        /// </summary>
        /// <param name="sqlText">SQL命令字符串</param>
        /// <param name="parameters">其他参数</param>
        /// <returns>影响的行数</returns>
        public int ExecuteNonQuery(string sqlText, params SQLiteParameter[] parameters)
        {
            int affectRows = 0;

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlText, con))
                {
                    try
                    {
                        if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);

                        con.Open();

                        affectRows = cmd.ExecuteNonQuery();
                    }
                    catch (Exception) { throw; }
                }
            }

            return affectRows;
        }
        /// <summary>
        /// 执行批处理 - 增、删、改
        /// </summary>
        /// <param name="list">key = sqlText, value = parameters</param>
        public void ExecuteNonQueryBatch(List< KeyValuePair<string, SQLiteParameter[]>> list)
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                try
                {
                    con.Open();
                }
                catch (Exception) { throw; }

                using (SQLiteTransaction trans = con.BeginTransaction())
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(con))
                    {
                        try
                        {
                            foreach (var item in list)
                            {
                                cmd.CommandText = item.Key;
                                if (item.Value != null) cmd.Parameters.AddRange(item.Value);
                                cmd.ExecuteNonQuery();
                            }
                            trans.Commit();
                        }
                        catch (Exception) { throw; }
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL命令 - 查询
        /// </summary>
        /// <returns>查询的结果</returns>
        /// <param name="sqlText">SQL命令字符串</param>
        /// <param name="parameters">其他参数</param>
        public SQLiteDataReader ExecuteReader(string sqlText, params SQLiteParameter[] parameters)
        {
            SQLiteConnection con = new SQLiteConnection(_connectionString);
            SQLiteCommand cmd = new SQLiteCommand(sqlText, con);
            try
            {
                if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);

                con.Open();

                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception) { throw; }
        }

        public DataTable ExecuteReaderToDataTable(string sqlText, params SQLiteParameter[] parameters)
        {
            DataTable tb = new DataTable();

            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlText, con))
                {
                    try
                    {
                        if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);

                        con.Open();

                        SQLiteDataAdapter adt = new SQLiteDataAdapter(cmd);
                        adt.Fill(tb);
                    }
                    catch (Exception) { throw; }
                }
            }

            return tb;
        }

        /// <summary>
        /// 执行SQL命令 - 检索
        /// </summary>
        /// <returns>检索的结果</returns>
        /// <param name="sqlText">SQL命令字符串</param>
        /// <param name="parameters">其他参数</param>
        public object ExecuteScalar(string sqlText, params SQLiteParameter[] parameters)
        {
            using (SQLiteConnection con = new SQLiteConnection(_connectionString))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqlText, con))
                {
                    try
                    {
                        if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(parameters);

                        con.Open();

                        return cmd.ExecuteScalar();
                    }
                    catch (Exception) { throw; }
                }
            }
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public void CloseConnection()
        {
        }

        /// <summary>
        /// 读取整张数据表
        /// </summary>
        /// <returns>The full table.</returns>
        /// <param name="tableName">数据表名称</param>
        public SQLiteDataReader ReadFullTable(string tableName)
        {
            string queryString = "SELECT * FROM " + tableName;
            return ExecuteReader(queryString);
        }

        /// <summary>
        /// 向指定数据表中插入数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="values">插入的数值</param>
        public int InsertValues(string tableName, string[] values)
        {
            //获取数据表中字段数目
            SQLiteDataReader reader = ReadFullTable(tableName);
            int fieldCount = reader.FieldCount;
            reader.Close();

            //当插入的数据长度不等于字段数目时引发异常
            if (values.Length != fieldCount)
            {
                throw new SQLiteException("values.Length!=fieldCount");
            }

            string queryString = "INSERT INTO " + tableName + " VALUES (" + "'" + values[0] + "'";
            for (int i = 1; i < values.Length; i++)
            {
                queryString += ", " + "'" + values[i] + "'";
            }
            queryString += " )";
            return ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 更新指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        /// <param name="key">关键字</param>
        /// <param name="value">关键字对应的值</param>
        /// <param name="operation">运算符：=,<,>,...，默认“=”</param>
        public int UpdateValues(string tableName, string[] colNames, string[] colValues, string key, string value, string operation = "=")
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length");
            }

            string queryString = "UPDATE " + tableName + " SET " + colNames[0] + "=" + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += ", " + colNames[i] + "=" + "'" + colValues[i] + "'";
            }
            queryString += " WHERE " + key + operation + "'" + value + "'";
            return ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public int DeleteValuesOR(string tableName, string[] colNames, string[] colValues, string[] operations)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += "OR " + colNames[i] + operations[0] + "'" + colValues[i] + "'";
            }
            return ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 删除指定数据表内的数据
        /// </summary>
        /// <returns>The values.</returns>
        /// <param name="tableName">数据表名称</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colValues">字段名对应的数据</param>
        public int DeleteValuesAND(string tableName, string[] colNames, string[] colValues, string[] operations)
        {
            //当字段名称和字段数值不对应时引发异常
            if (colNames.Length != colValues.Length || operations.Length != colNames.Length || operations.Length != colValues.Length)
            {
                throw new SQLiteException("colNames.Length!=colValues.Length || operations.Length!=colNames.Length || operations.Length!=colValues.Length");
            }

            string queryString = "DELETE FROM " + tableName + " WHERE " + colNames[0] + operations[0] + "'" + colValues[0] + "'";
            for (int i = 1; i < colValues.Length; i++)
            {
                queryString += " AND " + colNames[i] + operations[i] + "'" + colValues[i] + "'";
            }
            return ExecuteNonQuery(queryString);
        }


        /// <summary>
        /// 创建数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public int CreateTable(string tableName, string[] colNames, string[] colTypes)
        {
            string queryString = "CREATE TABLE IF NOT EXISTS " + tableName + "( " + colNames[0] + " " + colTypes[0];
            for (int i = 1; i < colNames.Length; i++)
            {
                queryString += ", " + colNames[i] + " " + colTypes[i];
            }
            queryString += "  ) ";
            return ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// 删除数据表
        /// </summary> +
        /// <returns>The table.</returns>
        /// <param name="tableName">数据表名</param>
        /// <param name="colNames">字段名</param>
        /// <param name="colTypes">字段名类型</param>
        public int DeleteTable(string tableName)
        {
            string queryString = "trop table " + tableName;

            return ExecuteNonQuery(queryString);
        }

        /// <summary>
        /// Reads the table.
        /// </summary>
        /// <returns>The table.</returns>
        /// <param name="tableName">Table name.</param>
        /// <param name="items">Items.</param>
        /// <param name="colNames">Col names.</param>
        /// <param name="operations">Operations.</param>
        /// <param name="colValues">Col values.</param>
        public SQLiteDataReader ReadTable(string tableName, string[] items, string[] colNames, string[] operations, string[] colValues)
        {
            string queryString = "SELECT " + items[0];
            for (int i = 1; i < items.Length; i++)
            {
                queryString += ", " + items[i];
            }
            queryString += " FROM " + tableName + " WHERE " + colNames[0] + " " + operations[0] + " " + colValues[0];
            for (int i = 0; i < colNames.Length; i++)
            {
                queryString += " AND " + colNames[i] + " " + operations[i] + " " + colValues[0] + " ";
            }
            return ExecuteReader(queryString);
        }

        /// <summary>
        /// 本类log
        /// </summary>
        /// <param name="s"></param>
        static void Log(string s)
        {
            Console.WriteLine("class SqLiteHelper:::" + s);
        }

    }
}
