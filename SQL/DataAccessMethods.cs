﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.Data.SqlClient;
using WebApi.Options;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

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

        public DataAccessMethods(IOptionsSnapshot<ConnectionStringOptions> connectionStringOptions)
        {
            _connectionString = connectionStringOptions.Value;

        }
        public async Task<DbResponse> ExecuteProcedureAsync(string procedureName, SortedList<string, object> parameters)
        {          
            var ds = new DataSet();
            //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["IMConString"].ConnectionString);
            //var con = new SqlConnection("data source=localhost;initial catalog=IM;persist security info=True; Integrated Security=SSPI;");
            var con = new SqlConnection(_connectionString.Main);
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

        public  DbResponse ExecuteProcedureAsync2(string procedureName, SortedList<string, object> parameters)
        {
            var ds = new DataSet();
            //var con = new SqlConnection(ConfigurationManager.ConnectionStrings["IMConString"].ConnectionString);
            //var con = new SqlConnection("data source=localhost;initial catalog=IM;persist security info=True; Integrated Security=SSPI;");
            
          //  cmd.CommandTimeout = 0;
                    

            using (SqlConnection con2 = new SqlConnection(_connectionString.Main))
            {
                
                using (SqlCommand cmd2 = new SqlCommand(procedureName, con2))
                {
                    cmd2.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (string k in parameters.Keys)
                            cmd2.Parameters.AddWithValue(k, parameters[k]);
                    }


                    con2.Open();
                    var dr =   cmd2.ExecuteReader();
                    dr.NextResult();
                    while (dr.Read())
                    {
                        var s = dr["Title"].ToString();
                    }
                }
            }       

            try
            {
                // da.Fill(ds);
               // await Task.Run(() => da.Fill(ds));

            }
            catch (Exception ex)
            {
                return new DbResponse { Ds = null, Error = true, ErrorMsg = ex.Message };
            }
            finally
            {
               // con2.Dispose();
            }

            return new DbResponse { Ds = ds, Error = false, ErrorMsg = null };
        }
    }// end of class
}