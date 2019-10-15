using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.View.ViewModel
{
    class CoatingListViewModel
    {
        public ObservableCollection<Coating> Coatings { get; set; } = new ObservableCollection<Coating>();

        public Project Project { get; set; }
        DatabaseUnitOfWork db;

        public CoatingListViewModel(Project project)
        {
            this.Project = project;
            db = new DatabaseUnitOfWork();
            var tmp = db.Coatings.GetAllAsNoTracking();
            foreach (var t in tmp)
                Coatings.Add(t);
        }

        public void Delete(Coating coating)
        {
            Coatings.Remove(coating);

            db.Context.Coating.Attach(coating);
            db.Coatings.Remove(coating);            
            db.Complete();
        }
    }
}
