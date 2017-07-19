using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class IListEqualsComparer<T> : IEqualityComparer<ICollection<T>> //this equalitycomparer takes list order into account
    {
        public virtual bool Equals(ICollection<T> x, ICollection<T> y)
        {
            return x.SequenceEqual(y);
        }

        public virtual int GetHashCode(ICollection<T> obj)
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

    public static class ListEquivalentComparer//: IEqualityComparer<IReadOnlyCollection<T>> //this equalitycomparer does not take order into account
    {
        public static bool Equals<T>(ICollection<T> x, IReadOnlyCollection<T> y)
        {
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public static int GetHashCode<T>(ICollection<T> obj)
        {
            int hc = 0;
            foreach (var p in obj)
            {
                hc ^= p.GetHashCode();
            }
            return hc;
        }
    }
}
