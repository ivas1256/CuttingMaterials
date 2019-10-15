using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Repository
{
    /// <summary>
    /// it's IUnitOfWork as repository pattern tell's
    /// </summary>
    interface IUnitOfWork : IDisposable
    {
        ProjectRepository Projects { get; }

        TemplateRepository Templates { get; }

        CoatingRepository Coatings { get; }

        void Complete();
    }
}
