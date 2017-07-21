using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class IListEqualsComparer<T> : IEqualityComparer<IReadOnlyCollection<T>> //this equalitycomparer takes list order into account
    {
        public virtual bool Equals(IReadOnlyCollection<T> x, IReadOnlyCollection<T> y)
        {
            return x.SequenceEqual(y);
        }

        public virtual int GetHashCode(IReadOnlyCollection<T> obj)
        {
            int hc = 0;
            foreach (var p in obj)
            {
                hc ^= p.GetHashCode();
                hc = (hc << 7) | (hc >> (32 - 7));
            }
            return hc;
        }
    }

    public class ListEquivalentComparer<T>: IEqualityComparer<IReadOnlyCollection<T>> //this equalitycomparer does not take order into account
    {
        public static bool Equals(IReadOnlyCollection<T> x, IReadOnlyCollection<T> y)
        {
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public static int GetHashCode(IReadOnlyCollection<T> obj)
        {
            int hc = 0;
            foreach (var p in obj)
            {
                hc ^= p.GetHashCode();
            }
            return hc;
        }

        bool IEqualityComparer<IReadOnlyCollection<T>>.Equals(IReadOnlyCollection<T> x, IReadOnlyCollection<T> y)
        {
            return Equals(x,y);
        }

        int IEqualityComparer<IReadOnlyCollection<T>>.GetHashCode(IReadOnlyCollection<T> obj)
        {
            return GetHashCode(obj);
        }
    }
}
