using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace VaeEntity.Hospital
{
    [Table("query_schema_info", Schema = "hospital")]
    public class QuerySchemaInfo
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Column("last_query_schema_id")]
        public int LastQuerySchemaId { get; set; }

        [Column("last_query_schema_name", TypeName = "varchar(200)")]
        public string LastQuerySchemaName { get; set; }

        [Column("excetion_schema_ids")]
        public string ExcetionSchemaIds { get; set; }

        [Column("last_query_time")]
        public DateTime LastQueryTime { get; set; }

        [Column("create_time")]
        public DateTime CreateTime { get; set; }
    }
}
