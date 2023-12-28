using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("patient_visit_info", Schema = "hospital")]
    public class PatientVisitInfo
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("visit_id", TypeName = "varchar(15)")]
        public string VisitId { get; set; }

        [Column("is_reviewed")]
        public bool IsReviewed { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }
    }
}
