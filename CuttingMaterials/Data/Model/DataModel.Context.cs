﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Coating> Coating { get; set; }
        public virtual DbSet<Project> Project { get; set; }
        public virtual DbSet<Template> Template { get; set; }
        public virtual DbSet<Detail> Detail { get; set; }
        public virtual DbSet<DetailSize> DetailSize { get; set; }
        public virtual DbSet<TemplateSize> TemplateSize { get; set; }
        public virtual DbSet<DefaultDetail> default_detail { get; set; }
        public virtual DbSet<Offcut> Offcut { get; set; }
        public virtual DbSet<Node> Node { get; set; }
    }
}
