using CuttingMaterials.Data.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CuttingMaterials.View.Control
{
    /// <summary>
    /// Interaction logic for DesignArea.xaml
    /// </summary>
    public partial class DesignArea : UserControl
    {
        public int LineWidth { get; set; } = 2;
        int textFontSize = 40;
        public int TextFontSize
        {
            get
            {
                return textFontSize;
            }
            set
            {
                textFontSize = value;
                if (focusedTextBox != null)
                    focusedTextBox.FontSize = textFontSize;
            }
        }
        public DesignArea()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainGrid.DataContext = this;
        }

        #region outside interface
        public void SetXaml(string xaml, List<DetailSize> sizes)
        {
            Reset();

            if (string.IsNullOrEmpty(xaml))
                return;

            var savedCanvas = XamlReader.Parse(xaml) as Canvas;
            var savedElements = new List<UIElement>();
            foreach (UIElement obj in savedCanvas.Children)
                savedElements.Add(obj);

            foreach (var el in savedElements)
            {
                savedCanvas.Children.Remove(el);
                canvas.Children.Add(el);

                if (el is Line)
                    ((Line)el).PreviewMouseLeftButtonDown += Line_PreviewMouseLeftButtonDown;

                if (el is System.Windows.Shapes.Path)
                    ((System.Windows.Shapes.Path)el).PreviewMouseLeftButtonDown += Path_PreviewMouseLeftButtonDown;

                if (el is TextBox)
                {
                    ((TextBox)el).PreviewMouseLeftButtonDown += Tb_MouseLeftButtonDown;
                    ((TextBox)el).PreviewMouseLeftButtonDown += Tb_MouseLeftButtonDown;
                    ((TextBox)el).PreviewMouseLeftButtonUp += Tb_MouseLeftButtonUp;
                    ((TextBox)el).GotFocus += Tb_GotFocus;

                    if (((TextBox)el).Name.Contains("lineLength"))
                    {
                        var tag = ((TextBox)el).Tag.ToString();
                        var selectedSize = sizes.Where(x =>
                        {
                            if (tag.Contains("runtime"))
                                return x.RuntimeId?.Equals(tag) ?? false;
                            else
                                return x.Id.ToString().Equals(tag);
                        }).FirstOrDefault();

                        if (selectedSize != null)
                        {
                            ((TextBox)el).DataContext = new DetailSize()
                            {
                                Value = selectedSize.Value,
                                RuntimeId = selectedSize.Id == 0 ? selectedSize.RuntimeId : selectedSize.Id.ToString(),
                                Id = selectedSize.Id
                            };

                            ((TextBox)el).Text = selectedSize.Value.ToString();

                            if (selectedSize.Id != 0)
                            {
                                ((TextBox)el).Tag = selectedSize.Id.ToString();
                                selectedSize.RuntimeId = null;
                            }

                            ((TextBox)el).TextChanged += TextBox_TextChanged;
                        }
                    }
                }
            }
        }
        public string GetXaml()
        {
            return XamlWriter.Save(canvas);
        }

        public void Reset()
        {
            canvas.Children.Clear();
        }

        public void SetImage(string imageFileName)
        {
            if (string.IsNullOrWhiteSpace(imageFileName) || !File.Exists(imageFileName))
                return;

            Reset();

            int margin = 45;
            var imgSource = new BitmapImage()
            {
                CacheOption = BitmapCacheOption.OnLoad,
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            using (var stream = new FileStream(imageFileName, FileMode.Open, FileAccess.Read))
            {
                imgSource.BeginInit();
                imgSource.CacheOption = BitmapCacheOption.OnLoad;
                imgSource.StreamSource = stream;
                imgSource.EndInit();
            }

            Image img = new Image();
            img.Source = imgSource;
            img.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            if (img.DesiredSize.Width > canvas.ActualWidth ||
                img.DesiredSize.Height > canvas.ActualHeight)
            {
                img.Width = canvas.ActualWidth - margin;
                img.Height = canvas.ActualHeight - margin;

                Canvas.SetTop(img, margin);
                Canvas.SetLeft(img, margin);
            }
            else
            {
                Canvas.SetTop(img, canvas.ActualHeight / 2 - img.DesiredSize.Height / 2);
                Canvas.SetLeft(img, canvas.ActualWidth / 2 - img.DesiredSize.Width / 2);
            }

            canvas.Children.Add(img);
        }
        public void SaveToImage(string imgFileName)
        {
            if (File.Exists(imgFileName))
                File.Delete(imgFileName);

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)canvas.RenderSize.Width,
                    (int)canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);
            rtb.Render(canvas);

            double minX = 0, minY = 0, maxX = 0, maxY = 0;
            foreach (var obj in canvas.Children)
            {
                if (obj is Line)
                {
                    var line = (Line)obj;
                    if (minX == 0)
                        minX = line.X1;
                    if (minY == 0)
                        minY = line.Y1;

                    minX = Math.Min(minX, Math.Min(line.X1, line.X2));
                    minY = Math.Min(minY, Math.Min(line.Y1, line.Y2));

                    maxX = Math.Max(maxX, Math.Max(line.X1, line.X2));
                    maxY = Math.Max(maxY, Math.Max(line.Y1, line.Y2));
                }

                if (obj is TextBox)
                {
                    var tb = (TextBox)obj;
                    if (minX == 0)
                        minX = Canvas.GetLeft(tb);
                    if (minY == 0)
                        minY = Canvas.GetTop(tb);

                    minX = Math.Min(minX, Canvas.GetLeft(tb));
                    minY = Math.Min(minY, Canvas.GetTop(tb));

                    maxX = Math.Max(maxX, Canvas.GetLeft(tb));
                    maxY = Math.Max(maxY, Canvas.GetTop(tb));
                }

                if (obj is System.Windows.Shapes.Path)
                {
                    var path = (System.Windows.Shapes.Path)obj;
                    var figures = ((PathGeometry)path.Data).Figures;
                    foreach (var fig in figures)
                    {
                        if (minX == 0)
                            minX = fig.StartPoint.X;
                        if (minY == 0)
                            minY = fig.StartPoint.Y;
                        if (maxX == 0)
                            maxX = fig.StartPoint.X;
                        if (maxY == 0)
                            maxY = fig.StartPoint.Y;

                        if (fig.Segments != null)
                            foreach (var segment in fig.Segments)
                            {
                                if (segment is PolyLineSegment)
                                {
                                    var polyLine = (PolyLineSegment)segment;

                                    minX = Math.Min(minX, polyLine.Points.Select(x => x.X).Min());
                                    minY = Math.Min(minY, polyLine.Points.Select(x => x.Y).Min());

                                    maxX = Math.Max(maxX, polyLine.Points.Select(x => x.X).Max());
                                    maxY = Math.Max(maxY, polyLine.Points.Select(x => x.Y).Max());
                                }

                                if (segment is QuadraticBezierSegment)
                                {
                                    var bez = (QuadraticBezierSegment)segment;
                                    minX = Math.Min(minX, new double[] { bez.Point1.X, bez.Point2.X }.Min());
                                    minY = Math.Min(minY, new double[] { bez.Point1.Y, bez.Point2.Y }.Min());

                                    maxX = Math.Max(maxX, new double[] { bez.Point1.X, bez.Point2.X }.Max());
                                    maxY = Math.Max(maxY, new double[] { bez.Point1.Y, bez.Point2.Y }.Max());
                                }
                            }
                    }
                }
            }

            var crop = new CroppedBitmap(rtb, new Int32Rect((int)minX, (int)minY, (int)maxX, (int)maxY));
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(crop));
            using (var fs = File.OpenWrite(imgFileName))
                pngEncoder.Save(fs);
        }

        public void ChangeSizeValue(DetailSize newSize)
        {
            foreach (var child in canvas.Children)
            {
                var tb = child as TextBox;
                if (tb == null)
                    continue;

                if (Tag == null)
                    continue;

                if (!tb.Tag.ToString().Equals(newSize.Id == 0 ? newSize.RuntimeId : newSize.Id.ToString()))
                    continue;

                tb.Text = newSize.Value.ToString();
            }
        }
        #endregion

        string st;
        string selectedTool
        {
            get
            {
                return st;
            }
            set
            {
                st = value;
                Console.WriteLine(value);
            }
        }
        Point clickPosition;
        UIElement drawingObj;
        UIElement drawingObj2;
        Point mousePosition;
        bool isSkip;
        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed ||/* isSkip ||*/
                string.IsNullOrEmpty(selectedTool))
                return;

            if (mousePosition.Equals(e.GetPosition(canvas)))
                return;

            if (Keyboard.IsKeyDown(Key.LeftShift))
                isShiftPressed = true;
            else
                isShiftPressed = false;

            mousePosition = e.GetPosition(canvas);

            int angleOrtogonalRange = 100;
            Line line = null;
            TextBox textBox = null;
            System.Windows.Shapes.Path path = null;
            switch (selectedTool)
            {
                #region line
                case "line":
                    line = (drawingObj as Line) ?? CreateLineObj();
                    line.RenderTransform = null;

                    if (isShiftPressed)
                    {
                        var angle = Math.Atan2(mousePosition.Y - clickPosition.Y, mousePosition.X - clickPosition.X) * (180 / Math.PI);

                        if ((angle >= -22.5 && angle <= 22.5) || (angle >= -157.5 && angle >= 157.5))
                        {//left, right
                            line.X2 = mousePosition.X;
                            line.Y2 = clickPosition.Y;
                        }

                        if ((angle < -45 && angle > -135) || (angle > 45 && angle < 135))
                        {//up, down
                            line.X2 = clickPosition.X;
                            line.Y2 = mousePosition.Y;
                        }

                        if ((angle < -22.5 && angle > -67.5) || (angle > 112.5 && angle < 157.5))
                        {//right up,down 45
                            line.X2 = mousePosition.X;
                            line.Y2 = clickPosition.Y;

                            line.RenderTransform = new RotateTransform(-45, clickPosition.X, clickPosition.Y);
                        }

                        if ((angle < -112.5 && angle > -157.5) || (angle > 22.5 && angle < 67.5))
                        {//left up,down 45
                            line.X2 = mousePosition.X;
                            line.Y2 = clickPosition.Y;

                            line.RenderTransform = new RotateTransform(45, clickPosition.X, clickPosition.Y);
                        }
                    }
                    else
                    {
                        line.X2 = mousePosition.X;
                        line.Y2 = mousePosition.Y;
                    }

                    if (drawingObj == null)
                    {
                        line.Name = $"line_{line.GetHashCode()}";
                        canvas.Children.Add(line);
                    }
                    drawingObj = line;
                    break;
                #endregion

                #region angle
                case "angle":
                    path = (drawingObj as System.Windows.Shapes.Path) ?? CreateArcObject();

                    var geomentry = (PathGeometry)path.Data;
                    var arc = (QuadraticBezierSegment)((PathGeometry)path.Data).Figures[0].Segments[0];
                    arc.Point1 = new Point(mousePosition.X, clickPosition.Y + clickPosition.Y * 0.05);
                    arc.Point2 = mousePosition;

                    bool isOrtogonal = false;

                    if (clickPosition.X - mousePosition.X <= angleOrtogonalRange &&
                        clickPosition.X - mousePosition.X > 0)
                    {//down&up, left
                        var y = clickPosition.Y + ((mousePosition.Y - clickPosition.Y) / 2);
                        var x = Math.Tan(-35) * y + clickPosition.X;
                        arc.Point1 = new Point(x, y);
                        isOrtogonal = true;
                    }

                    if (mousePosition.X - clickPosition.X <= angleOrtogonalRange &&
                        mousePosition.X - clickPosition.X >= 0)
                    {//down&up, right
                        var y = clickPosition.Y + ((mousePosition.Y - clickPosition.Y) / 2);
                        var x = Math.Tan(35) * y + clickPosition.X;
                        arc.Point1 = new Point(x, y);
                        isOrtogonal = true;
                    }

                    if (mousePosition.Y - clickPosition.Y <= angleOrtogonalRange &&
                       mousePosition.Y - clickPosition.Y > 0)
                    {//straight right&right, down
                        var x = clickPosition.X + ((mousePosition.X - clickPosition.X) / 2);
                        var y = Math.Tan(-25) * x + clickPosition.Y;
                        arc.Point1 = new Point(x, y);
                        isOrtogonal = true;
                    }

                    if (clickPosition.Y - mousePosition.Y <= angleOrtogonalRange &&
                       clickPosition.Y - mousePosition.Y > 0)
                    {//straight left&right, up
                        var x = clickPosition.X + ((mousePosition.X - clickPosition.X) / 2);
                        var y = Math.Tan(25) * x + clickPosition.Y;
                        arc.Point1 = new Point(x, y);
                        isOrtogonal = true;
                    }

                    geomentry.Figures[1].Segments.Clear();
                    geomentry.Figures[1].StartPoint = clickPosition;
                    geomentry.Figures[1].Segments.Add(DrawArrow(arc.Point1, clickPosition));

                    geomentry.Figures[2].Segments.Clear();
                    geomentry.Figures[2].StartPoint = mousePosition;
                    geomentry.Figures[2].Segments.Add(DrawArrow(arc.Point1, mousePosition));

                    textBox = (TextBox)drawingObj2 ?? CreateTextBox("angleTextBoxStyle");

                    double t = 0.5;
                    var p0 = ((PathGeometry)path.Data).Figures[0].StartPoint;
                    var p1 = arc.Point1;
                    var p2 = arc.Point2;
                    var yy = (1 - t) * (1 - t) * p0.Y + 2 * t * (1 - t) * p1.Y + t * t * p2.Y;
                    var xx = (1 - t) * (1 - t) * p0.X + 2 * t * (1 - t) * p1.X + t * t * p2.X;
                    var centerLabelPoint = new Point(xx, yy);

                    if (isOrtogonal)
                    {
                        Canvas.SetLeft(textBox, centerLabelPoint.X);
                        Canvas.SetTop(textBox, centerLabelPoint.Y);

                        if (mousePosition.X < clickPosition.X)
                            Canvas.SetLeft(textBox, Canvas.GetLeft(textBox) - textBox.DesiredSize.Width - 5);

                        if (mousePosition.Y < clickPosition.Y)
                            Canvas.SetTop(textBox, Canvas.GetTop(textBox) - textBox.DesiredSize.Height - 5);
                    }
                    else
                    {
                        if (clickPosition.X <= mousePosition.X)
                        {
                            Canvas.SetLeft(textBox, centerLabelPoint.X + textBox.DesiredSize.Width + 5);
                            Canvas.SetTop(textBox, centerLabelPoint.Y - textBox.DesiredSize.Height - 5);
                        }
                        else
                        {
                            Canvas.SetLeft(textBox, centerLabelPoint.X - textBox.DesiredSize.Width - 5);
                            Canvas.SetTop(textBox, centerLabelPoint.Y - textBox.DesiredSize.Height - 5);
                        }
                    }

                    path.Tag = $"arc_{path.GetHashCode()}";
                    textBox.Tag = $"textBoxArc_{path.GetHashCode()}";

                    if (drawingObj == null && drawingObj2 == null)
                    {
                        canvas.Children.Add(path);
                        canvas.Children.Add(textBox);
                    }
                    drawingObj = path;
                    drawingObj2 = textBox;
                    break;
                #endregion

                #region comment
                case "comment":
                    var geometry = new PathGeometry();
                    var fig = new PathFigure();
                    fig.StartPoint = clickPosition;

                    var lineArrow = new PolyLineSegment();
                    lineArrow.Points.Add(mousePosition);
                    fig.Segments.Add(lineArrow);
                    fig.Segments.Add(DrawArrow(mousePosition, clickPosition));
                    geometry.Figures.Add(fig);

                    var pathArrow = (System.Windows.Shapes.Path)drawingObj ?? new System.Windows.Shapes.Path();
                    pathArrow.PreviewMouseLeftButtonDown += Path_PreviewMouseLeftButtonDown;
                    pathArrow.Data = geometry;
                    pathArrow.Stroke = Brushes.Black;
                    pathArrow.StrokeThickness = LineWidth;
                    pathArrow.Fill = Brushes.Black;

                    textBox = (TextBox)drawingObj2 ?? CreateTextBox("commentTextBoxStyle");
                    textBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    textBox.BorderThickness = new Thickness(0, 0, 0, LineWidth);
                    Panel.SetZIndex(textBox, 2);

                    if (clickPosition.X <= mousePosition.X)
                    {
                        Canvas.SetTop(textBox, mousePosition.Y - textBox.DesiredSize.Height + 1);
                        Canvas.SetLeft(textBox, mousePosition.X - 1);
                    }
                    else
                    {
                        Canvas.SetTop(textBox, mousePosition.Y - textBox.DesiredSize.Height + 1);
                        Canvas.SetLeft(textBox, mousePosition.X - 1 - textBox.DesiredSize.Width);
                    }

                    pathArrow.Tag = $"pathComment_{pathArrow.GetHashCode()}";
                    textBox.Tag = $"textComment_{pathArrow.GetHashCode()}";

                    if (drawingObj == null && drawingObj2 == null)
                    {
                        canvas.Children.Add(pathArrow);
                        canvas.Children.Add(textBox);
                    }
                    drawingObj = pathArrow;
                    drawingObj2 = textBox;
                    break;
                #endregion

                case "pencil":
                    path = (System.Windows.Shapes.Path)drawingObj ?? CreatePencilPath();
                    PolyLineSegment segment = (PolyLineSegment)((PathGeometry)path.Data).Figures[0].Segments[0];
                    segment.Points.Add(mousePosition);

                    if (drawingObj == null)
                        canvas.Children.Add(path);
                    drawingObj = path;
                    break;

                case "text_move":
                    if (movingTextBox == null)
                        return;

                    var diffX = clickPosition.X - Canvas.GetLeft(movingTextBox);
                    var diffY = clickPosition.Y - Canvas.GetTop(movingTextBox);
                    if (mousePosition.X - clickPosition.X == 0)
                        diffX = 0;
                    if (mousePosition.Y - clickPosition.Y == 0)
                        diffY = 0;

                    Canvas.SetLeft(movingTextBox, Canvas.GetLeft(movingTextBox) + (mousePosition.X - clickPosition.X) - diffX);
                    Canvas.SetTop(movingTextBox, Canvas.GetTop(movingTextBox) + (mousePosition.Y - clickPosition.Y) - diffY);

                    clickPosition.X = Canvas.GetLeft(movingTextBox);
                    clickPosition.Y = Canvas.GetTop(movingTextBox);
                    break;
            }
        }


        System.Windows.Shapes.Path CreatePencilPath()
        {
            var path = new System.Windows.Shapes.Path();
            path.Stroke = Brushes.Black;
            path.StrokeThickness = LineWidth;

            var geometry = new PathGeometry();
            var fig = new PathFigure();
            fig.StartPoint = clickPosition;
            var segment = new PolyLineSegment();
            segment.Points.Add(clickPosition);
            fig.Segments.Add(segment);
            geometry.Figures.Add(fig);

            path.Data = geometry;
            path.PreviewMouseLeftButtonDown += Path_PreviewMouseLeftButtonDown;

            return path;
        }
        TextBox CreateTextBox(string styleName)
        {
            var tb = new TextBox();
            tb.FontSize = TextFontSize;
            tb.Style = (Style)Resources[styleName];

            tb.PreviewMouseLeftButtonDown += Tb_MouseLeftButtonDown;
            tb.PreviewMouseLeftButtonUp += Tb_MouseLeftButtonUp;

            tb.GotFocus += Tb_GotFocus;
            return tb;
        }

        TextBox focusedTextBox;
        private void Tb_GotFocus(object sender, RoutedEventArgs e)
        {
            focusedTextBox = sender as TextBox;
        }

        System.Windows.Shapes.Path CreateArcObject()
        {
            var pathGeometry = new PathGeometry();

            var pathFigure = new PathFigure();
            pathFigure.StartPoint = clickPosition;
            pathFigure.IsFilled = false;

            var arc = new QuadraticBezierSegment();
            pathFigure.Segments.Add(arc);
            pathGeometry.Figures.Add(pathFigure);

            pathGeometry.Figures.Add(new PathFigure());
            pathGeometry.Figures.Add(new PathFigure());

            var p = new System.Windows.Shapes.Path();
            p.Data = pathGeometry;
            p.Stroke = Brushes.Black;
            p.StrokeThickness = LineWidth;
            p.Fill = Brushes.Black;

            p.PreviewMouseLeftButtonDown += Path_PreviewMouseLeftButtonDown;
            return p;
        }
        Line CreateLineObj()
        {
            var line = new Line();
            line.Fill = Brushes.Black;
            line.Stroke = Brushes.Black;
            line.StrokeThickness = LineWidth;
            line.X1 = clickPosition.X;
            line.Y1 = clickPosition.Y;

            line.PreviewMouseLeftButtonDown += Line_PreviewMouseLeftButtonDown;

            return line;
        }

        public PolyLineSegment DrawArrow(Point a, Point b)
        {
            double HeadWidth = 10; // Ширина между ребрами стрелки
            double HeadHeight = 5; // Длина ребер стрелки

            double X1 = a.X;
            double Y1 = a.Y;

            double X2 = b.X;
            double Y2 = b.Y;

            double theta = Math.Atan2(Y1 - Y2, X1 - X2);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            Point pt3 = new Point(
                X2 + (HeadWidth * cost - HeadHeight * sint),
                Y2 + (HeadWidth * sint + HeadHeight * cost));

            Point pt4 = new Point(
                X2 + (HeadWidth * cost + HeadHeight * sint),
                Y2 - (HeadHeight * cost - HeadWidth * sint));

            PolyLineSegment arrow = new PolyLineSegment();
            arrow.Points.Add(b);
            arrow.Points.Add(pt3);
            arrow.Points.Add(pt4);
            arrow.Points.Add(b);

            return arrow;
        }

        string savedTool;
        TextBox movingTextBox;
        private void Tb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedTool))
                return;

            isSkip = false;           

            if (selectedTool.Equals("text_move"))
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                movingTextBox = null;
            }

            selectedTool = savedTool;
            savedTool = null;
        }
        private void Tb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedTool))
                return;

            if (selectedTool?.Equals("delete") ?? false)
            {
                var tb = sender as TextBox;
                if (!string.IsNullOrEmpty(tb.Name))
                {
                    var size = tb.DataContext as DetailSize;
                    OnSizeDeletedEvent(size);
                }
                canvas.Children.Remove((UIElement)sender);
            }
            else
            {
                if (string.IsNullOrEmpty(savedTool))
                    savedTool = selectedTool;
                selectedTool = "text_move";
                Mouse.OverrideCursor = Cursors.SizeAll;
                movingTextBox = sender as TextBox;
                clickPosition = e.GetPosition(canvas);
            }

            isSkip = true;
        }
        private void Path_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedTool))
                return;

            if (selectedTool.Equals("delete"))
            {
                var el = (System.Windows.Shapes.Path)sender;
                var tag = el.Tag.ToString().Split('_').Last();

                var toRemove = new List<object>();
                foreach (FrameworkElement child in canvas.Children)
                {
                    if (child.Tag != null)
                        if (child.Tag.ToString().Contains(tag))
                            toRemove.Add(child);
                }

                foreach (var r in toRemove)
                    canvas.Children.Remove((UIElement)r);
            }
        }
        private void Line_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedTool))
                return;

            var line = (Line)sender;
            if (line == null)
                return;

            if (selectedTool.Equals("line_length"))
            {
                var textBox = CreateTextBox("lengthTextBoxStyle");
                textBox.Name = $"lineLength_{line.Name.Split('_').Last()}";
                textBox.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                Canvas.SetLeft(textBox, line.X1 + (line.X2 - line.X1) / 2 - textBox.DesiredSize.Width - 5);
                Canvas.SetTop(textBox, line.Y1 + (line.Y2 - line.Y1) / 2 - textBox.DesiredSize.Height - 5);

                if (line.X1 < line.X2 && line.Y1 < line.Y2)
                {//right & down
                    Canvas.SetLeft(textBox, line.X1 + (line.X2 - line.X1) / 2 + 5);
                    Canvas.SetTop(textBox, line.Y2 + (line.Y1 - line.Y2) / 2 - textBox.DesiredSize.Height - 5);
                }

                if (line.X1 > line.X2 && line.Y1 > line.Y2)
                {//left, up
                    Canvas.SetLeft(textBox, line.X2 + (line.X1 - line.X2) / 2 - textBox.DesiredSize.Width - 5);
                    Canvas.SetTop(textBox, line.Y2 + (line.Y1 - line.Y2) / 2 - textBox.DesiredSize.Height - 5);
                }

                canvas.Children.Add(textBox);
                Keyboard.Focus(textBox);

                var size = OnCreateNewSizeEvent();
                textBox.DataContext = size;
                textBox.Tag = size.Id == 0 ? size.RuntimeId : size.Id.ToString();
                textBox.TextChanged += TextBox_TextChanged;
            }

            if (selectedTool.Equals("delete"))
            {
                string tag = line.Name.Split('_').Last();

                if (!string.IsNullOrEmpty(tag))
                {
                    canvas.Children.Remove((UIElement)sender);

                    var tags = new List<string>();
                    var toRemove = new List<object>();
                    foreach (FrameworkElement child in canvas.Children)
                    {
                        if (child.Name != null)
                        {
                            tags.Add(child.Name.ToString());
                            if (child.Name.ToString().Contains(tag))
                                toRemove.Add(child);
                        }
                    }

                    foreach (var r in toRemove)
                    {
                        if (r is TextBox)
                        {
                            var tb = r as TextBox;
                            OnSizeDeletedEvent(tb.DataContext as DetailSize);
                        }

                        canvas.Children.Remove((UIElement)r);
                    }
                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var obj = (sender as TextBox)?.DataContext as DetailSize;
            if (obj == null)
                return;

            obj.Value = int.Parse((sender as TextBox).Text);

            OnSizeChangedEvent(obj);
        }

        public delegate void OnSizeChangedEventHandler(DetailSize detailSize);
        public event OnSizeChangedEventHandler OnSizeChangedEvent;
        public delegate DetailSize OnNewSizeEventHandler();
        public event OnNewSizeEventHandler OnCreateNewSizeEvent;
        public delegate void OnSizeDeletedEventHandler(DetailSize size);
        public event OnSizeChangedEventHandler OnSizeDeletedEvent;

        #region canvas input events
        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickPosition = e.GetPosition(canvas);

            if (string.IsNullOrEmpty(selectedTool))
                return;

            if (selectedTool.Equals("text"))
            {
                var textBox = CreateTextBox("commentTextBoxStyle");
                Canvas.SetTop(textBox, clickPosition.Y);
                Canvas.SetLeft(textBox, clickPosition.X);

                canvas.Children.Add(textBox);
                textBox.Focus();
                Keyboard.Focus(textBox);
            }
        }

        private void canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedTool))
                return;

            if (selectedTool.Equals("angle") && drawingObj2 != null)
            {
                drawingObj2.Focus();
                Keyboard.Focus(drawingObj2);
            }

            if (drawingObj != null)
                drawingObj = null;
            if (drawingObj2 != null)
                drawingObj2 = null;
        }


        bool isShiftPressed;
        private void canvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                isShiftPressed = true;
        }

        private void canvas_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                isShiftPressed = false;
        }
        #endregion

        #region tool select
        void ResetSelection(string currName)
        {
            Mouse.OverrideCursor = Cursors.Arrow;

            foreach (ToggleButton c in stackPanel.Children)
            {
                if (!c.Name.Equals(currName))
                    c.IsChecked = false;
            }
        }

        private void btn_line_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_line.IsChecked.Value)
                selectedTool = "line";
            else
                selectedTool = "";
        }

        private void btn_angle_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_angle.IsChecked.Value)
                selectedTool = "angle";
            else
                selectedTool = "";
        }

        private void btn_comment_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_comment.IsChecked.Value)
                selectedTool = "comment";
            else
                selectedTool = "";
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_delete.IsChecked.Value)
                selectedTool = "delete";
            else
                selectedTool = "";
        }

        private void btn_lineLength_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_lineLength.IsChecked.Value)
                selectedTool = "line_length";
            else
                selectedTool = "";
        }

        private void btn_pencil_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_pencil.IsChecked.Value)
                selectedTool = "pencil";
            else
                selectedTool = "";
        }

        private void btn_text_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);

            if (btn_text.IsChecked.Value)
            {
                selectedTool = "text";
                Mouse.OverrideCursor = Cursors.Pen;
            }
            else
                selectedTool = "";
        }

        private void btn_width_Click(object sender, RoutedEventArgs e)
        {
            ResetSelection(((ToggleButton)sender).Name);
        }
        #endregion        
    }
}
