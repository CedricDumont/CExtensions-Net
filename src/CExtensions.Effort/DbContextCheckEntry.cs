using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.Effort
{
    public class DbContextCheckEntry
    {

        public static DbContextCheckEntry ForObject(string id, string objectName, Object objectValue, string idPropertyName)
        {
            DbContextCheckEntry entry = new DbContextCheckEntry();
            entry.ObjectId = id;
            entry.ObjectName = objectName;
            entry.ObjectValue = objectValue;
            entry.IdPropertyName = idPropertyName;
            return entry;
        }

        public DbContextCheckEntry WithProperty(string propertyName)
        {
            this.PropertyName = propertyName;
            return this;
        }

        public DbContextCheckEntry WithDescription(string description)
        {
            this.Description = description;
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

        public string IdPropertyName { get; set; }

        public String ObjectId { get; set; }

        public string ObjectName { get; set; }

        public string Description { get; set; }

        public Object ObjectValue { get; set; }

        public String PropertyName { get; set; }

        public string ActualPropertyContent { get; set; }

        public string ExpectedPropertyContent { get; set; }

        public override string ToString()
        {
            String result = "";
            string objectIdentifierText = "object with " + (IdPropertyName ?? "id") + " : " + ObjectId;
            if (ObjectValue == null)
            {
                result += ObjectName + " was null - " + objectIdentifierText;
            }
            else
            {
                // return   ObjectName + " with id " + ObjectId + " has " + PropertyName + " containing [" + ActualPropertyContent + "] instead of [" + ExpectedPropertyContent + "]";
                result += ObjectName + "." + PropertyName + " Should be [" + ExpectedPropertyContent + "] but was [" + ActualPropertyContent + "] - " + objectIdentifierText;
            }

            if(Description != null)
            {
                result += " (" + Description + ")";
            }

            return result;
        }
    }
}
