using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CExtensions.EntityFramework
{
    public class OriginalDbContextTracker : IDisposable
    {
        private IDictionary<Guid, DbContext> _dbContextCollection;

        public static OriginalDbContextTracker Instance = new OriginalDbContextTracker();

        private OriginalDbContextTracker()
        {
            _dbContextCollection = new Dictionary<Guid, DbContext>();
        }

        public DbContext AddTracker(DbContext toBeTracked)
        {
            // create an new DbContext
            DbContext tracker = new DbContext(toBeTracked.AsObjectContext().Connection, false);

            //Just to retrieve it later on
            Guid newGuid = Guid.NewGuid();

            //create the tracker delegate
            var delegateTracker = new InternalMaterializerDelegateTracker(newGuid);
            toBeTracked.AsObjectContext().ObjectMaterialized += delegateTracker.OriginalDbContextTracker_ObjectMaterialized;

            _dbContextCollection.Add(newGuid, tracker);

            return tracker;
        }

        public DbContext this[Guid guid]
        {
            get
            {
               return _dbContextCollection[guid];
            }

        }

        public void Dispose()
        {
            foreach (var item in _dbContextCollection.Values)
            {
                item.DisposeIfNotNull();
            }

            _dbContextCollection.Clear();
        }
    }

    internal class InternalMaterializerDelegateTracker
    {
        public InternalMaterializerDelegateTracker(Guid guid)
        {
            Guid = guid;
        }

        public  Guid Guid {  get; private set; }


        public void OriginalDbContextTracker_ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            try
            {
                //retrieve the tracker with the Guid
                DbContext tempContext = OriginalDbContextTracker.Instance[Guid];

                var clrType = ObjectContext.GetObjectType(e.Entity.GetType());
                DbSet dbset = tempContext.Set(clrType);
                var new_Entity = Activator.CreateInstance(clrType);
                //var new_Entity = dbset.Create(clrType);
                dbset.Add(new_Entity);
                tempContext.Entry(new_Entity).CurrentValues.SetValues(e.Entity);
                tempContext.Entry(new_Entity).State = EntityState.Unchanged;
                //var entry = tempContext.AsObjectContext().ObjectStateManager.GetObjectStateEntry(new_Entity);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
