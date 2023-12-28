using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace VaeEntity.Hospital
{
    [Table("doct_schedule", Schema = "hospital")]
    public class DoctSchedule
    {
        //Id,DoctId,DoctName,DeptId,DeptName,RegPointID,OrgId,OrgName,EntityName,NoonCode,VisitDate,CreateTime
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("id")]
        [Column("doct_id", TypeName = "varchar(10)")]
        public string DoctId { get; set; }

        [JsonProperty("name")]
        [Column("doct_name", TypeName = "varchar(20)")]
        public string DoctName { get; set; }

        [Column("dept_id", TypeName = "varchar(10)")]
        public string DeptId { get; set; }

        [Column("dept_name", TypeName = "varchar(50)")]
        public string DeptName { get; set; }

        [Column("reg_point_id", TypeName = "varchar(10)")]
        public string RegPointID { get; set; }

        [Column("org_id", TypeName = "varchar(2)")]
        public string OrgId { get; set; }

        [Column("org_name", TypeName = "varchar(10)")]
        public string OrgName { get; set; }

        [Column("entity_name", TypeName = "varchar(500)")]
        public string EntityName { get; set; }

        [Column("noon_code", TypeName = "varchar(5)")]
        public string NoonCode { get; set; }

        [Column("visit_date")]
        public DateTime VisitDate { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
