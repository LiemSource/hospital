using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("patient", Schema = "hospital")]
    public class Patient
    {
        [Key]
        [Required]
        [Column("id", TypeName = "varchar(15)")]
        public string Id { get; set; }

        [Column("in_patient_id", TypeName = "varchar(20)")]
        public string InPatientId { get; set; }

        [Column("schema_id", TypeName = "varchar(10)")]
        public string SchemaId { get; set; }

        [Column("visit_id", TypeName = "varchar(15)")]
        public string VisitId { get; set; }

        [Column("see_no", TypeName = "varchar(4)")]
        public string SeeNo { get; set; }

        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Column("chief_doct_name", TypeName = "varchar(50)")]
        public string ChiefDoctName { get; set; }

        [Column("id_card", TypeName = "varchar(20)")]
        public string IdCard { get; set; }

        [Column("patient_type")]
        public PatientType PatientType { get; set; } //PatientType=0 门诊 1住院

        [Column("dept_id", TypeName = "varchar(10)")]
        public string DeptId { get; set; }

        [Column("dept_name", TypeName = "varchar(20)")]
        public string DeptName { get; set; }

        [Column("diagnosis", TypeName = "varchar(100)")]
        public string Diagnosis { get; set; }

        [Column("in_date", TypeName = "varchar(30)")]
        public string InDate { get; set; }

        [Column("out_date", TypeName = "varchar(20)")]
        public string OutDate { get; set; }

        [Column("remark")]
        public string Remark { get; set; }

        [Column("medication_max_time")]
        public DateTime? MedicationMaxTime { get; set; }

        [Column("temp_medication_max_time")]
        public DateTime? TempMedicationMaxTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }

        [Column("update_time")]
        public DateTime UpdateTime { get; set; }

        [Column("medication_failed")]
        public bool MedicationFailed { get; set; }

        [NotMapped]
        public string PatientContent { get; set; }
        [NotMapped]
        public DateTime? InDateFormate { get; set; }
    }

    public enum PatientType
    {
        /// <summary>
        /// 门诊
        /// </summary>
        outpatient,
        /// <summary>
        /// 住院
        /// </summary>
        inpatient,
        /// <summary>
        /// 已出院
        /// </summary>
        outofpatient
    }
}
