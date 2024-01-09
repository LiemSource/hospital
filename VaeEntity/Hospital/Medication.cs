using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("medication", Schema = "hospital")]
    public class Medication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Column("id")]
        public int Id { get; set; }

        [Column("patient_id", TypeName = "varchar(15)")]
        public string PatientId { get; set; }

        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; }

        [Column("doct_name", TypeName = "varchar(50)")]
        public string DoctName { get; set; }

        [Column("group_id", TypeName = "varchar(20)")]
        public string GroupId { get; set; }

        /// <summary>
        /// 剂量
        /// </summary>
        [Column("dosage", TypeName = "varchar(10)")]
        public string Dosage { get; set; }

        [Column("frequency", TypeName = "varchar(20)")]
        public string Frequency { get; set; }

        [Column("usage", TypeName = "varchar(50)")]
        public string Usage { get; set; }

        [Column("specs", TypeName = "varchar(50)")]
        public string Specs { get; set; }

        [Column("mo_note", TypeName = "varchar(200)")]
        public string MoNote { get; set; }

        [Column("place_date_time", TypeName = "varchar(20)")]
        public string PlaceDateTime { get; set; }

        [Column("begin_date_time", TypeName = "varchar(20)")]
        public string BeginDateTime { get; set; }

        [Column("end_date_time", TypeName = "varchar(20)")]
        public string EndDateTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
