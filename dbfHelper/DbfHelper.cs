using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbfHelper
{
    public class DbfHelper
    {
        /// <summary>
        /// 数据库所在路径
        /// </summary>
        private string filePath = "";

        /// <summary>
        /// 连接字符串
        /// </summary>
        private string connstring = "";

        /// <summary>
        /// 数据库连接
        /// </summary>
        private OleDbConnection Connection = new OleDbConnection();

        /// <summary>
        /// 错误信息
        /// </summary>
        private string _ErrInfo;

        #region 构造函数 初始化connectionstring
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath">dbf文件所在文件夹路径</param>
        public DbfHelper(string filePath)
        {
            this.filePath = filePath;
            this.connstring = string.Format("Provider = Microsoft.Jet.OLEDB.4.0 ;Data Source ={0};Extended Properties=dBASE IV;", filePath);
            this.Connection = new OleDbConnection(connstring);
        }
        #endregion

        #region 更改数据库所在路径
        /// <summary>
        /// 改变数据库所在路径
        /// </summary>
        /// <param name="filePath">新文件夹路径</param>
        /// <returns></returns>
        public bool ChangeDbfPosition(string filePath)
        {
            bool success = true;
            if (!Directory.Exists(filePath))
            {
                success = false;
            }
            else
            {
                this.filePath = filePath;
                this.connstring = string.Format("Provider = Microsoft.Jet.OLEDB.4.0 ;Data Source ={0};Extended Properties=dBASE IV;", filePath);
                this.Connection = new OleDbConnection(connstring);
                this._ErrInfo = string.Empty;

            }
            return success;
        }
        #endregion

        #region 读取Dbf文件转化为datatable
        /// <summary>
        ///  读取Dbf文件转化为datatable
        /// </summary>
        /// <param name="fileName">dbf文件名</param>
        /// <returns></returns>
        public DataTable GetDateTableByDBF(string fileName)
        {
            DataTable blTable = new DataTable();
            try
            {
                Connection.Open();
                string m_Path = filePath + "\\" + fileName;   //获取完整的文件路径
                // 复制一个新的dbf文件  
                string strNewFileName = "newBL.dbf";
                string strPath = filePath + "\\" + strNewFileName;
                if (File.Exists(strPath))
                {
                    File.Delete(strPath);
                }
                File.Copy(m_Path, strPath);
                // 读取dbf文件  
                string strTableName = Path.GetFileNameWithoutExtension(strPath);
                string sql = @"SELECT * FROM " + strTableName;
                OleDbDataAdapter adapter = new OleDbDataAdapter(sql, Connection);
                DataSet dataSet = new DataSet();
                adapter.Fill(dataSet);

                foreach (DataRow myRow in dataSet.Tables[0].Rows)
                    foreach (DataColumn myCol in dataSet.Tables[0].Columns)
                    {
                        string sss = myCol.ColumnName;
                        string ss = myCol.DataType.Name;
                        if (myRow[myCol].ToString() == "")
                        {
                            myRow[myCol] = "empty";
                        }
                    }
                blTable = dataSet.Tables[0];
                if (File.Exists(strPath))
                {
                    File.Delete(strPath);
                }
            }
            catch (System.Exception ex)
            {
                throw;
            }
            return blTable;
        }
        #endregion

        #region 创建表
        /// <summary>
        /// 构造dbf文件，文件名称为dt的表名，后缀名为dbf
        /// </summary>
        /// <param name="dt">待写入的表格数据</param>
        /// <returns></returns>
        public bool CreateNewTable(DataTable dt)
        {
            bool success = false;
            OleDbCommand command = Connection.CreateCommand();
            try
            {
                if (File.Exists(filePath + @"\" + dt.TableName + ".dbf"))
                {
                    File.Delete(filePath + @"\" + dt.TableName + ".dbf");
                }
                Connection.Open();
                command.CommandType = CommandType.Text;
                List<string> cols = new List<string>();
                foreach (DataColumn dc in dt.Columns)
                {
                    string colType = "";
                    string colName = dc.ColumnName;
                    switch (dc.DataType.Name)
                    {
                        case "Boolean":
                            colType = "bit";
                            break;
                        case "Double":
                        case "Float":
                            colType = "double";
                            break;
                        case "Int16":
                        case "Int32":
                        case "Int64":
                        case "Int":
                            colType = "int";
                            break;
                        case "String":
                            colType = "varchar(20)";
                            break;
                        case "DateTime":
                            colType = "datetime";
                            break;
                        default:
                            colType = "varchar(20)";
                            break;
                    }
                    cols.Add(string.Format(@"{0} {1}", colName, colType));
                }
                string cols_where = string.Join(",", cols);
                string sql = string.Format(@"CREATE TABLE {0} ({1})", dt.TableName, cols_where);
                command.CommandText = sql;
                command.ExecuteNonQuery();
                success = true;
            }
            catch (Exception c)
            {
                _ErrInfo = c.Message;
            }
            finally
            {
                command.Dispose();
                if (Connection.State == System.Data.ConnectionState.Open)
                    Connection.Close();
                command.Dispose();
            }
            return success;
        }
        #endregion

        #region 读取表
        /// <summary>
        /// 导入数据到dbf文件
        /// </summary>
        /// <param name="dt"></param>
        /// <returns>导入的数据条数</returns>
        public int fillData(DataTable dt)
        {
            int count = 0;
            OleDbCommand dc = Connection.CreateCommand();
            _ErrInfo = "";
            try
            {
                Connection.Open();
                //导入数据
                foreach (DataRow row in dt.Rows)
                {
                    string sqlInsert = "insert into " + dt.TableName + "({0}) values({1})";
                    string invalues = "";
                    string cols = "";
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (row[col].ToString() != string.Empty && row[col].ToString() != null && row[col].ToString() != "null")
                        {
                            cols += col.ColumnName + ",";
                            if (col.DataType == typeof(string))
                            {
                                invalues += "'" + row[col].ToString() + "',";
                            }
                            else if (col.DataType == typeof(DateTime))
                            {
                                invalues += "#" + row[col].ToString() + "#,";
                            }
                            else
                            {
                                invalues += row[col].ToString() + ",";
                            }
                        }
                    }
                    invalues = invalues.Remove(invalues.Length - 1, 1);
                    cols = cols.Remove(cols.Length - 1, 1);
                    sqlInsert = string.Format(sqlInsert, cols, invalues);
                    dc.CommandText = sqlInsert;
                    count += dc.ExecuteNonQuery();
                }
            }
            catch (Exception err)
            {
                _ErrInfo = err.Message;
            }
            finally
            {
                if (Connection != null)
                    Connection.Close();
                dc.Dispose();
            }
            return count;
        }
        #endregion

        #region 摧毁对象
        /// <summary>
        /// 摧毁对象
        /// </summary>
        public void Dispose()
        {
            if (Connection != null)
                Connection.Dispose();
        }
        #endregion
    }
}
