using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Data.Model
{
    [MetadataType(typeof(TemplateMetadata))]
    public partial class Template : DataValidationBase
    {
        public string FullFileName
        {
            get
            {
                if (string.IsNullOrEmpty(ImageFileName))
                    return null;

                var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(currDir, ImageFileName);
            }
        }

        public bool IsFileExist
        {
            get
            {
                string previewImageFile = "";
                if (string.IsNullOrEmpty(ImageFileName))
                    previewImageFile = XamlPreviewImageFile;
                else
                    previewImageFile = ImageFileName;

                if (string.IsNullOrEmpty(previewImageFile))
                    return false;

                return File.Exists(Path.Combine(App.CurrDirectory, previewImageFile));
            }
        }

        public string CachedPreviewImageFileName
        {
            get
            {
                string previewImageFile = "";
                if (string.IsNullOrEmpty(ImageFileName))
                    previewImageFile = XamlPreviewImageFile;
                else
                    previewImageFile = ImageFileName;

                return Path.Combine(App.CacheDirectory, Path.GetFileName(previewImageFile));
            }
        }

        public string GetPreviewFileName()
        {
            return $"{Name}_preview.jpg";
        }

        public bool IsDrawingEnabled
        {
            get
            {
                return string.IsNullOrWhiteSpace(ImageFileName);
            }
        }
    }

    internal sealed class TemplateMetadata
    {
        [Required]
        [Display(Name = "Название")]
        public string Name { get; set; }
    }
}
