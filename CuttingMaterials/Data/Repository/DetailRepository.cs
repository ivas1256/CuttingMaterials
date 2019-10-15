using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace CuttingMaterials.Data.Repository
{
    public class DetailRepository : Repository<Detail>
    {
        public DetailRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
