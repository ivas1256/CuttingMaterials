using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CuttingMaterials.View.ViewModel
{
    class DefaultDetailsViewModel : INotifyPropertyChanged
    {       
        public string Query { get; set; }

        List<DefaultDetail> defaultDetails;
        public ICollectionView DefaultDetails
        {
            get
            {
                var col = CollectionViewSource.GetDefaultView(defaultDetails);
                col.Filter = (object x) =>
                {
                    if (string.IsNullOrEmpty(Query))
                        return true;

                    return ((DefaultDetail)x)?.Name?.Contains(Query ?? "") ?? false;
                };

                return col;
            }
        }

        DatabaseUnitOfWork db;
        public DefaultDetailsViewModel(DatabaseUnitOfWork db)
        {
            this.db = db;
            defaultDetails = new List<DefaultDetail>();
            foreach (var dd in db.DefaultDetails.GetAllAsNoTracking())
                defaultDetails.Add(dd);
        }

        public void AddNew()
        {
            db.DefaultDetails.Add(new DefaultDetail() { Name = $"Деталь {db.DefaultDetails.Count() + 1}" });
            db.Complete();
            PropChanged("DefaultDetails");
        }

        public void Remove(DefaultDetail defDetail)
        {
            db.DefaultDetails.Remove(defDetail);
            db.Complete();
            PropChanged("DefaultDetails");
        }

        public void SaveChanges()
        {
            db?.Complete();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
