using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.View.ViewModel;
using Microsoft.Win32;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CuttingMaterials.View.Windows.Editors
{
    /// <summary>
    /// Interaction logic for TemplateEditWindow.xaml
    /// </summary>
    public partial class TemplateEditWindow : Window
    {
        Logger logger;
        public string DialogResultAction { get; set; }

        EditorAction editorAction;
        public Template UpdatedTemplate { get; set; }
        public TemplateEditWindow(EditorAction editorAction)
        {
            this.editorAction = editorAction;
            logger = LogManager.GetCurrentClassLogger();

            InitializeComponent();
        }

        string oldImageFile;
        DatabaseUnitOfWork db;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                db = new DatabaseUnitOfWork();
                Template template = null;
                switch (editorAction)
                {
                    case EditorAction.Edit:
                        template = db.Templates.GetWithSizes(UpdatedTemplate.Id);
                        Title = $"Редактировать шаблон: {UpdatedTemplate.Name}";
                        break;
                    case EditorAction.Create:
                        template = new Template();
                        break;
                }

                templateSizesList.SetDataContext(template, db);
                DataContext = template;
                if (!string.IsNullOrEmpty(template.ImageFileName))
                    designArea.SetImage(template.FullFileName);
                else if (!string.IsNullOrEmpty(template.Xaml))
                    designArea.SetXaml(template.Xaml, template.Sizes.Select(x => new DetailSize()
                    {
                        Value = x.Value,
                        Id = x.Id,
                        RuntimeId = x.RuntimeId
                    }).ToList());

                oldImageFile = template.ImageFileName;
                designArea.IsEnabled = template.IsDrawingEnabled;

                designArea.OnCreateNewSizeEvent += DesignArea_OnCreateNewSizeEvent;
                designArea.OnSizeChangedEvent += DesignArea_OnSizeChanged;
                designArea.OnSizeDeletedEvent += DesignArea_OnSizeDeletedEvent;

                templateSizesList.OnSizeChanged += TemplateSizesList_OnSizeChanged;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        #region sizes crud
        private void TemplateSizesList_OnSizeChanged(TemplateSize newSize)
        {
            designArea.ChangeSizeValue(new DetailSize()
            {
                Value = newSize.Value,
                Id = newSize.Id,
                RuntimeId = newSize.RuntimeId
            });
        }

        private void DesignArea_OnSizeChanged(DetailSize detailSize)
        {
            templateSizesList.ChangeSizeValue(detailSize);
        }

        private DetailSize DesignArea_OnCreateNewSizeEvent()
        {
            var ts = templateSizesList.AddNewSize();
            return new DetailSize()
            {
                Value = ts.Value,
                Id = ts.Id,
                RuntimeId = ts.RuntimeId
            };
        }

        private void DesignArea_OnSizeDeletedEvent(DetailSize detailSize)
        {
            templateSizesList.DeleteSize(detailSize);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            designArea.IsEnabled = (DataContext as Template).IsDrawingEnabled;
            if (designArea.IsEnabled)
                designArea.Reset();
        }
        #endregion

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(App.CacheDirectory))
                Directory.CreateDirectory(App.CacheDirectory);

            try
            {
                var template = DataContext as Template;
                if (template == null)
                    return;

                if (!template.Validate(this))
                    return;

                if (string.IsNullOrEmpty(template.ImageFileName))
                    template.ImageFileName = null;
                if (editorAction == EditorAction.Create)
                    db.Templates.Add(template);

                if (!string.IsNullOrEmpty(template.ImageFileName))
                {
                    string newImageFile = System.IO.Path.Combine(App.CacheDirectory, System.IO.Path.GetFileName(template.ImageFileName));
                    string cuttedImageFileName = System.IO.Path.Combine(
                                System.IO.Path.GetFileName(App.CacheDirectory),
                                System.IO.Path.GetFileName(newImageFile));

                    if (!File.Exists(newImageFile))
                        File.Copy(template.ImageFileName, newImageFile);

                    if (!oldImageFile.Equals(newImageFile) && File.Exists(oldImageFile))
                    {
                        var check = db.Templates.GetAll().Select(x => x.ImageFileName).ToList();
                        if(!check.Contains(oldImageFile))
                            File.Delete(System.IO.Path.Combine(App.CurrDirectory, oldImageFile));  
                    }
                }
                else
                {
                    template.Xaml = designArea.GetXaml();
                    template.XamlPreviewImageFile = System.IO.Path.Combine(
                            System.IO.Path.GetFileName(App.CacheDirectory),
                            template.GetPreviewFileName());

                    designArea.SaveToImage(System.IO.Path.Combine(App.CurrDirectory, template.XamlPreviewImageFile));
                }

                db.Complete();
                DialogResultAction = "edit";
                Close();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось сохранить запись");
            }
        }

        private void btn_browse_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (!ofd.ShowDialog().Value)
                return;

            (DataContext as Template).ImageFileName = ofd.FileName;
            designArea.SetImage(ofd.FileName);

            tb_file.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            designArea.IsEnabled = (DataContext as Template).IsDrawingEnabled;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TemplateEditWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }
    }
}
