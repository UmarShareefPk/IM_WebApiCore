using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.Data.SqlClient;
using WebApi.Options;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using IM_Core.Common;
using Microsoft.Extensions.Configuration;

namespace IM.SQL
{
    public class DbResponse
    {
        public DataSet Ds { get; set; }
        public bool Error { get; set; }
        public string ErrorMsg { get; set; }

        public static explicit operator DbResponse(Task<object> v)
        {
            throw new NotImplementedException();
        }
    }
    public class DataAccessMethods
    {
        private readonly  ConnectionStringOptions _connectionString;
        private IConfiguration _configuration;

        public DataAccessMethods(IOptionsSnapshot<ConnectionStringOptions> connectionStringOptions, IConfiguration configuration)
        {
            _connectionString = connectionStringOptions.Value;
            _configuration = configuration;
        }
        public async Task<DbResponse> ExecuteProcedureAsyncold(string procedureName, SortedList<string, object> parameters)
        {          
            var ds = new DataSet();
            //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["IMConString"].ConnectionString);
            //var con = new SqlConnection("data source=localhost;initial catalog=IM;persist security info=True; Integrated Security=SSPI;");
          //  var con = new SqlConnection(_connectionString.Main);
            var con = new SqlConnection(_configuration.GetValue<string>("ConnectionStringAzure"));
            var cmd = con.CreateCommand();
            cmd.CommandText = procedureName;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 5;

            if (parameters != null)
            {
                foreach (string k in parameters.Keys)
                    cmd.Parameters.AddWithValue(k, parameters[k]);
            }
           // var reader = cmd.ExecuteReaderAsync();
            var da = new SqlDataAdapter(cmd);
            try
            {
                // da.Fill(ds);
                await Task.Run(() => da.Fill(ds));

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

        public async Task<DbResponse> ExecuteProcedureAsync(string procedureName, SortedList<string, object> parameters)
        {
            var ds = new DataSet();
            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetValue<string>("ConnectionStringAzure")))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (string k in parameters.Keys)
                                cmd.Parameters.AddWithValue(k, parameters[k]);
                        }

                        await con.OpenAsync();
                        var dr = await cmd.ExecuteReaderAsync();
                     

                        while (!dr.IsClosed)
                            ds.Tables.Add().Load(dr);
                    }
                }

            }
            catch (Exception ex)
            {
                return new DbResponse { Ds = null, Error = true, ErrorMsg = ex.Message };
            }            

            return new DbResponse { Ds = ds, Error = false, ErrorMsg = null };
        }

        public async Task<object> ExecuteProcedureAsync2(string procedureName, SortedList<string, object> parameters, Func<SqlDataReader, Task<object>> returnData)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(_configuration.GetValue<string>("ConnectionStringAzure")))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (string k in parameters.Keys)
                                cmd.Parameters.AddWithValue(k, parameters[k]);
                        }

                        await con.OpenAsync();
                        var dr = await cmd.ExecuteReaderAsync();
                        var ds = new DataSet();

                        //while (!dr.IsClosed)
                        //   ds.Tables.Add().Load(dr);

                        return await returnData(dr);
                        //dr.NextResult();
                        //while (dr.Read())
                        //{
                        //    var s = dr["Title"].ToString();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {                
                return new DbError { Message = ex.Message, InnerException = ex.InnerException };
            }         

        }
    }// end of class
}