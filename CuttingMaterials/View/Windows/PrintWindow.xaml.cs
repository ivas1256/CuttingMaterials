using CuttingMaterials.Logic;
using CuttingMaterials.View.ViewModel;
using DotLiquid;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
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
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace CuttingMaterials.View.Windows
{
    /// <summary>
    /// Interaction logic for PrintWindow.xaml
    /// </summary>
    public partial class PrintWindow : Window
    {
        Logger logger;
        ProjectViewModel projectVm;
        public PrintWindow(ProjectViewModel projectVm)
        {
            logger = LogManager.GetCurrentClassLogger();
            this.projectVm = projectVm;
            InitializeComponent();
        }

        Printer printer;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            KeyDown += PrintWindow_KeyDown;

            try
            {
                printer = new Printer(projectVm);
                docView.Document = printer.CreateDocument();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.Error(ex, "Не удалось создать документ");
            }
        }

        private void PrintWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }
    }
}
