﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace XMSIntellegoSync
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ExportHistoryEntities : DbContext
    {
        public ExportHistoryEntities()
            : base("name=ExportHistoryEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<CallReExport> CallReExports { get; set; }
        public virtual DbSet<ExportLog> ExportLogs { get; set; }
        public virtual DbSet<ExportTarget> ExportTargets { get; set; }
        public virtual DbSet<HI3Retrieve> HI3Retrieve { get; set; }
        public virtual DbSet<HotNumber> HotNumbers { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
    }
}
