using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CuttingMaterials.View.Control
{
    /// <summary>
    /// Interaction logic for TemplateSizesList.xaml
    /// </summary>
    public partial class TemplateSizesList : UserControl
    {
        Logger logger;
        public TemplateSizesList()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        DatabaseUnitOfWork db;
        public void SetDataContext(Template template, DatabaseUnitOfWork db)
        {
            this.db = db;
            lb_sizes.DataContext = new TemplateSizesViewModel(template);
        }

        public TemplateSize AddNewSize()
        {
            btn_addTemplateSize_Click(null, null);
            var sizesVm = lb_sizes.DataContext as TemplateSizesViewModel;
            if (sizesVm == null)
                return null;

            return sizesVm.Sizes.Last();
        }

        public void ChangeSizeValue(DetailSize detailSize)
        {
            (lb_sizes.DataContext as TemplateSizesViewModel)?.ChangeSizeValue(detailSize.Id, detailSize.RuntimeId, detailSize.Value);
        }

        public void DeleteSize(DetailSize deletingSize)
        {
            try
            {
                var sizes = (lb_sizes.DataContext as TemplateSizesViewModel)?.Sizes;

                var detailVm = lb_sizes.DataContext as TemplateSizesViewModel;
                if (detailVm == null)
                    return;
                var size = sizes.Where(x =>
                {
                    if (x.Id == 0)
                        return x.RuntimeId.Equals(deletingSize.RuntimeId);
                    else
                        return x.Id == deletingSize.Id;
                }).ToList().FirstOrDefault();
                if (size == null)
                    return;

                detailVm.Remove(db, size);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void btn_addTemplateSize_Click(object sender, RoutedEventArgs e)
        {
            var sizesVm = lb_sizes.DataContext as TemplateSizesViewModel;
            if (sizesVm == null)
                return;

            sizesVm.AddNew();
        }

        private void btn_deleteTemplateSize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var templateSizesVm = lb_sizes.DataContext as TemplateSizesViewModel;
                if (templateSizesVm == null)
                    return;

                var size = lb_sizes.SelectedItem as TemplateSize;
                if (size == null)
                    return;

                if (!MessagesProvider.Confirm($"Действительно удалить размер {size.Name}?"))
                    return;

                templateSizesVm.Remove(db, size);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void lb_sizes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                btn_deleteTemplateSize_Click(null, null);
        }

        public delegate void OnSizeChangedEventHandler(TemplateSize newSize);
        public event OnSizeChangedEventHandler OnSizeChanged;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var size = tb.DataContext as TemplateSize;

            OnSizeChanged(size);
        }
    }
}
