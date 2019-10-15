using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof(ProjectMetadata))]
    public partial class Project : DataValidationBase
    {
        public static Project CreateDefaultObj()
        {
            return new Project()
            {
                BlankSize = "1250x2000",
                StartDate = DateTime.Now.Date
            };
        }

        public int BlankWidth
        {
            get
            {
                return int.Parse(BlankSize.Split('x')[1]);
            }
        }

        public int BlankHeight
        {
            get
            {
                return int.Parse(BlankSize.Split('x')[0]);
            }
        }

        public List<double> BlankThicknessItemsSource
        {
            get
            {
                return new List<double>()
                {
                    0.35,0.4,0.45,0.5
                };
            }
        }
    }

    internal sealed class ProjectMetadata
    {
        [Required]
        [Display(Name = "Название проекта")]
        public string Name { get; set; }
        //[Range(0, 9999)]
        [Display(Name = "Толщина листа")]
        public Nullable<double> BlankThickness { get; set; }
        //[Required]
        public Nullable<int> CoatingId { get; set; }
        //[Required]
        [Display(Name = "Цвет покрытия")]
        public string CoatingColor { get; set; }
        //[Required]
        [Display(Name = "ФИО заказчика")]
        public string CustomerFio { get; set; }
        //[Required]
        [Display(Name = "Номер заказа")]
        public string OrderNumber { get; set; }
        //[Required]
        [Display(Name = "Дата отправки в производство")]
        public Nullable<System.DateTime> StartDate { get; set; }
        //[Required]
        [Display(Name = "Запланированная дата готовности")]
        public Nullable<System.DateTime> EndDate { get; set; }

        //[Required]
        public string BlankSize { get; set; }
    }
}
