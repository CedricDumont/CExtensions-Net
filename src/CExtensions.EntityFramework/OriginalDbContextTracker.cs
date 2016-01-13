using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace CExtensions.EntityFramework
{
    public class OriginalDbContextTracker<T> : IDisposable where T : DbContext
    {
        private IDictionary<Guid, Tuple<T, InternalMaterializerDelegateTracker<T>>> _OriginalValuesDbContextCollection;

        private IDictionary<Guid, T> _TrackedDbContextCollection;

        public static OriginalDbContextTracker<T> Instance = new OriginalDbContextTracker<T>();

        private OriginalDbContextTracker()
        {
            _OriginalValuesDbContextCollection = new Dictionary<Guid, Tuple<T, InternalMaterializerDelegateTracker<T>>>();
            _TrackedDbContextCollection = new Dictionary<Guid, T>();
        }

        public T AddTracker(T trackedContext)
        {
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(DbConnection), typeof(bool) });

            if (constructor == null)
            {
                throw new ArgumentException($"{typeof(T)} Should contain a constructor with a {typeof(DbConnection)} and {typeof(bool)}");
            }
            
            T originalValuesContext = (T)constructor.Invoke(new Object[] { trackedContext.AsObjectContext().Connection, false });

          
            //Just to retrieve it later on
            Guid newGuid = Guid.NewGuid();

            //create the tracker delegate
            var delegateTracker = new InternalMaterializerDelegateTracker<T>(originalValuesContext);
            trackedContext.AsObjectContext().ObjectMaterialized += delegateTracker.OriginalDbContextTracker_ObjectMaterialized;

            _OriginalValuesDbContextCollection.Add(newGuid, Tuple.Create(originalValuesContext, delegateTracker));
            _TrackedDbContextCollection.Add(newGuid, trackedContext);

            return trackedContext;
        }


        public void PauseTracking(T trackedContext)
        {
            var guid = GetGuidFromTrackedDbContext(trackedContext);

           // DbContext tracker = this[trackedContext];

            var materializer = _OriginalValuesDbContextCollection[guid].Item2;

            trackedContext.AsObjectContext().ObjectMaterialized -= materializer.OriginalDbContextTracker_ObjectMaterialized;
            
        }

        public void ContinueTracking(T trackedContext)
        {
            var guid = GetGuidFromTrackedDbContext(trackedContext);

           // DbContext tracker = this[trackedContext];

            var materializer = _OriginalValuesDbContextCollection[guid].Item2;

            trackedContext.AsObjectContext().ObjectMaterialized += materializer.OriginalDbContextTracker_ObjectMaterialized;

        }

        public void StopTracking(T trackedContext)
        {
            var guid = GetGuidFromTrackedDbContext(trackedContext);

            DbContext tracker = this[trackedContext];

            tracker.DisposeIfNotNull();

            _OriginalValuesDbContextCollection.Remove(guid);
            _TrackedDbContextCollection.Remove(guid);

        }

        public T this[Guid guid]
        {
            get
            {
                return _OriginalValuesDbContextCollection[guid].Item1;
            }

        }

        private Guid GetGuidFromTrackedDbContext(T trackedContext)
        {
            Guid guid = (from keypair in _TrackedDbContextCollection where keypair.Value == trackedContext select keypair.Key).FirstOrDefault();

            return guid;
        }

        public T this[T trackedContext]
        {
            get
            {
                var guid = GetGuidFromTrackedDbContext(trackedContext);

                return _OriginalValuesDbContextCollection[guid].Item1;
            }

        }

        public void Dispose()
        {
            foreach (var item in _OriginalValuesDbContextCollection.Values)
            {
                item.Item1.DisposeIfNotNull();
            }

            _OriginalValuesDbContextCollection.Clear();
            _TrackedDbContextCollection.Clear();
        }
    }

    internal class InternalMaterializerDelegateTracker<T> where T : DbContext
    {
        public InternalMaterializerDelegateTracker(T originalValuesContext)
        {
            OriginalValuesContext = originalValuesContext;
        }

        public T OriginalValuesContext { get; private set; }


        public void OriginalDbContextTracker_ObjectMaterialized(object sender, System.Data.Entity.Core.Objects.ObjectMaterializedEventArgs e)
        {
            try
            {
                //retrieve the tracker with the Guid
                DbContext tempContext = OriginalValuesContext;

                var clrType = ObjectContext.GetObjectType(e.Entity.GetType());
                DbSet dbset = tempContext.Set(clrType);
                var new_Entity = Activator.CreateInstance(clrType);
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
