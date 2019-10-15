using CuttingMaterials.Data.Model;
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

namespace CuttingMaterials.View.Windows
{
    /// <summary>
    /// Interaction logic for CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        public CreateProjectWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tb_name.Focus();
            DataContext = Project.CreateDefaultObj();

            KeyDown += CreateProjectWindow_KeyDown;
        }

        private void CreateProjectWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        public Project SelectedProject { get; set; }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            var prj = DataContext as Project;
            if (prj == null)
                return;

            if (string.IsNullOrWhiteSpace(prj.Name))
                if (!prj.Validate(this))
                    return;

            SelectedProject = prj;
            Close();
        }
    }
}
