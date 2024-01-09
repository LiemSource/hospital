using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using VaeEntity.Hospital;

namespace VaeDbContext
{
    public class HospitalContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;
        public HospitalContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HospitalContext(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (_loggerFactory != null)
            //    optionsBuilder.UseLoggerFactory(_loggerFactory);//输出执行的sql
            optionsBuilder.UseMySQL(_configuration["ConnectionStrings:HospitalConnection"], b => b.MigrationsAssembly("HospitalJob"));
            
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            return base.Set<T>();
        }
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_configuration["ConnectionStrings:HospitalConnection"]);
        }
        public MySqlParameter CreateParameter(string name, Object value)
        {
            return new MySqlParameter(name, value);
        }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<TempMedication> TempMedications { get; set; }
        public DbSet<OutPatientMedication> OutPatientMedications { get; set; }
        public DbSet<QuerySchemaInfo> QuerySchemaInfos { get; set; }
        public DbSet<DoctSchedule> DoctSchedules { get; set; }
        public DbSet<DoctReservationTask> DoctReservationTasks { get; set; }
        public DbSet<PatientVisitInfo> PatientVisitInfos { get; set; }
    }
}
