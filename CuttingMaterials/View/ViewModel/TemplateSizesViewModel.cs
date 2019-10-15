using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.View.ViewModel
{
    class TemplateSizesViewModel : INotifyPropertyChanged
    {
        Template template;
        public Template Template
        {
            get
            {
                return template;
            }
        }

        public TemplateSizesViewModel(Template template)
        {
            this.template = template;
        }

        //public ObservableCollection<TemplateSize> Sizes
        //{
        //    get
        //    {
        //        var res = new ObservableCollection<TemplateSize>();
        //        foreach (var d in template.Sizes)
        //            res.Add(d);

        //        return res;
        //    }
        //}

        public List<TemplateSize> Sizes
        {
            get
            {
                return template.Sizes?.ToList();
            }
        }

        public void ChangeSizeValue(int sizeId, string runtimeId, int newValue)
        {
            if (sizeId != 0)
            {
                var obj = Sizes.Find(x => x.Id == sizeId);
                if (obj != null)
                    obj.GetValue = newValue;
            }
            if (!string.IsNullOrEmpty(runtimeId))
            {
                var obj = Sizes.Find(x => x.RuntimeId?.Equals(runtimeId) ?? false);
                if (obj != null)
                    obj.GetValue = newValue;
            }
        }

        public void AddNew()
        {
            template.Sizes.Add(new TemplateSize()
            {
                Name = $"Размер {template.Sizes.Count + 1}",
                RuntimeId = $"runtime_{DateTime.Now.ToString().GetHashCode()}"
            });
            PropChanged("Sizes");
        }

        public void Remove(Data.Repository.DatabaseUnitOfWork db, TemplateSize size)
        {
            db.TemplateSizes.Remove(size);
            PropChanged("Sizes");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
