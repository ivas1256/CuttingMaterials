using CuttingMaterials.Data.Model;
using CuttingMaterials.View.ViewModel;
using CuttingMaterials.View.Windows.Editors;
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
    /// Interaction logic for CoatingListWindow.xaml
    /// </summary>
    public partial class CoatingListWindow : Window
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        public CoatingListWindow()
        {
            InitializeComponent();
        }

        public string DialogResultAction { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyDown += CoatingListWindow_KeyDown;
                DataContext = new CoatingListViewModel(null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось загрузить список типов покрытий");
            }
        }

        private void CoatingListWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void lb_projects_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lb_coatings.SelectedItem == null)
                return;

            btn_edit_Click(sender, null);
        }

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {
            var coating = lb_coatings.SelectedItem as Coating;
            if (coating == null)
                return;        

            var w = new CoatingEditWindow(EditorAction.Edit);
            w.EditingCoating = coating;
            w.ShowDialog();

            if (!string.IsNullOrEmpty(w.DialogResultAction))
                Window_Loaded(null, null);

            DialogResultAction = "edit";
        }

        private void btn_deleteClick(object sender, RoutedEventArgs e)
        {
            var coating = lb_coatings.SelectedItem as Coating;
            if (coating == null)
                return;

            if (!MessagesProvider.Confirm($"Действительно удалить запись \"{coating.Name}\"?"))
                return;

            try
            {
                (DataContext as CoatingListViewModel).Delete(coating);
                
                DialogResultAction = "delete";                
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось удалить запись");
            }            
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            var w = new CoatingEditWindow(EditorAction.Create);
            w.ShowDialog();

            if (!string.IsNullOrEmpty(w.DialogResultAction))
                Window_Loaded(null, null);

            DialogResultAction = "edit";
        }

        private void lb_coatings_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                btn_deleteClick(null, null);
        }
    }
}
