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
using System.Windows.Shapes;

namespace CuttingMaterials.View.Windows.Lists
{
    /// <summary>
    /// Interaction logic for DefaultDetailsListWindow.xaml
    /// </summary>
    public partial class DefaultDetailsListWindow : Window
    {
        Logger logger;
        public DefaultDetailsListWindow()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyDown += DefaultDetailsListWindow_KeyDown;
                DataContext = new DefaultDetailsViewModel(new DatabaseUnitOfWork());
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void DefaultDetailsListWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void btn_addDefaultDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((DefaultDetailsViewModel)DataContext)?.AddNew();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void btn_deleteDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var defDetail = lb_details.SelectedItem as DefaultDetail;
                if (defDetail == null)
                    return;

                ((DefaultDetailsViewModel)DataContext)?.Remove(defDetail);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                ((DefaultDetailsViewModel)DataContext)?.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось сохранить изменения");
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (DataContext as DefaultDetailsViewModel)?.DefaultDetails.Refresh();
        }
    }
}
