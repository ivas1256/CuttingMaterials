using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.Logic;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.ComponentModel;

namespace CuttingMaterials.View.ViewModel
{
    public class ProjectViewModel : INotifyPropertyChanged
    {
        public Project Project { get; set; }
        public bool IsEnabled
        {
            get
            {
                return Project != null;
            }
        }
        bool hasInfError;
        public bool HasInfError
        {
            get
            {
                return hasInfError;
            }
            set
            {
                hasInfError = value;
                PropChanged("HasInfError");
            }
        }
        string status;
        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
                PropChanged("Status");
            }
        }
        string statusIconCode;
        public string StatusIconCode
        {
            get
            {
                return statusIconCode;
            }

            set
            {
                statusIconCode = value;
                PropChanged("StatusIconCode");
            }
        }

        DatabaseUnitOfWork db;
        public DatabaseUnitOfWork DataContext
        {
            get
            {
                return db;
            }

            set
            {
                db = value;
            }
        }

        public DetailViewModel Details { get; set; }
        public OffcutViewModel Offcuts { get; set; }

        public ProjectViewModel(Project project = null, bool isLoadFromDB = true)
        {
            db = new DatabaseUnitOfWork();

            if (project == null)
                Project = db.Projects.GetFirstWithData();
            else
            {
                if (project.Id != 0 && isLoadFromDB)
                    Project = db.Projects.GetWithData(project.Id);
                else
                    Project = project;
            }

            Details = new DetailViewModel(Project, null, db);
            Offcuts = new OffcutViewModel(db);
        }

        public static Project GetFirstProject()
        {
            using (var db = new DatabaseUnitOfWork())
                return db.Projects.GetFirstWithData();
        }

        public bool CustomValidate()
        {
            if (string.IsNullOrEmpty(Project.Name) || string.IsNullOrEmpty(Project.BlankSize)
                || string.IsNullOrEmpty(Project.CoatingColor) || string.IsNullOrEmpty(Project.CustomerFio)
                || string.IsNullOrEmpty(Project.OrderNumber) || Project.StartDate == null ||
                Project.EndDate == null || Project.CoatingId == null)
                return false;

            return true;
        }

        public void SaveChanges()
        {
            if (Project.Id == 0)
                db.Projects.Add(Project);

            db.Complete();
        }

        public void Delete(Project project)
        {
            var tmp = project.Id;
            db.Projects.RemoveRange(db.Projects.Find(x => x.Id == tmp));
            db.Complete();
        }

        public Coating GetCoating()
        {
            return db.Coatings.Get(Project.CoatingId.Value);
        }

        public Template GetTemplate(int id)
        {
            return db.Templates.Get(id);
        }

        #region cutting
        public List<int[]> CuttingResult { get; set; }
        public List<BitmapSource> Bitmaps { get; set; }
        Cutter cutter;

        public bool IsCuttingCalc
        {
            get
            {
                return cutter != null;
            }
        }

        public void CalcCutting()
        {
            cutter = new Cutter(this, false);
            if (CuttingResult != null && CuttingResult.Count != 0)
                cutter.CalculateExisting(CuttingResult);
            else
                cutter.CalculateNew();

            CuttingResult = cutter.Result;
        }

        public void RenderCutting(Canvas canvas, int canvasWidth, bool isSplitToFiles)
        {
            cutter.Render(canvas, canvasWidth, isSplitToFiles);

            Bitmaps = cutter.Bitmaps;
        }

        public void ClearCutting()
        {
            CuttingResult = null;
            Bitmaps = null;
        }

        public string CalcOffcut()
        {
            return cutter.CalcOffcut(db.DefaultDetails.GetAll().ToList());
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        void PropChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
