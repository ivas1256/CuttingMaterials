using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CuttingMaterials.Data.Repository
{
    public class ProjectRepository : Repository<Project>
    {
        public ProjectRepository(DataContext dbContext) : base(dbContext)
        {
        }

        public Project GetWithData(int id)
        {
            return ((DataContext)dbContext).Project.Include("Details.Sizes")
                .Include("Details.Template")
                .Where(x => x.Id == id).FirstOrDefault();
        }

        public Project GetFirstWithData()
        {
            return ((DataContext)dbContext).Project.Include("Details.Sizes")
                .Include("Details.Template").FirstOrDefault();
        }
    }
}
