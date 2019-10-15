using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.Logic;
using CuttingMaterials.View.ViewModel;
using CuttingMaterials.View.Windows;
using CuttingMaterials.View.Windows.Editors;
using CuttingMaterials.View.Windows.Lists;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CuttingMaterials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Logger logger;
        public MainWindow()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var nw = new NodesWorker();
                nw.TestIt();
            }
            catch (Exception ex)
            {
                MessagesProvider.Error(ex, "Не удалось загрузить рабочую область");
                Close();
                Application.Current.Shutdown();
            }

            OpenProject(null);

            designArea.OnCreateNewSizeEvent += DesignArea_OnNewSizeEvent;
            designArea.OnSizeChangedEvent += DesignArea_OnSizeChanged;
            designArea.OnSizeDeletedEvent += DesignArea_OnSizeDeletedEvent;

            detailSizesList.OnSizeChangedEvent += DetailSizesList_OnSizeChangedEvent;
        }

        #region CRUD project
        private void btn_newProject_Click(object sender, RoutedEventArgs e)
        {
            var w = new CreateProjectWindow();
            w.Focus();
            w.ShowDialog();

            if (w.SelectedProject == null)
                return;

            OpenProject(w.SelectedProject);
        }

        private void btn_saveProject_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = DataContext as ProjectViewModel;
            if (prjVm == null)
                return;

            prjVm.Status = "Сохранение...";
            prjVm.StatusIconCode = null;

            if (!prjVm.Project.Validate(this))
            {
                prjVm.Status = "Сохранение отклонено: не все поля заполнены корректно";
                prjVm.HasInfError = true;
                prjVm.StatusIconCode = "error";
                return;
            }

            try
            {
                if (selectedDetail != null && selectedDetail.TemplateId == null)
                {
                    prjVm.Details.SetXaml(selectedDetail.Id, designArea.GetXaml());
                    designArea.SaveToImage(System.IO.Path.Combine(App.CacheDirectory, selectedDetail.GetPreviewImageFileName()));
                    selectedDetail.PreviewFile = System.IO.Path.Combine(System.IO.Path.GetFileName(App.CacheDirectory),
                        selectedDetail.GetPreviewImageFileName());
                }

                if (selectedDetail != null && tabSelectedIndex == 0)
                {
                    if (selectedDetail.TemplateId == null)
                        selectedDetail.Xaml = designArea.GetXaml();
                }

                prjVm.SaveChanges();

                prjVm.Status = "Проект сохранен";
                prjVm.HasInfError = false;
                prjVm.StatusIconCode = "ok";

                lb_details_SelectionChanged(lb_details, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Ошибка сохранения");
            }
        }

        private void btn_openProject_Click(object sender, RoutedEventArgs e)
        {
            var w = new ProjectListWindow();
            w.Focus();
            w.ShowDialog();

            if (w.SelectedProject == null)
                return;

            if (string.IsNullOrEmpty(w.DialogResultAction))
                return;

            if (w.DialogResultAction.Equals("delete"))
            {
                try
                {
                    (DataContext as ProjectViewModel).Delete(w.SelectedProject);
                    if ((DataContext as ProjectViewModel).Project.Name.Equals(w.SelectedProject.Name))
                        DataContext = new ProjectViewModel();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    MessagesProvider.Error(ex, "Не удалось удалить проект");
                }
            }
            else
                OpenProject(w.SelectedProject);
        }

        void OpenProject(Project project, bool isLoadFromDB = true)
        {
            try
            {
                designArea.Reset();

                detailSizesList.Reset();

                DataContext = new ProjectViewModel(project, isLoadFromDB);
                project = ((ProjectViewModel)DataContext).Project;

                cb_coatings.DataContext = new CoatingListViewModel(project);
                if (project.Details.Count != 0)
                    lb_details.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось подключиться к базе данных");
            }
        }
        #endregion

        #region CRUD coating
        private void btn_CoatingList_Click(object sender, RoutedEventArgs e)
        {
            var w = new CoatingListWindow();
            w.ShowDialog();

            if (string.IsNullOrEmpty(w.DialogResultAction))
                return;

            try
            {
                cb_coatings.DataContext = new CoatingListViewModel(((ProjectViewModel)DataContext).Project);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void btn_addCoating_Click(object sender, RoutedEventArgs e)
        {
            var w = new CoatingEditWindow(EditorAction.Create);
            w.ShowDialog();

            if (string.IsNullOrEmpty(w.DialogResultAction))
                return;

            try
            {
                cb_coatings.DataContext = new CoatingListViewModel(((ProjectViewModel)DataContext).Project);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }
        #endregion

        #region CRUD detail
        private void btn_addDetail_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = (ProjectViewModel)DataContext;
            if (prjVm == null)
                return;

            var w = new DetailEditorWindow(EditorAction.Create, prjVm);
            w.ShowDialog();
        }

        private void btn_editDetail_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = (ProjectViewModel)DataContext;
            if (prjVm == null)
                return;

            var detail = lb_details.SelectedItem as Detail;
            if (detail == null)
                return;

            var w = new DetailEditorWindow(EditorAction.Edit, prjVm);
            w.UpdatedDetail = detail;
            w.ShowDialog();

            lb_details.GetBindingExpression(ListBox.ItemsSourceProperty).UpdateTarget();

            lb_details_SelectionChanged(lb_details, null);
        }

        private void btn_deleteDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var detail = lb_details.SelectedItem as Detail;
                if (detail == null)
                    return;
                var prjVm = DataContext as ProjectViewModel;
                if (prjVm == null)
                    return;

                if (!MessagesProvider.Confirm($"Действительно удалить деталь {detail.Name}?"))
                    return;

                prjVm.Details.Remove(detail);
                detailSizesList.Reset();
                designArea.Reset();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось удалить запись");
            }
        }
        private void lb_details_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                btn_deleteDetail_Click(null, null);
        }

        Detail selectedDetail;
        private void lb_details_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {//тут происходит загрузка изображения в designArea
            var prjVm = (ProjectViewModel)DataContext;
            if (prjVm == null)
                return;
            var detail = lb_details.SelectedItem as Detail;
            if (detail == null)
                return;

            try
            {
                if (selectedDetail != null)//для предыдущей детали
                {//сохраняем xaml и обновляем изображение preview
                    var newXaml = designArea.GetXaml();
                    if (!newXaml.Equals(selectedDetail.Xaml))
                    {
                        if (selectedDetail.TemplateId == null)
                            prjVm.Details.SetXaml(selectedDetail.Id, newXaml);

                        if (!string.IsNullOrEmpty(selectedDetail.PreviewFile))
                            designArea.SaveToImage(System.IO.Path.Combine(App.CurrDirectory, selectedDetail.PreviewFile));
                        else if (selectedDetail.TemplateId == null)
                        {
                            designArea.SaveToImage(System.IO.Path.Combine(App.CacheDirectory, selectedDetail.GetPreviewImageFileName()));
                            selectedDetail.PreviewFile = System.IO.Path.Combine(System.IO.Path.GetFileName(App.CacheDirectory),
                                selectedDetail.GetPreviewImageFileName());
                            prjVm.SaveChanges();
                        }
                    }
                }

                designArea.IsEnabled = detail.IsDrawingEnabled;
                detailSizesList.IsEnabled = true;
                detailSizesList.SetDataContext(detail, prjVm.DataContext);
                selectedDetail = detail;

                if (detail.TemplateId == null || detail.TemplateId < 0)
                    designArea.SetXaml(selectedDetail.Xaml, selectedDetail.Sizes.ToList());
                else
                {
                    var template = prjVm.GetTemplate(detail.TemplateId.Value);
                    if (!string.IsNullOrEmpty(template.ImageFileName))
                        designArea.SetImage(template.CachedPreviewImageFileName);
                    else if (!string.IsNullOrEmpty(detail.Xaml))
                        designArea.SetXaml(detail.Xaml, selectedDetail.Sizes.ToList());
                }

                FindImage((DependencyObject)sender, detail)?
                    .RaiseEvent(new RoutedEventArgs(Image.LoadedEvent, FindImage((DependencyObject)sender, detail)));
                ((ListBox)sender).ApplyTemplate();
            }
            catch (Exception ex)
            {
                MessagesProvider.UnknownError(ex);
                logger.Error(ex);
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var imgControl = sender as Image;
            if (imgControl == null)
                return;
            var detail = imgControl.DataContext as Detail;
            if (detail == null)
                return;

            string imgFileName = "";
            if (!string.IsNullOrEmpty(detail.PreviewFile))
                imgFileName = System.IO.Path.Combine(App.CurrDirectory, detail.PreviewFile);
            else
            {
                if (detail.TemplateId != null)
                {
                    if (!string.IsNullOrEmpty(detail.Template.ImageFileName))
                        imgFileName = System.IO.Path.Combine(App.CacheDirectory,
                                        System.IO.Path.GetFileName(detail.Template.ImageFileName));

                    if (!string.IsNullOrEmpty(detail.Template.XamlPreviewImageFile))
                        imgFileName = System.IO.Path.Combine(App.CurrDirectory, detail.Template.XamlPreviewImageFile);
                }
            }

            if (!File.Exists(imgFileName))
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

        Image FindImage(DependencyObject depObj, Detail dataContext)
        {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as Image) ?? FindImage(child, dataContext);
                if (result != null && ((Detail)result.DataContext).Name.Equals(dataContext.Name))
                    return result;
            }
            return null;
        }
        #endregion

        #region detail sizes editing
        private DetailSize DesignArea_OnNewSizeEvent()
        {
            return detailSizesList.AddNewSize();
        }

        private void DesignArea_OnSizeChanged(DetailSize detailSize)
        {
            detailSizesList.ChangeSizeValue(detailSize);
        }

        private void DetailSizesList_OnSizeChangedEvent(DetailSize newSize)
        {
            designArea.ChangeSizeValue(newSize);
        }

        private void DesignArea_OnSizeDeletedEvent(DetailSize detailSize)
        {
            detailSizesList.DeleteSize(detailSize);
        }
        #endregion

        #region cutting
        private void btn_CalcCutting_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = DataContext as ProjectViewModel;
            if (prjVm == null)
                return;

            try
            {
                btn_calcCutting.IsEnabled = false;
                prjVm.SaveChanges();

                Task.Run(() =>
                {
                    try
                    {
                        prjVm.CalcCutting();

                        Dispatcher.Invoke(() =>
                        {
                            prjVm.RenderCutting(cutterCanvas, (int)cutterCanvas.ActualWidth, false);

                            l_listAmount.Content = prjVm.CuttingResult.Count;
                            tb_offcutInfo.Text = prjVm.CalcOffcut();
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        MessagesProvider.UnknownError(ex);
                    }
                    finally
                    {
                        Dispatcher.Invoke(() => btn_calcCutting.IsEnabled = true);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void btn_clearCutting_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = DataContext as ProjectViewModel;
            if (prjVm == null)
                return;

            prjVm.ClearCutting();
            cutterCanvas.Children.Clear();
        }
        #endregion

        private void btn_print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var prjVm = DataContext as ProjectViewModel;
                if (prjVm == null)
                    return;

                if (!prjVm.CustomValidate())
                {
                    MessagesProvider.IncorrectData("Не вся информация о проекте заполнена");
                    return;
                }

                btn_saveProject_Click(null, null);

                var canvas = new Canvas();
                canvas.Resources.Add("HatchBrush", cutterCanvas.Resources["HatchBrush"]);

                prjVm.CalcCutting();
                IEnumerable<PaperSize> paperSizes = new PrinterSettings().PaperSizes.Cast<PaperSize>();
                PaperSize sizeA4 = paperSizes.First(size => size.Kind == PaperKind.A4);

                prjVm.RenderCutting(canvas, sizeA4.Width - 50, true);

                var w = new PrintWindow(prjVm);
                w.ShowDialog();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void btn_savePdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var prjVm = DataContext as ProjectViewModel;
                if (prjVm == null)
                    return;

                if (!prjVm.CustomValidate())
                {
                    MessagesProvider.IncorrectData("Не вся информация о проекте заполнена");
                    return;
                }

                btn_saveProject_Click(null, null);

                var sfd = new SaveFileDialog();
                sfd.FileName = $"{prjVm.Project.Name}.pdf";
                sfd.Filter = "Pdf файл|*.pdf";
                if (!sfd.ShowDialog().Value)
                    return;

                IEnumerable<PaperSize> paperSizes = new PrinterSettings().PaperSizes.Cast<PaperSize>();
                PaperSize sizeA4 = paperSizes.First(size => size.Kind == PaperKind.A4);

                var canvas = new Canvas();
                canvas.Resources.Add("HatchBrush", cutterCanvas.Resources["HatchBrush"]);
                prjVm.CalcCutting();
                prjVm.RenderCutting(canvas, sizeA4.Width - 70, true);

                var printer = new Printer(prjVm);
                printer.CreatePdf(sfd.FileName);

                MessagesProvider.Successfully("Сохранено");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        #region window manage    
        int tabSelectedIndex;
        private void tab_main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab_main.SelectedIndex != 2 && tabSelectedIndex == 2)
            {
                btn_saveProject_Click(null, null);
            }

            if (tab_main.SelectedIndex == 2)
            {
                var prjVm = DataContext as ProjectViewModel;
                if (prjVm != null)
                    prjVm.HasInfError = false;
            }

            tabSelectedIndex = tab_main.SelectedIndex;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                btn_saveProject_Click(null, null);
            }

            if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
            {
                btn_print_Click(null, null);
            }
        }

        private void btn_defaultDetails_Click(object sender, RoutedEventArgs e)
        {
            var w = new DefaultDetailsListWindow();
            w.ShowDialog();
        }

        private void btn_offcuts_Click(object sender, RoutedEventArgs e)
        {
            var w = new OffcutListWindow();
            w.ShowDialog();
        }

        private void btn_templates_Click(object sender, RoutedEventArgs e)
        {
            var prjVm = (ProjectViewModel)DataContext;
            if (prjVm == null)
                return;

            var w = new DefaultTemplatesListWindow(prjVm, false);
            w.ShowDialog();
        }
        #endregion
    }
}
