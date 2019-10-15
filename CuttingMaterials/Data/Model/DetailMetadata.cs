using DotLiquid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof(DetailMetadata))]
    public partial class Detail : DataValidationBase, INotifyPropertyChanged, ILiquidizable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Index { get; set; }
        public string ImageFileName { get; set; }
        public int ListsToDo { get; set; }

        public object ToLiquid()
        {
            return new Dictionary<string, object>()
            {
                { "Width", Sizes.Select(x => x.GetValue).Sum() },
                { "Amount", Amount },
                { "Name", Name },
                { "SizesListNoName", string.Join("x", Sizes.Select(x => x.GetValue))},
                { "SizesListWithName", string.Join(", ", Sizes.Select(x => $"{x.Name} = {x.GetValue}мм"))},
                { "Index", Index },
                { "ImageFileName", ImageFileName },
                { "ListsToDo", ListsToDo }
            };
        }

        public bool IsDrawingEnabled
        {
            get
            {
                return string.IsNullOrEmpty(Template?.FullFileName);
            }
        }

        public bool IsFileExist
        {
            get
            {
                var path = Path.Combine(App.CurrDirectory, Template.ImageFileName ?? PreviewFile);
                return File.Exists(path);
            }
        }

        public string GetPreviewImageFileName()
        {
            return $"{Name}_previewDetail.jpg";
        }
    }

    internal sealed class DetailMetadata
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Range(1, 9999999)]
        [Display(Name = "Количество")]
        public int Amount { get; set; }
    }
}
