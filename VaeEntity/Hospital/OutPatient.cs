using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("out_patient", Schema = "hospital")]
    public class OutPatient
    {
        [Key]
        [Required]
        [Column("id", TypeName = "varchar(15)")]
        public string Id { get; set; }

        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Column("patient_type")]
        public PatientType PatientType { get; set; }

        [Column("dept_id", TypeName = "varchar(10)")]
        public string DeptId { get; set; }

        [Column("dept_name", TypeName = "varchar(20)")]
        public string DeptName { get; set; }

        [Column("diagnosis", TypeName = "varchar(100)")]
        public string Diagnosis { get; set; }

        [Column("in_date", TypeName = "varchar(20)")]
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
    }
}
