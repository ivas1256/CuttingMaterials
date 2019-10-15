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
    class TemplatesListViewModel
    {
        public string Query { get; set; }

        List<Template> templates;
        public ICollectionView Templates
        {
            get
            {
                var col = CollectionViewSource.GetDefaultView(templates);
                col.Filter = (object x) =>
                {
                    if (string.IsNullOrEmpty(Query))
                        return true;

                    return ((Template)x)?.Name?.ToUpper().Contains(Query.ToUpper()) ?? false;
                };

                return col;
            }
        }

        DatabaseUnitOfWork db;

        public TemplatesListViewModel(bool isAddPlaceholder)
        {
            templates = new List<Template>();
            if (isAddPlaceholder)
                templates.Add(new Template()
                {
                    Name = "Без шаблона",
                    Id = -1
                });

            db = new DatabaseUnitOfWork();
            var tmp = db.Templates.GetAllAsNoTracking();
            foreach (var t in tmp)
                templates.Add(t);
        }
    }
}
