using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CuttingMaterials
{
    class ApplicationWindow : Window
    {
        public ApplicationWindow()
        {
            KeyDown += ApplicationWindow_KeyDown;
        }

        private void ApplicationWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }
    }
}
