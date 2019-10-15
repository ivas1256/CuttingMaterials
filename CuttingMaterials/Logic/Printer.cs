using CuttingMaterials.View.ViewModel;
using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

namespace CuttingMaterials.Logic
{
    class Printer
    {
        const string PATTREN_FILE_NAME = "Заявка шаблон.docx";
        ProjectViewModel projectVm;

        public Printer(ProjectViewModel projectVm)
        {
            this.projectVm = projectVm;
            if (!projectVm.IsCuttingCalc)
                projectVm.CalcCutting();
        }

        public FixedDocument CreateDocument()
        {
            using (var stream = new FileStream("PrintTemplate.lqd", FileMode.Open))
            {
                using (var reader = new StreamReader(stream))
                {
                    if (projectVm.Project.Details.Count == 0)
                        return null;

                    for (int i = 0; i < projectVm.Project.Details.Count; i++)
                    {
                        var currDetail = projectVm.Project.Details.ToList()[i];
                        currDetail.Index = i + 1;
                        currDetail.ListsToDo = projectVm.CuttingResult
                            .Where(x => x.Contains(currDetail.Sizes.Select(y => y.GetValue).Sum())).Count();

                        if (!string.IsNullOrEmpty(currDetail.Template?.ImageFileName))
                        {
                            currDetail.ImageFileName = Path.Combine(App.CurrDirectory, currDetail.Template.ImageFileName);
                        }
                        else if (!string.IsNullOrEmpty(currDetail.Xaml) &&
                            !string.IsNullOrEmpty(currDetail.PreviewFile))
                        {
                            currDetail.ImageFileName = Path.Combine(App.CurrDirectory, currDetail.PreviewFile);
                        }
                    }
                    var cuttingFiles = new List<string>();
                    int j = 0;
                    foreach (var bitmap in projectVm.Bitmaps)
                    {
                        using (var fileStream = new FileStream($"cutting{j++}.png", FileMode.Create))
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            encoder.Save(fileStream);
                        }

                        cuttingFiles.Add(Path.Combine(App.CurrDirectory, $"cutting{j - 1}.png"));
                    }

                    var dict = new Dictionary<string, object>()
                        {
                            {"CustomerFio",  projectVm.Project.CustomerFio},
                            {"OrderNumber",  projectVm.Project.OrderNumber},
                            {"StartDate",  projectVm.Project.StartDate.Value.ToString("dd.MM.yyyy")},
                            {"EndDate",  projectVm.Project.EndDate.Value.ToString("dd.MM.yyyy")},
                            {"Details", projectVm.Project.Details },
                            {"Coating", projectVm.GetCoating().Name},
                            {"Length", projectVm.Project.BlankWidth},
                            {"FactListsForDo", projectVm.CuttingResult.Count},
                            {"CoatingColor", projectVm.Project.CoatingColor },
                            {"BlankThickness", projectVm.Project.BlankThickness },
                            {"CuttingFiles", cuttingFiles }
                        };

                    var templateString = reader.ReadToEnd();
                    var template = DotLiquid.Template.Parse(templateString);
                    var docContext = Hash.FromDictionary(dict);
                    var docString = template.Render(docContext);

                    var flow = ((FlowDocument)XamlReader.Parse(docString));

                    return GetFixedDocument(flow);
                }
            }
        }

        public void CreatePdf(string fileName)
        {
            var doc = CreateDocument();
            using (var stream = new FileStream(fileName.Replace(".pdf", ".xps"), FileMode.Create))
            {
                using (var package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite))
                {
                    using (var xpsDoc = new XpsDocument(package, CompressionOption.Maximum))
                    {
                        var rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                        rsm.SaveAsXaml(paginator);
                        rsm.Commit();
                    }
                }
                stream.Position = 0;

                var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(stream);
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, fileName, 0);
                pdfXpsDoc.Close();
            }

            File.WriteAllBytes(fileName, PageNumbersWriter.AddPageNumbers(fileName));
            File.Delete(fileName.Replace(".pdf", ".xps"));
        }

        FixedDocument GetFixedDocument(FlowDocument doc)
        {
            var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
            var package = Package.Open(new MemoryStream(), FileMode.Create, FileAccess.ReadWrite);
            var packUri = new Uri("pack://temp.xps");
            PackageStore.RemovePackage(packUri);
            PackageStore.AddPackage(packUri, package);
            var xps = new XpsDocument(package, CompressionOption.NotCompressed, packUri.ToString());
            XpsDocument.CreateXpsDocumentWriter(xps).Write(paginator);
            return xps.GetFixedDocumentSequence().References[0].GetDocument(true);
        }
    }
}
