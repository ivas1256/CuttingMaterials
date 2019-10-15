using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof (CoatingMetadata))]
    public partial class Coating : DataValidationBase
    {
    }

    internal sealed class CoatingMetadata
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}
