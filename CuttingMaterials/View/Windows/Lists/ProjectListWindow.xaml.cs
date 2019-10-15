using CuttingMaterials.Data.Model;
using CuttingMaterials.View.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CuttingMaterials.View.Windows.Lists
{
    /// <summary>
    /// Interaction logic for ProjectListWindow.xaml
    /// </summary>
    public partial class ProjectListWindow : Window
    {
        public ProjectListWindow()
        {
            InitializeComponent();
        }

        Logger logger = LogManager.GetCurrentClassLogger();
        public Project SelectedProject { get; set; }
        public string DialogResultAction { get; set; }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataContext = new ProjectListViewModel();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось загрузить список проектов");
            }
        }

        private void lb_projects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_projects.SelectedItem == null)
                return;

            btn_select_Click(sender, null);
        }

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            var prj = lb_projects.SelectedItem as Project;
            if (prj == null)
                return;

            DialogResultAction = "select";
            SelectedProject = prj;
            Close();
        }

        private void btn_deleteClick(object sender, RoutedEventArgs e)
        {
            var prj = lb_projects.SelectedItem as Project;
            if (prj == null)
                return;

            if (!MessagesProvider.Confirm($"Действительно удалить проект \"{prj.Name}\"?"))
                return;

            DialogResultAction = "delete";            
            SelectedProject = prj;
            Close();
        }

        private void lb_projects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.Delete)
                btn_deleteClick(null, null);
        }

        private void tb_searchQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            (DataContext as ProjectListViewModel)?.Projects.Refresh();
        }
    }
}
