using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CuttingMaterials.Data.Repository
{
    public class TemplateRepository : Repository<Template>
    {
        public TemplateRepository(DataContext dbContext) : base(dbContext)
        {
        }

        

        public Template GetWithSizes(int id)
        {
            return dbContext.Set<Template>().Include("Sizes")
                .Where(x => x.Id == id).FirstOrDefault();
        }
    }
}
