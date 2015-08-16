using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Effort
{
    public class DbContextCheckEntry
    {

        public static DbContextCheckEntry ForObject(string id, string objectName)
        {
            DbContextCheckEntry entry = new DbContextCheckEntry();
            entry.ObjectId = id;
            entry.ObjectName = objectName;
            return entry;
        }

        public DbContextCheckEntry WithProperty(string propertyName)
        {
            this.PropertyName = propertyName;
            return this;
        }

        public DbContextCheckEntry Was(string actualPropertyContent)
        {
            this.ActualPropertyContent = actualPropertyContent;
            return this;
        }

        public DbContextCheckEntry InsteadOf(string expectedPropertyContent)
        {
            this.ExpectedPropertyContent = expectedPropertyContent;
            return this;
        }

        public String ObjectId { get; set; }

        public string ObjectName { get; set; }

        public String PropertyName { get; set; }

        public string ActualPropertyContent { get; set; }

        public string ExpectedPropertyContent { get; set; }

        public override string ToString()
        {
            // return   ObjectName + " with id " + ObjectId + " has " + PropertyName + " containing [" + ActualPropertyContent + "] instead of [" + ExpectedPropertyContent + "]";
            return ObjectName + "." + PropertyName  + " Should be [" + ExpectedPropertyContent + "] but was [" + ActualPropertyContent + "] - object id : " + ObjectId;
        }
    }
}
