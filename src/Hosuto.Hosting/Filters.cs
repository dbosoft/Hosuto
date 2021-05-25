using System;
using System.Collections.Generic;
using System.Linq;

namespace Dbosoft.Hosuto
{
    public static class Filters
    {

        public static Action<T1> BuildFilterPipeline<T1>(IEnumerable<IFilter<T1>> filters, Action<T1> filteredDelegate)
        {
            if (filteredDelegate == null) throw new ArgumentNullException(nameof(filteredDelegate));

            return (p1 =>
            {
                var enumerable = filters.Reverse().ToList();
                if (enumerable.Count == 0)
                {
                    filteredDelegate(p1);
                    return;
                }

                var pipeline = enumerable.Aggregate(filteredDelegate, (current, filter) => filter.Invoke(current));
                pipeline(p1);

            });
        }

        public static Action<T1,T2> BuildFilterPipeline<T1, T2>(IEnumerable<IFilter<T1, T2>> filters, Action<T1,T2> filteredDelegate)
        {
            if (filteredDelegate == null) throw new ArgumentNullException(nameof(filteredDelegate));
            return ((p1, p2) =>
            {
                var enumerable = filters.Reverse().ToList();
                if (enumerable.Count == 0)
                {
                    filteredDelegate(p1,p2);
                    return;
                }

                var pipeline = enumerable.Aggregate(filteredDelegate, (current, filter) => filter.Invoke(current));
                pipeline?.Invoke(p1,p2);

            });
        }
        



    }
}
