using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.EntityFramework
{
    public class DbContextCheckResult
    {
        public DbContextCheckResult(bool areEqual = true, DbContextCheckEntry entry = null)
        {
            this.AreEqual = areEqual;
            Differences = new List<DbContextCheckEntry>();
            if (entry != null)
            {
                Differences.Add(entry);
            }
        }

        public DbContextCheckResult(bool areEqual, IEnumerable<DbContextCheckEntry> entries) : this(areEqual, (DbContextCheckEntry)null)
        {
            foreach (var item in entries)
            {
                this.Differences.Add(item);
            }
        }

        public bool AreEqual { get; set; }

        public IList<DbContextCheckEntry> Differences { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Found at least " + Differences.Count + " Differences : ");
            int counter = 1;
            foreach(var diff in Differences)
            {
                sb.AppendLine(counter++ + " / " + diff.ToString());
            }
            return sb.ToString();
        }

    }
}
