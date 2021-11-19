using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.Data.SqlClient;


namespace IM.SQL
{
    public class DbResponse
    {
        public DataSet Ds { get; set; }
        public bool Error { get; set; }
        public string ErrorMsg { get; set; }
    }
    public class DataAccessMethods
    {
        public static DbResponse ExecuteProcedure(string procedureName, SortedList<string, object> parameters)
        {
            var ds = new DataSet();
            //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["IMConString"].ConnectionString);
            //var con = new SqlConnection("data source=localhost;initial catalog=IM;persist security info=True; Integrated Security=SSPI;");
            var con = new SqlConnection("Server=localhost;Database=IM;User Id=sa;Password=pioneer007;");
            var cmd = con.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 5;

            if (parameters != null)
            {
                foreach (string k in parameters.Keys)
                    cmd.Parameters.AddWithValue(k, parameters[k]);
            }
            var da = new SqlDataAdapter(cmd);
            try
            {
                da.Fill(ds);
            }
            catch(Exception ex)
            {
                return new DbResponse { Ds = null, Error = true , ErrorMsg = ex.Message };
            }
            finally
            {
                con.Dispose();
            }

            return new DbResponse { Ds = ds, Error = false , ErrorMsg = null};
        }
    }// end of class
}