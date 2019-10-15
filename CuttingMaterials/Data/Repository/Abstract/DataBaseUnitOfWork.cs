using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Repository
{
    public class DatabaseUnitOfWork : IUnitOfWork
    {
        private readonly DataContext context;
        public DatabaseUnitOfWork()
        {
            context = new DataContext();

            Projects = new ProjectRepository(context);
            Templates = new TemplateRepository(context);
            Coatings = new CoatingRepository(context);
            Details = new DetailRepository(context);
            DetailSizes = new DetailSizeRepository(context);
            TemplateSizes = new TemplateSizeRepository(context);
            DefaultDetails = new DefaultDetailRepository(context);
            Offcuts = new OffcutRepository(context);
            Nodes = new Repository<Node>(context);
        }        

        public ProjectRepository Projects { get; private set; }
        public TemplateRepository Templates { get; private set; }
        public CoatingRepository Coatings { get; private set; }
        public DetailRepository Details { get; private set; }
        public DetailSizeRepository DetailSizes { get; private set; }
        public TemplateSizeRepository TemplateSizes { get; private set; }
        public DefaultDetailRepository DefaultDetails { get; private set; }
        public OffcutRepository Offcuts { get; private set; }
        public Repository<Node> Nodes { get; private set; }

        public DataContext Context
        {
            get
            {
                return context;
            }
        }

        public void Sql(string sql, params object[] args)
        {
            context.Database.ExecuteSqlCommand(sql, args);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        public void Complete()
        {
            context.SaveChanges();
        }
    }
}
