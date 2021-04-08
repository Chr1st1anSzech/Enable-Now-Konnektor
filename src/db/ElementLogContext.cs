using Enable_Now_Konnektor.src.enable_now;
using Enable_Now_Konnektor_Bibliothek.src.misc;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Enable_Now_Konnektor.src.db
{
    internal class ElementLogContext : DbContext
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string DatabaseDirPath = Path.Combine(Util.GetApplicationRoot(), "db");
        private static readonly string DatabaseFilePath = Path.Combine(DatabaseDirPath, "ElementLogging.db");
        private DbSet<ElementLog> ElementLogs { get; set; }

        private static readonly object objLock = new object();


        internal void Initialize()
        {
            lock (objLock)
            {
                Database.EnsureCreated();
            }
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!Directory.Exists(DatabaseDirPath))
            {
                Directory.CreateDirectory(DatabaseDirPath);
            }
            optionsBuilder.UseSqlite(@"Filename=" + DatabaseFilePath, options =>
            {
                options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
            });
            base.OnConfiguring(optionsBuilder);
        }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ElementLog>().ToTable("Enable_Now_Data");
            modelBuilder.Entity<ElementLog>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.JobId });
                entity.Property(e => e.WasFound);
                entity.Property(e => e.Hash);
            });
            base.OnModelCreating(modelBuilder);
        }

        internal void ResetAllFoundStatus(string jobId)
        {
            Database.EnsureCreated();
            var elementLogs = GetAllElementLogs(e => e.JobId == jobId);
            foreach (var elementLog in elementLogs)
            {
                elementLog.WasFound = false;
            }
            SaveChanges();
        }

        private void AddElementLog(Element element, string jobId, bool wasFound = true)
        {
            Database.EnsureCreated();
            var log = GetElementLog(element.Id, jobId);
            if (log == null)
            {
                ElementLogs.Add(new ElementLog { Id = element.Id, WasFound = wasFound, Hash = element.Hash, JobId = jobId });
            }
            else
            {
                log.WasFound = wasFound;
                log.Hash = element.Hash;
                UpdateElementsLog(log);
            }

            SaveChanges();
        }

        internal void RemoveElementLog(string elementId, string jobId)
        {
            Database.EnsureCreated();
            var log = GetElementLog(elementId, jobId);
            if (log != null)
            {
                ElementLogs.Remove(log);
            }

            SaveChanges();
        }

        internal IEnumerable<ElementLog> GetAllElementLogs(Func<ElementLog, bool> condition)
        {
            Database.EnsureCreated();
            return ElementLogs.Where(condition);
        }

        internal ElementLog GetElementLog(Element element, string jobId)
        {
            Database.EnsureCreated();
            return ElementLogs.Find(element.Id, jobId);
        }

        internal ElementLog GetElementLog(string elementId, string jobId)
        {
            Database.EnsureCreated();
            return ElementLogs.Find(elementId, jobId);
        }

        internal void UpdateElementsLog(params ElementLog[] elementLogs)
        {
            Database.EnsureCreated();
            ElementLogs.UpdateRange(elementLogs);
            SaveChanges();
        }

        internal void SetElementFound(Element element, string jobId, bool wasFound = true)
        {
            Database.EnsureCreated();
            var elementLog = GetElementLog(element.Id, jobId);
            if (elementLog == null)
            {
                AddElementLog(element, jobId, wasFound);
            }
            else
            {
                elementLog.WasFound = wasFound;
                UpdateElementsLog(elementLog);
            }
        }
    }
}
