using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CuttingMaterials.View.Resources
{
    public partial class Validation
    {
        private void tb_name_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void tb_validationError(object sender, ValidationErrorEventArgs e)
        {
            MessagesProvider.Successfully("error");
        }
    }
}
