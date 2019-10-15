using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.View.ViewModel
{
    public class DetailViewModel : INotifyPropertyChanged
    {
        public Project Project { get; set; }
        DatabaseUnitOfWork db;
        public List<Detail> List
        {
            get
            {
                return Project.Details?.ToList();
            }
        }

        Detail detail;
        public Detail Detail
        {
            get
            {
                return detail;
            }
        }
        public List<DetailSize> Sizes
        {
            get
            {
                return detail?.Sizes.ToList();
            }
        }

        public DetailViewModel(Project project, Detail detail, DatabaseUnitOfWork db)
        {
            this.detail = detail;
            this.Project = project;
            this.db = db;
        }

        public void SetXaml(int detailId, string xaml)
        {
            if (List != null)
            {
                var obj = List.Find(x => x.Id == detailId);
                if (obj != null)
                    obj.Xaml = xaml;
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

        public void Add(Detail detail)
        {
            Project.Details.Add(detail);
            PropChanged("List");
        }

        public void Remove(Detail detail)
        {
            db.DetailSizes.RemoveRange(detail.Sizes);
            db.Details.Remove(detail);
            db.Complete();
            PropChanged("List");
        }

        public void AddNewSize()
        {
            Detail.Sizes.Add(new DetailSize()
            {
                Name = $"Размер {Detail.Sizes.Count + 1}",
                RuntimeId = $"runtime_{DateTime.Now.ToString().GetHashCode()}"
            });
            PropChanged("Sizes");
        }

        public void Remove(DetailSize size)
        {
            db.DetailSizes.Remove(size);
            db.Complete();
            PropChanged("Sizes");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
