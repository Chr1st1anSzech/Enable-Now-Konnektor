using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor.src.misc;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Enable_Now_Konnektor.src.db
{
    class ElementLogContext : DbContext
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string DatabasePath = Path.Combine(Util.GetApplicationRoot(), "db", "ElementLogging.db");
        private readonly string _tableName;
        private DbSet<ElementLog> ElementLogs { get; set; }

        public ElementLogContext(string tableName) {
            if( string.IsNullOrWhiteSpace(tableName))
            {
                _log.Fatal( Util.GetFormattedResource("ElementLogContextMessage01") );
                throw new ArgumentException( Util.GetFormattedResource("ElementLogContextMessage01") );
            }
            
            _tableName = tableName;
        }




        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Filename=" + DatabasePath, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ElementLog>().ToTable(_tableName);
            modelBuilder.Entity<ElementLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WasFound);
                entity.Property(e => e.Hash);
            });
            base.OnModelCreating(modelBuilder);
        }



        private void AddElementLog(Element element, bool wasFound = true)
        {
            Database.EnsureCreated();
            var log = GetElementLog(element);
            if (log == null)
            {
                ElementLogs.Add(new ElementLog { Id = element.Id, WasFound = wasFound, Hash = element.Hash });
            }
            else
            {
                log.WasFound = wasFound;
                log.Hash = element.Hash;
                UpdateElementLog(log);
            }
            
            SaveChanges();
        }

        public void RemoveElementLog(Element element)
        {
            Database.EnsureCreated();
            var log = GetElementLog(element);
            if (log != null)
            {
                ElementLogs.Remove(log);
            }
            
            SaveChanges();
        }

        public ElementLog GetElementLog(Element element)
        {
            Database.EnsureCreated();
            return ElementLogs.Find(element.Id);
        }

        public void UpdateElementLog(ElementLog elementLog)
        {
            Database.EnsureCreated();
            ElementLogs.Update(elementLog);
            SaveChanges();
        }

        public void SetElementFound(Element element, bool wasFound = true)
        {
            Database.EnsureCreated();
            var elementLog = GetElementLog(element);
            if( elementLog == null)
            {
                AddElementLog(element, wasFound);
            }
            else
            {
                elementLog.WasFound = wasFound;
                UpdateElementLog(elementLog);
            }
        }
    }
}
