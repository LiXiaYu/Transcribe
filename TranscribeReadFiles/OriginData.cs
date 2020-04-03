using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranscribeReadFiles
{

    [Table("OriginData")]
    public class OriginData
    {
        [Key]
        [StringLength(256)]
        public string FilesId { get; set; }
        [Required]
        [Index("INDEX_REGNUM", IsClustered = true)]
        public int CaseHistoryId { get; set; }
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        public int Sex { get; set; }
        public int Years { get; set; }
        [Required]
        [StringLength(50)]
        public string MajorDiagnosisCoding { get; set; }
        [Required]
        [StringLength(50)]
        public string MajorDiagnosis { get; set; }

        [StringLength(5000000)]
        public string HistoryOfPastIllness { get; set; }

        public double? Temperature { get; set; }
        public double? Pulse { get; set; }
        public double? Breath { get; set; }
        public double? Blood_Pressure_Systolic { get; set; }
        public double? Blood_Pressure_Diastolic { get; set; }

        public double? Width { get; set; }
        public double? Height { get; set; }

        public double? Corneal_Endothelium_Right { get; set; }
        public double? Corneal_Endothelium_Left { get; set; }

        public double? Right_Intraocular_Pressure { get; set; }
        public double? Left_Intraocular_Pressure { get; set; }
    }

    public class OriginDataContext : DbContext
    {
        public OriginDataContext() : base("name=OriginDataContext")
        {
            Database.SetInitializer<OriginDataContext>(null);
        }
        public DbSet<OriginData> OriginDatas { get; set; }

    }

}
