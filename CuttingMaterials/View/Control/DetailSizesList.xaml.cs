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
using CuttingMaterials.View.ViewModel;
using CuttingMaterials.Data.Model;
using NLog;
using CuttingMaterials.Data.Repository;
using CuttingMaterials.View.Convertor;

namespace CuttingMaterials.View.Control
{
    /// <summary>
    /// Interaction logic for DetailSizesList.xaml
    /// </summary>
    public partial class DetailSizesList : UserControl
    {
        Logger logger;
        public DetailSizesList()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        public void Reset()
        {
            IsEnabled = false;
            lb_sizes.DataContext = null;
        }

        public void SetDataContext(Detail detail, DatabaseUnitOfWork db)
        {
            lb_sizes.DataContext = new DetailViewModel(detail.Project, detail, db);
        }

        public void ChangeSizeValue(DetailSize detailSize)
        {
            (lb_sizes.DataContext as DetailViewModel)?.ChangeSizeValue(detailSize.Id, detailSize.RuntimeId, detailSize.Value);
        }

        public void DeleteSize(DetailSize deletingSize)
        {
            try
            {
                var sizes = (lb_sizes.DataContext as DetailViewModel)?.Detail.Sizes;

                var detailVm = lb_sizes.DataContext as DetailViewModel;
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

                detailVm.Remove(size);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        public DetailSize AddNewSize()
        {
            btn_addDetailSize_Click(null, null);

            var vm = (lb_sizes.DataContext as DetailViewModel);
            var currDetail = vm.Detail;

            return currDetail.Sizes.Last();
        }

        private void btn_addDetailSize_Click(object sender, RoutedEventArgs e)
        {
            var detailVm = lb_sizes.DataContext as DetailViewModel;
            if (detailVm == null)
                return;

            detailVm.AddNewSize();
        }

        private void btn_deleteDetailSize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var detailVm = lb_sizes.DataContext as DetailViewModel;
                if (detailVm == null)
                    return;
                var size = lb_sizes.SelectedItem as DetailSize;
                if (size == null)
                    return;

                if (!MessagesProvider.Confirm($"Действительно удалить размер {size.Name}?"))
                    return;

                detailVm.Remove(size);
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
                btn_deleteDetailSize_Click(null, null);
        }

        public delegate void OnSizeChangedEventHandler(DetailSize newSize);
        public event OnSizeChangedEventHandler OnSizeChangedEvent;

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var size = tb.DataContext as DetailSize;

            OnSizeChangedEvent(size);
        }
    }
}
