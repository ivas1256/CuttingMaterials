using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CuttingMaterials.View.ViewModel
{
    class ProjectListViewModel
    {
        public string Query { get; set; }

        List<Project> projects;
        public ICollectionView Projects
        {
            get
            {
                var col = CollectionViewSource.GetDefaultView(projects);
                col.Filter = (object x) => ((Project)x)?.Name?.Contains(Query ?? "") ?? false;

                return col;
            }
        }

        public Project Project { get; set; }
        DatabaseUnitOfWork db;

        public ProjectListViewModel()
        {
            projects = new List<Project>();
            db = new DatabaseUnitOfWork();
            var tmp = db.Projects.GetAllAsNoTracking();
            foreach (var t in tmp)
                projects.Add(t);
        }
    }
}
