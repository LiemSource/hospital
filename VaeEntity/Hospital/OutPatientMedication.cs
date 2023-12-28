using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaeEntity.Hospital
{
    [Table("out_patient_medication", Schema = "hospital")]
    public class OutPatientMedication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("patient_id", TypeName = "varchar(15)")]
        public string PatientId { get; set; }

        [Column("in_patient_id", TypeName = "varchar(20)")]
        public string InPatientId { get; set; }

        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Column("doct_name", TypeName = "varchar(50)")]
        public string DoctorName { get; set; }

        [Column("group_id", TypeName = "varchar(20)")]
        public string GroupId { get; set; }
        /// <summary>
        /// 剂量
        /// </summary>
        [Column("dosage", TypeName = "varchar(10)")]
        public string Dosage { get; set; }

        [Column("frequency", TypeName = "varchar(20)")]
        public string Frequency { get; set; } //频次

        [Column("usage", TypeName = "varchar(50)")]
        public string Usage { get; set; }  

        [Column("unit", TypeName = "varchar(20)")]
        public string Unit { get; set; }

        [Column("begin_date_time", TypeName = "varchar(20)")]
        public string BeginDateTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("prescript_url", TypeName = "varchar(200)")]
        public string PrescriptUrl { get; set; }

        [Column("usage_detail", TypeName = "varchar(200)")]
        public string UsageDetail { get; set; }

        [Column("prescript")]
        public string Prescript { get; set; }
    }
}
