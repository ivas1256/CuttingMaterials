using CuttingMaterials.Data.Model;
using CuttingMaterials.Data.Repository;
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

namespace CuttingMaterials.View.Windows.Editors
{
    /// <summary>
    /// Interaction logic for CoatingEditWindow.xaml
    /// </summary>
    public enum EditorAction { Edit, Create }

    public partial class CoatingEditWindow : Window
    {

        Logger logger = LogManager.GetCurrentClassLogger();
        EditorAction editorAction;
        public Coating EditingCoating { get; set; }
        public string DialogResultAction { get; set; }
        public CoatingEditWindow(EditorAction editorAction)
        {
            this.editorAction = editorAction;
            InitializeComponent();
        }

        DatabaseUnitOfWork db;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                KeyDown += CoatingEditWindow_KeyDown;
                db = new DatabaseUnitOfWork();
                switch (editorAction)
                {
                    case EditorAction.Create:
                        DataContext = new Coating();
                        break;
                    case EditorAction.Edit:
                        DataContext = db.Coatings.Get(EditingCoating.Id);
                        Title = $"Редактировать тип покрытия: {EditingCoating.Name}";
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                MessagesProvider.UnknownError(ex);
            }
        }

        private void CoatingEditWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Close();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var coating = DataContext as Coating;
                if (coating == null)
                    return;

                if (!coating.Validate(this))
                    return;

                if (editorAction == EditorAction.Create)
                    db.Coatings.Add(coating);

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

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
