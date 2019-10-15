using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof(DetailSizeMetadata))]
    public partial class DetailSize : DataValidationBase, INotifyPropertyChanged
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
                PropChanged("IsEditing");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                //PropChanged("GetValueForDesigner");S
            }
        }

        public int GetValueForDesigner
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
               // PropChanged("GetValue");
                PropChanged("GetValueForDesigner");
            }
        }
    }

    internal sealed class DetailSizeMetadata
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}
