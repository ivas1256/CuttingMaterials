using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.View.ViewModel;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace CuttingMaterials.View.Windows.Editors
{
    /// <summary>
    /// Interaction logic for DetailEditorWindow.xaml
    /// </summary>
    public partial class DetailEditorWindow : Window
    {
        Logger logger;
        EditorAction editorAction;
        public Detail UpdatedDetail { get; set; }
        ProjectViewModel projectViewModel;
        TemplatesListViewModel templatesViewModel;
        int savedTemplateId;//templateID какой он был при открытии окна

        public DetailEditorWindow(EditorAction editorAction, ProjectViewModel projectViewModel)
        {
            logger = LogManager.GetCurrentClassLogger();
            this.projectViewModel = projectViewModel;
            this.editorAction = editorAction;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                templatesViewModel = new TemplatesListViewModel(true);
                cb_Templates.ItemsSource = templatesViewModel.Templates;

                if (editorAction == EditorAction.Create)
                    DataContext = new Detail() { Project = projectViewModel.Project, TemplateId = -1 };
                else
                {
                    var d = projectViewModel.DataContext.Details.Get(UpdatedDetail.Id);
                    if (d.TemplateId == null)
                        d.TemplateId = -1;
                    DataContext = d;
                    Title = $"Редактировать деталь: {UpdatedDetail.Name}";
                }

                savedTemplateId = ((Detail)DataContext).TemplateId.Value;               
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }        

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var detail = DataContext as Detail;
                if (detail == null)
                    return;

                if (!detail.Validate(this))
                    return;

                if (detail.TemplateId < 0)
                    detail.TemplateId = null;
                else if (detail.Template == null)
                    detail.Template = projectViewModel.GetTemplate(detail.TemplateId.Value);

                if (editorAction == EditorAction.Create)
                    projectViewModel.Details.Add(detail);

                if(detail.Id == 0 && detail.TemplateId != null)
                {
                    var templateDefaultSizes = projectViewModel.DataContext.TemplateSizes
                        .Find(x => x.TemplateId == detail.TemplateId);
                    foreach (var templateSize in templateDefaultSizes)
                        detail.Sizes.Add(new DetailSize()
                        {
                            Name = templateSize.Name,
                            Value = templateSize.Value
                        });
                }

                if(detail.TemplateId != null)
                {
                    if (!string.IsNullOrEmpty(detail.Template.Xaml))
                    {
                        detail.Xaml = detail.Template.Xaml;

                        if (!string.IsNullOrEmpty(detail.Template.XamlPreviewImageFile))
                        {
                            var detailPreviewFileName = System.IO.Path.Combine(App.CacheDirectory, detail.GetPreviewImageFileName());
                            File.Copy(System.IO.Path.Combine(App.CurrDirectory, detail.Template.XamlPreviewImageFile),
                                        detailPreviewFileName);

                            detail.PreviewFile = System.IO.Path.Combine(System.IO.Path.GetFileName(App.CacheDirectory),
                                                    detail.GetPreviewImageFileName());
                        }
                    }
                }
                else
                {
                    detail.Xaml = null;
                    detail.PreviewFile = null;
                }

                projectViewModel.SaveChanges();
                Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось сохранить запись");
            }
        }       

        private void cb_Templates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var detail = DataContext as Detail;
            if (detail == null)
                return;

            if (string.IsNullOrEmpty(detail.Name))
            {
                detail.Name = ((Template)cb_Templates.SelectedItem)?.Name.ToString();
                detail.PropChanged("Name");
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DetailEditorWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
