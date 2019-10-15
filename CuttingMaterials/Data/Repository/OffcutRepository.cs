using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CuttingMaterials.Data.Repository
{
    public class OffcutRepository : Repository<Offcut>
    {
        public OffcutRepository(DataContext dbContext) : base(dbContext)
        {
        }

        public List<Offcut> GetAllWithProject()
        {
            return dbContext.Set<Offcut>().Include("Project").ToList();
        }
    }
}
