//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CuttingMaterials.Data.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class Detail
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Detail()
        {
            this.Sizes = new HashSet<DetailSize>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProjectId { get; set; }
        public Nullable<int> TemplateId { get; set; }
        public int Amount { get; set; }
        public string Xaml { get; set; }
        public string PreviewFile { get; set; }
    
        public virtual Template Template { get; set; }
        public virtual Project Project { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DetailSize> Sizes { get; set; }
    }
}
