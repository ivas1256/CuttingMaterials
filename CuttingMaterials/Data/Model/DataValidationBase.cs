using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CuttingMaterials.Data.Model
{
    public abstract class DataValidationBase : IDataErrorInfo
    {
        public bool CanValidate { get; set; } = false;

        public bool Validate(DependencyObject validateControl)
        {
            CanValidate = true;
            ICollection<string> errors = new List<string>();
            var fields = GetType().GetProperties();
            foreach(var field in fields)
            {
                object ObjectMetaData = GetType().GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                MetadataTypeAttribute MetaData = ObjectMetaData as MetadataTypeAttribute;
                if (MetaData == null)
                    throw new NullReferenceException("Metadata null");
                Type metadataClassType = MetaData.MetadataClassType;

                var check = metadataClassType.GetProperties().Where(x => x.Name.Equals(field.Name));
                if (check == null || check.Count() == 0)
                    continue;
                var err = this[field.Name];
                if (!string.IsNullOrEmpty(err))
                    errors.Add(err);
            }

            if (errors.Count() == 0)
            {
                CanValidate = false;
                return true;
            }

            foreach (Control cb in FindLogicalChildren<Control>(validateControl))
            {
                if (cb is TextBox)
                    cb.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                if (cb is ComboBox)
                    cb.GetBindingExpression(ComboBox.SelectedValueProperty)?.UpdateTarget();
                if (cb is DatePicker)
                    cb.GetBindingExpression(DatePicker.SelectedDateProperty)?.UpdateTarget();
            }
            CanValidate = false;
            return false;
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                if (!CanValidate)
                    return null;

                object ObjectMetaData = GetType().GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
                MetadataTypeAttribute MetaData = ObjectMetaData as MetadataTypeAttribute;
                if (MetaData == null)
                    throw new NullReferenceException("Metadata null");
                Type metadataClassType = MetaData.MetadataClassType;

                var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                if (Validator.TryValidateProperty(
                        GetType().GetProperty(columnName).GetValue(this, new object[0])
                        , new ValidationContext(Activator.CreateInstance(metadataClassType), null, null)
                        {
                            MemberName = columnName
                        }
                        , validationResults))
                    return null;

                return validationResults.First().ErrorMessage;
            }
        }

        public IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }
    }
}
