using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dbfHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadDBf();
            //WriteDbf();
        }

        #region 读取原文件为datatable并打印
        private static void ReadDBf()
        {
            DbfHelper helper = new DbfHelper(@"E:\download");
            DataTable dt = helper.GetDateTableByDBF("DATAS.DBF");
            int columns = 0;  //总列数
            if (dt != null && dt.Rows.Count > 0)
            {
                columns = dt.Columns.Count;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Console.WriteLine(string.Format("====================当前为第{0}行数据=====================", i+1));
                    for (int j = 0; j < columns; j++)
                    {
                        Console.WriteLine(string.Format("当前列名为：{0},列的内容为：{1}", dt.Columns[j].ColumnName, dt.Rows[i][j].ToString()));
                    }
                    
                }
                Console.ReadKey();
            }
        }
        #endregion

        #region 写数据操作
        private static void WriteDbf()
        {
            DataTable dt = new DataTable();
            dt = CreateTable();
            DbfHelper helper = new DbfHelper(@"E:\download");
            helper.CreateNewTable(dt);
            helper.fillData(dt);
        }
        #endregion

        #region 创建table
        private static DataTable CreateTable()
        {
            DataTable tblDatas = new DataTable("Datas");
            DataColumn dc = null;
            DataRow newRow;

            #region 创建表
            dc = tblDatas.Columns.Add("HH1", Type.GetType("System.DateTime"));      //
            dc = tblDatas.Columns.Add("HH2", Type.GetType("System.String"));     //
            dc = tblDatas.Columns.Add("HH3", Type.GetType("System.Double"));       //
            dc = tblDatas.Columns.Add("HH4", Type.GetType("System.Boolean"));  //
            #endregion
            
            #region 第一个记录
            newRow = tblDatas.NewRow();
            newRow["HH1"] = DateTime.Now;
            newRow["HH2"] = "这里是字符串";
            newRow["HH3"] = 1.00000;
            newRow["HH4"] = false;
            tblDatas.Rows.Add(newRow);
            #endregion

            #region 第二个记录
            newRow = tblDatas.NewRow();
            newRow["HH1"] = DateTime.Now;
            newRow["HH2"] = "这里是字符串2";
            newRow["HH3"] = 1.00000;
            newRow["HH4"] = false;
            tblDatas.Rows.Add(newRow);
            #endregion

            return tblDatas;
        }
        #endregion
    }
}
