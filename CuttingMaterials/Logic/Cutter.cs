using CuttingMaterials.Data.Model;
using CuttingMaterials.View.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CuttingMaterials.Logic
{
    class Cutter
    {
        ProjectViewModel projectVm;
        Project project
        {
            get
            {
                return projectVm?.Project;
            }
        }
        bool isDebug;
        public string DebugStatus { get; set; }

        public List<int[]> Result { get; set; }

        public Cutter(ProjectViewModel projectViewModel, bool isDebug)
        {
            this.isDebug = isDebug;
            this.projectVm = projectViewModel;
        }

        public void Calculate(List<int[]> previosCutting)
        {
            projectVm.Offcuts.Remove(project);
            Result = new List<int[]>();

            var detailHeights = new List<int>();
            foreach (var detail in project.Details)
            {
                int height = detail.Sizes.Select(x => x.GetValue).Sum();
                if (height == 0)
                    continue;

                for (int i = 0; i < detail.Amount; i++)
                    detailHeights.Add(height);
            }

            if (previosCutting.Count != 0)
            {
                var newDetailHeights = new List<int>(detailHeights);
                foreach (var oldDetailHeight in previosCutting.SelectMany(x => x))
                    newDetailHeights.Remove(oldDetailHeight);

                if (newDetailHeights.Count != 0)
                    CalculateExisting(previosCutting, newDetailHeights);
            }
            else
                CalculateNew(detailHeights);

            projectVm.SaveChanges();
        }
        
        private void CalculateExisting(List<int[]> oldCutting, List<int> newDetails)
        {
            Result = new List<int[]>(oldCutting);

        }

        private void CalculateNew(List<int> newDetails)
        {
            while (newDetails.Count != 0)
            {
                var solver = new BackpackProblem(newDetails);
                var solv = solver.CalcDP(project.BlankHeight, newDetails.Count);

                Result.Add(solv.ToArray());
                projectVm.Offcuts.Add(new Offcut()
                {
                    ProjectId = project.Id,
                    Size = $"{project.BlankHeight - solv.Sum()}x{project.BlankWidth}"
                });
                foreach (var s in solv)
                    newDetails.Remove(s);
            }
        }

        public void CalculateExisting(List<int[]> existingResult)
        {
            if (existingResult.Count == 0)
                throw new Exception("call calcNew plz");

            projectVm.Offcuts.Remove(project);
            Result = new List<int[]>();

            var detailHeights = new List<int>();
            foreach (var detail in project.Details)
            {
                int height = 0;
                foreach (var size in detail.Sizes)
                    height += size.GetValue;

                if (height == 0)
                    continue;

                for (int i = 0; i < detail.Amount; i++)
                    detailHeights.Add(height);
            }

            var tmp = new List<int[]>();
            foreach(var comb in existingResult)
            {
                var newComb = new List<int>();
                foreach(var h in comb)
                {
                    if (detailHeights.Contains(h))
                        newComb.Add(h);
                }
                tmp.Add(newComb.ToArray());
            }

            existingResult = tmp;

            foreach (var existingDetail in existingResult.SelectMany(x => x))
            {
                if (detailHeights.Contains(existingDetail))
                    detailHeights.Remove(existingDetail);
            }

            if (detailHeights.Count == 0)//изменений нет
                Result = existingResult;
            else                
                foreach (var combination in existingResult)//добавляем детали к существующим листам
                {
                    var solver = new BackpackProblem(detailHeights);
                    var combAppend = solver.CalcDP(project.BlankHeight - combination.Sum(), detailHeights.Count);
                    if (combAppend.Count != 0)
                    {
                        foreach (var detailHeight in combAppend)
                            detailHeights.Remove(detailHeight);

                        var newComb = new List<int>();
                        newComb.AddRange(combination);
                        newComb.AddRange(combAppend);

                        Result.Add(newComb.ToArray());

                        if (project.BlankHeight - newComb.Sum() != 0)
                            projectVm.Offcuts.Add(new Offcut()
                            {
                                ProjectId = project.Id,
                                Size = $"{project.BlankHeight - newComb.Sum()}x{project.BlankWidth}"
                            });
                    }
                    else
                        Result.Add(combination);
                }

            while (detailHeights.Count != 0)
            {
                var solver = new BackpackProblem(detailHeights);

                var offcutsDict = new Dictionary<int, List<int>>();
                var usedComb = new Dictionary<int, int[]>();

                var solv = solver.CalcDP(project.BlankHeight, detailHeights.Count);
                offcutsDict.Add(solv.Sum(), solv);

                Result.Add(solv.ToArray());

                if (project.BlankHeight - solv.Sum() != 0)
                    projectVm.Offcuts.Add(new Offcut()
                    {
                        ProjectId = project.Id,
                        Size = $"{project.BlankHeight - solv.Sum()}x{project.BlankWidth}"
                    });
            }

            projectVm.SaveChanges();
        }

        public void CalculateNew()
        {
            projectVm.Offcuts.Remove(project);

            Result = new List<int[]>();

            var detailHeights = new List<int>();
            foreach (var detail in project.Details)
            {
                int height = 0;
                foreach (var size in detail.Sizes)
                    height += size.GetValue;

                if (height == 0)
                    continue;

                for (int i = 0; i < detail.Amount; i++)
                    detailHeights.Add(height);
            }

            while (detailHeights.Count != 0)
            {
                var solver = new BackpackProblem(detailHeights);
                var solv = solver.CalcDP(project.BlankHeight, detailHeights.Count);

                Result.Add(solv.ToArray());
                projectVm.Offcuts.Add(new Offcut()
                {
                    ProjectId = project.Id,
                    Size = $"{project.BlankHeight - solv.Sum()}x{project.BlankWidth}"
                });
                foreach (var s in solv)
                    detailHeights.Remove(s);
            }
            projectVm.SaveChanges();
        }

        public string CalcOffcut(List<DefaultDetail> defaultDetails)
        {
            if (Result.Count == 0)
                return null;
            var res = new StringBuilder();
            res.AppendLine("Из остатков можно изготовить:");
            var detailHeights = new List<int>();

            foreach (var detail in defaultDetails)
            {
                if (detail.Size == 0)
                    continue;

                int count = 0;
                foreach (var comb in Result)
                {
                    var remainder = project.BlankHeight - comb.Sum();
                    count += remainder / detail.Size;
                }
                res.AppendLine($"  {detail.Name} - {count} шт.");

                for (int i = 0; i < count; i++)
                    detailHeights.Add(detail.Size);
            }


            var combination = new List<List<int>>();
            foreach (var comb in Result)
            {
                if (detailHeights.Count == 0)
                    continue;

                var remainder = project.BlankHeight - comb.Sum();

                var solver = new BackpackProblem(detailHeights);
                var solv = solver.CalcDP(remainder, detailHeights.Count);

                combination.Add(solv);
                foreach (var s in solv)
                    detailHeights.Remove(s);
            }
            res.AppendLine("Оптимальная комбинация:");

            if (combination.Count == 0)
                res.AppendLine("Нет");

            foreach (var comb in combination)
            {
                var arr = comb.Select(x => defaultDetails.Find(y => y.Size == x).Name);

                res.AppendLine($"  Лист №{combination.IndexOf(comb) + 1} - {string.Join(", ", arr)}");
            }

            return res.ToString();

        }

        public List<BitmapSource> Bitmaps { get; set; } = new List<BitmapSource>();
        public void Render(Canvas canvas, int canvasWidth, bool isSplitToFiles)
        {
            canvas.Children.Clear();

            int listsPerWidth = 1;
            if (!isSplitToFiles)
                listsPerWidth = 2;
            int margin = 30;
            var listWidth = (canvasWidth / listsPerWidth) - margin * listsPerWidth;
            var listHeight = (listWidth * project.BlankHeight) / project.BlankWidth;

            canvas.Width = listWidth * listsPerWidth + margin * 4;
            canvas.Height = listHeight + margin * listsPerWidth;

            byte index = 0;
            int j = 0;
            for (int i = 0; i < Result.Count; i++)
            {
                if (!isSplitToFiles)
                {
                    j = i;
                }

                #region list
                var rect = new Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Black);
                rect.StrokeThickness = 1;
                rect.Width = listWidth;
                rect.Height = listHeight;
                canvas.Children.Add(rect);

                Canvas.SetTop(rect, margin * (j / 2 == 0 ? 1 : j / 2 + 1) + listHeight * (j / 2));
                Canvas.SetLeft(rect, margin * (index + 1) + listWidth * (index));
                #endregion

                #region list width label
                var text = CreateDefaultTextBlock();
                text.Text = project.BlankWidth.ToString();

                canvas.Children.Add(text);
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetTop(text, Canvas.GetTop(rect) - text.DesiredSize.Height);
                Canvas.SetLeft(text, Canvas.GetLeft(rect) + listWidth / 2);
                #endregion

                #region list height label
                text = CreateDefaultTextBlock();
                text.Text = project.BlankHeight.ToString();
                text.RenderTransform = new RotateTransform(90);

                canvas.Children.Add(text);
                text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetTop(text, Canvas.GetTop(rect) + listHeight / 2);
                Canvas.SetLeft(text, Canvas.GetLeft(rect) + listWidth + text.DesiredSize.Height);
                #endregion

                #region details
                double savedHeight = 0;
                Point verySmallDetailBeginPoint = new Point();
                //foreach (int detailOrigHeight in Result[i])
                for (int f = 0; f < Result[i].Count(); f++)
                {
                    int detailOrigHeight = Result[i][f];
                    var detailHeight = (detailOrigHeight * listHeight) / project.BlankHeight;

                    var line = new Line();
                    line.Stroke = Brushes.Black;
                    line.StrokeThickness = 1;
                    canvas.Children.Add(line);

                    line.X1 = Canvas.GetLeft(rect);
                    line.Y1 = Canvas.GetTop(rect) + detailHeight + savedHeight;
                    line.X2 = Canvas.GetLeft(rect) + listWidth;
                    line.Y2 = Canvas.GetTop(rect) + detailHeight + savedHeight;

                    if (detailOrigHeight > 50)
                    {
                        text = new TextBlock()
                        {
                            FontFamily = new FontFamily("Tahoma"),
                        };
                        text.Text = detailOrigHeight.ToString();
                        text.RenderTransform = new RotateTransform(90);
                        canvas.Children.Add(text);

                        text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        Canvas.SetTop(text, Canvas.GetTop(rect) + detailHeight / 2 + savedHeight - text.DesiredSize.Width / 2);
                        Canvas.SetLeft(text, Canvas.GetLeft(rect) + text.DesiredSize.Height);

                        text = new TextBlock()
                        {
                            FontFamily = new FontFamily("Tahoma"),
                        };
                        text.Text = project.BlankWidth.ToString();
                        canvas.Children.Add(text);

                        text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        Canvas.SetTop(text, Canvas.GetTop(rect) + detailHeight + savedHeight - text.DesiredSize.Height);
                        Canvas.SetLeft(text, Canvas.GetLeft(rect) + listWidth / 2);
                    }
                    else if (verySmallDetailBeginPoint.Equals(new Point()))
                        verySmallDetailBeginPoint = new Point(line.X2, line.Y2 - detailHeight);

                    if (!verySmallDetailBeginPoint.Equals(new Point()))
                        if (f + 1 == Result[i].Count() || Result[i][f + 1] != Result[i][f])
                        {//рисуем скобочку справа
                            var polyLine = new PolyLineSegment();

                            polyLine.Points.Add(new Point(verySmallDetailBeginPoint.X + 10, verySmallDetailBeginPoint.Y + 5));
                            polyLine.Points.Add(new Point(line.X2 + 10, line.Y1 - 5));
                            polyLine.Points.Add(new Point(line.X2 + 5, line.Y1 - 5));

                            var fig = new PathFigure();
                            fig.StartPoint = new Point(verySmallDetailBeginPoint.X + 5, verySmallDetailBeginPoint.Y + 5);
                            fig.Segments.Add(polyLine);
                            var geom = new PathGeometry();
                            geom.Figures.Add(fig);
                            var path = new System.Windows.Shapes.Path();
                            path.Stroke = Brushes.Black;
                            path.StrokeThickness = 1;
                            path.Data = geom;
                            canvas.Children.Add(path);

                            text = new TextBlock()
                            {
                                FontFamily = new FontFamily("Tahoma"),
                            };
                            text.Text = detailOrigHeight.ToString();
                            text.RenderTransform = new RotateTransform(90);
                            canvas.Children.Add(text);

                            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                            Canvas.SetTop(text, verySmallDetailBeginPoint.Y + 5 + ((line.Y1 - 5) - (verySmallDetailBeginPoint.Y + 5)) / 2 - text.DesiredSize.Height / 2);
                            Canvas.SetLeft(text, verySmallDetailBeginPoint.X + 5 + 7 + text.DesiredSize.Width);

                            verySmallDetailBeginPoint = new Point();
                        }

                    savedHeight += detailHeight;
                }
                #endregion

                #region offcut hatching
                int offcut = project.BlankHeight - Result[i].Sum();
                if (offcut != 0)
                {
                    var restRect = new Rectangle();
                    restRect.Fill = (Brush)canvas.Resources["HatchBrush"];
                    restRect.Width = listWidth - 2;
                    restRect.Height = listHeight - savedHeight - 2;
                    restRect.StrokeThickness = 0;
                    canvas.Children.Add(restRect);

                    Canvas.SetTop(restRect, Canvas.GetTop(rect) + savedHeight + 1);
                    Canvas.SetLeft(restRect, Canvas.GetLeft(rect) + 1);

                    text = new TextBlock()
                    {
                        FontFamily = new FontFamily("Tahoma"),
                        Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff3232"))
                    };
                    text.Text = offcut.ToString();
                    text.RenderTransform = new RotateTransform(-90);
                    canvas.Children.Add(text);
                    text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                    Canvas.SetTop(text, Canvas.GetTop(rect) + savedHeight + ((listHeight - savedHeight) / 2 + text.DesiredSize.Width / 2));
                    Canvas.SetLeft(text, Canvas.GetLeft(rect) - text.DesiredSize.Height);
                }
                #endregion

                index++;
                if (index > listsPerWidth - 1)
                {
                    if (isSplitToFiles)
                    {
                        Bitmaps.Add(ToImageSource(canvas));
                        canvas.Children.Clear();
                    }
                    index = 0;
                }
            }

            if (Bitmaps.Count == 0 && isSplitToFiles)
            {
                Bitmaps.Add(ToImageSource(canvas));
                canvas.Children.Clear();
            }

            canvas.Width = listWidth * 2 + margin * 4;
            canvas.Height = listHeight * Result.Count + margin * (Result.Count + 1);
        }

        TextBlock CreateDefaultTextBlock()
        {
            return new TextBlock()
            {
                FontStyle = FontStyles.Italic,
                FontFamily = new FontFamily("Tahoma")
            };
        }

        BitmapSource ToImageSource(Canvas obj)
        {
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            Size size = new Size(obj.Width, obj.Height);

            obj.Measure(size);
            obj.Arrange(new Rect(size));

            obj.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)obj.Width, (int)obj.Height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;

            return bmp;
        }
    }
}