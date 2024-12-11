using Microsoft.Data.SqlClient;
using Serilog;
using System.Data;
using TranslatorAPI.Helpers;
using TranslatorAPI.Models;

namespace TranslatorAPI.Services
{
    public class EmployeeService
    {
        private readonly string _connectionString;
        public EmployeeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("PACEDatabase");
        }

        public async Task<List<Employees>> SearchEmployees(string search)
        {
            var employees = new List<Employees>();
            try
            {
                using (var sqlCommand = new SqlCommand())
                {
                    sqlCommand.CommandText = $@"
                                                 Select * from vwGHRDB_Employees where FullName like '%{search}%'  and PrimaryEmail = 1 and ISNULL(EmploymentStatus, 'T') in ('A', 'L', 'P') 
                                                and ISNULL(DeleteFlag, 'Y') = 'N' 
                                                ";

                    using (var reader = EYSql.ExecuteReader(_connectionString, CommandType.Text, sqlCommand.CommandText))
                    {
                        while (await reader.ReadAsync())
                        {
                            var employee = new Employees
                            {
                                GUI = reader.GetString(reader.GetOrdinal("GUI")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.InnerException?.ToString());
            }

            return employees;
        }
    }
}
