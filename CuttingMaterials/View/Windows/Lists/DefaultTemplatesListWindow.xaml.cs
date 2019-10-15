using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.View.ViewModel;
using CuttingMaterials.View.Windows.Editors;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CuttingMaterials.View.Windows.Lists
{
    /// <summary>
    /// Interaction logic for DefaultTemplatesListWindow.xaml
    /// </summary>
    public partial class DefaultTemplatesListWindow : Window
    {
        Logger logger;
        public Template SelectedTemplate { get; set; }
        ProjectViewModel projectVm;
        bool isAllowSelection;
        public DefaultTemplatesListWindow(ProjectViewModel projectVm, bool isAllowSelection = true)
        {
            logger = LogManager.GetCurrentClassLogger();
            this.isAllowSelection = isAllowSelection;
            this.projectVm = projectVm;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DataContext = new TemplatesListViewModel(false);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось загузить список шаблонов");
            }
        }               

        private void btn_select_Click(object sender, RoutedEventArgs e)
        {
            var template = lb_defaultTempaltes.SelectedItem as Template;
            if (template == null)
                return;

            if (isAllowSelection)
            {
                SelectedTemplate = template;
                Close();
            }
            else
                btn_edit_Click(null, null);
        }
        private void lb_defaultTempaltes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btn_select_Click(null, null);
        }           

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            var w = new TemplateEditWindow(EditorAction.Create);
            w.Focus();
            w.ShowDialog();

            if (!string.IsNullOrEmpty(w.DialogResultAction))
                Window_Loaded(null, null);
        }

        private void btn_edit_Click(object sender, RoutedEventArgs e)
        {
            var template = lb_defaultTempaltes.SelectedItem as Template;
            if (template == null)
                return;

            var w = new TemplateEditWindow(EditorAction.Edit);
            w.UpdatedTemplate = template;

            w.Focus();
            w.ShowDialog();

            if (!string.IsNullOrEmpty(w.DialogResultAction))
            {
                Window_Loaded(null, null);
            }
        }

        private void btn_deleteClick(object sender, RoutedEventArgs e)
        {
            var template = lb_defaultTempaltes.SelectedItem as Template;
            if (template == null)
                return;

            if (!MessagesProvider.Confirm($"Действительно удалить запись \"{template.Name}\"?"))
                return;

            try
            {
                using (var db = new DatabaseUnitOfWork())
                {
                    db.Templates.RemoveRange(db.Templates.Find(x => x.Id == template.Id));
                    db.Complete();
                }

                Window_Loaded(null, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось удалить запись");
            }

        }        

        private void ListBoxItem_Loaded(object sender, RoutedEventArgs e)
        {
            var imgControl = App.FindVisualChildren<Image>((ListBoxItem)sender).FirstOrDefault();
            if (imgControl == null)
                return;

            var template = imgControl.DataContext as Template;
            if (template == null)
                return;

            string imgFileName = "";
            if (!string.IsNullOrEmpty(template.ImageFileName))
                imgFileName = System.IO.Path.Combine(App.CurrDirectory, template.CachedPreviewImageFileName);               
            else if (!string.IsNullOrEmpty(template.Xaml))
                imgFileName = System.IO.Path.Combine(App.CurrDirectory, template.XamlPreviewImageFile);

            if (string.IsNullOrEmpty(imgFileName) || !File.Exists(imgFileName))
                return;

            var img = new BitmapImage()
            {
                CacheOption = BitmapCacheOption.OnLoad,
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            using (var stream = new FileStream(imgFileName, FileMode.Open, FileAccess.Read))
            {
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = stream;
                img.EndInit();
            }
            imgControl.Source = img;
        }

        private void lb_defaultTempaltes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                btn_deleteClick(null, null);
        }

        private void tb_searchQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            (DataContext as TemplatesListViewModel)?.Templates.Refresh();
        }

        private void DefaultTemplatesListWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
