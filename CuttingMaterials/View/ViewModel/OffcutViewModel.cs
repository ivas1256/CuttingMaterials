using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using System.ComponentModel;
using System.Windows.Data;

namespace CuttingMaterials.View.ViewModel
{
    public class OffcutViewModel : INotifyPropertyChanged
    {
        public string Query { get; set; }

        DatabaseUnitOfWork db;
        List<Offcut> offcuts;
        public ICollectionView Offcuts
        {
            get
            {
                var col = CollectionViewSource.GetDefaultView(offcuts);
                col.Filter = (object x) =>
                {
                    if (string.IsNullOrEmpty(Query))
                        return true;

                    var offcut = (Offcut)x;
                    if (offcut.Size.Contains(Query) ||
                        offcut.Project.Name.Contains(Query))
                        return true;
                    return false;
                };

                return col;
            }
        }

        public OffcutViewModel()
        {
            db = new DatabaseUnitOfWork();
            offcuts = new List<Offcut>();
            foreach (var offcut in db.Offcuts.GetAllWithProject())
                offcuts.Add(offcut);
        }

        public OffcutViewModel(DatabaseUnitOfWork db)
        {
            this.db = db;
            offcuts = new List<Offcut>();
            foreach (var offcut in db.Offcuts.GetAllWithProject())
                offcuts.Add(offcut);
        }

        public void Add(Offcut obj)
        {
            db.Offcuts.Add(obj);
            PropChanged("Offcuts");
        }

        public void Remove(Project project)
        {
            db.Offcuts.RemoveRange(db.Offcuts.Find(x => x.ProjectId == project.Id));
            PropChanged("Offcuts");
        }

        public void SaveChanges()
        {
            db.Complete();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
