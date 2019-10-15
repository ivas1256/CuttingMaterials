using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof(TemplateMetadata))]
    public partial class TemplateSize : DataValidationBase, INotifyPropertyChanged
    {
        bool isEditing;
        public bool IsEditing
        {
            get
            {
                return isEditing;
            }

            set
            {
                isEditing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEditing"));
            }
        }

        public string RuntimeId { get; set; }

        public int GetValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
                PropChanged("GetValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    internal sealed class TemplateSizeMetadata
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}
