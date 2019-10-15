using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CuttingMaterials.Data.Repository
{
    public class CoatingRepository : Repository<Coating>
    {
        public CoatingRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
