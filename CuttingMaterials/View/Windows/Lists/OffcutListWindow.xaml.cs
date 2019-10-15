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
    /// Interaction logic for OffcutListWindow.xaml
    /// </summary>
    public partial class OffcutListWindow : Window
    {
        Logger logger;
        public OffcutListWindow()
        {
            logger = LogManager.GetCurrentClassLogger();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyDown += OffcutListWindow_KeyDown;
                DataContext = new OffcutViewModel();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void OffcutListWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (DataContext as OffcutViewModel)?.Offcuts.Refresh();
        }
    }
}
