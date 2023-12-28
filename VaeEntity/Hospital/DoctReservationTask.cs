using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("doct_reservation_task", Schema = "hospital")]
    public class DoctReservationTask
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("telephone", TypeName = "varchar(15)")]
        public string Telephone { get; set; }

        [Column("password", TypeName = "varchar(50)")]
        public string Password { get; set; }

        [Column("patient_name", TypeName = "varchar(50)")]
        public string PatientName { get; set; }

        [Column("visit_date")]
        public DateTime VisitDate { get; set; }

        [Column("doct_id", TypeName = "varchar(10)")]
        public string DoctId { get; set; }

        [Column("doct_name", TypeName = "varchar(20)")]
        public string DoctName { get; set; }

        [Column("dept_id", TypeName = "varchar(10)")]
        public string DeptId { get; set; }

        [Column("dept_name", TypeName = "varchar(50)")]
        public string DeptName { get; set; }

        [Column("status")]
        public ReservationStatus Status { get; set; }

        [Column("appointment_time")]
        public DateTime? AppointmentTime { get; set; }

        [Column("appointment")]
        public string Appointment { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
