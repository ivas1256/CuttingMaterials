using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Logic
{
    class PageNumbersWriter
    {
        public static byte[] AddPageNumbers(string fileName)
        {
            using (MemoryStream ms = new MemoryStream())
            using (PdfReader reader = new PdfReader(fileName))
            {
                int n = reader.NumberOfPages;
                Rectangle psize = reader.GetPageSize(1);

                using (Document document = new Document(psize, 50, 50, 50, 50))
                {
                    using (PdfWriter writer = PdfWriter.GetInstance(document, ms))
                    {

                        document.Open();
                        PdfContentByte cb = writer.DirectContent;

                        int p = 0;
                        for (int page = 1; page <= reader.NumberOfPages; page++)
                        {
                            document.NewPage();
                            p++;

                            PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                            cb.AddTemplate(importedPage, 0, 0);

                            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                            cb.BeginText();
                            cb.SetFontAndSize(bf, 10);
                            cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "   " + p.ToString(), 7, 44, 0);
                            cb.EndText();
                        }
                        document.Close();

                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
