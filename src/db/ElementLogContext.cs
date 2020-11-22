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
    internal class ElementLogContext : DbContext
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string DatabasePath = Path.Combine(Util.GetApplicationRoot(), "db", "ElementLogging.db");
        private readonly string _tableName;
        private DbSet<ElementLog> ElementLogs { get; set; }

        internal ElementLogContext(string tableName) {
            if( string.IsNullOrWhiteSpace(tableName))
            {
                _log.Fatal( Util.GetFormattedResource("ElementLogContextMessage01") );
                throw new ArgumentException( Util.GetFormattedResource("ElementLogContextMessage01") );
            }
            
            _tableName = tableName;
        }

        internal void Initialize()
        {
            Database.EnsureCreated();
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

        internal async void ResetAllFoundStatus()
        {
            await ElementLogs.ForEachAsync(elementLog =>
           {
               elementLog.WasFound = false;
           });
            SaveChanges();
        }

        private void AddElementLog(Element element, bool wasFound = true)
        {
            Database.EnsureCreated();
            var log = GetElementLog(element.Id);
            if (log == null)
            {
                ElementLogs.Add(new ElementLog { Id = element.Id, WasFound = wasFound, Hash = element.Hash });
            }
            else
            {
                log.WasFound = wasFound;
                log.Hash = element.Hash;
                UpdateElementsLog(log);
            }
            
            SaveChanges();
        }

        internal void RemoveElementLog(string id)
        {
            Database.EnsureCreated();
            var log = GetElementLog(id);
            if (log != null)
            {
                ElementLogs.Remove(log);
            }

            SaveChanges();
        }

        internal IEnumerable<ElementLog> GetAllElementLogs( Func<ElementLog, bool> condition)
        {
            Database.EnsureCreated();
            return ElementLogs.Where(condition);
        }

        internal ElementLog GetElementLog(Element element)
        {
            Database.EnsureCreated();
            return ElementLogs.Find(element.Id);
        }

        internal ElementLog GetElementLog(string id)
        {
            Database.EnsureCreated();
            return ElementLogs.Find(id);
        }

        internal void UpdateElementsLog(params ElementLog[] elementLogs)
        {
            Database.EnsureCreated();
            ElementLogs.UpdateRange(elementLogs);
            SaveChanges();
        }

        internal void SetElementFound(Element element, bool wasFound = true)
        {
            Database.EnsureCreated();
            var elementLog = GetElementLog(element.Id);
            if( elementLog == null)
            {
                AddElementLog(element, wasFound);
            }
            else
            {
                elementLog.WasFound = wasFound;
                UpdateElementsLog(elementLog);
            }
        }
    }
}
