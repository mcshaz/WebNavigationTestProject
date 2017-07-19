using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public static class DictionaryExtensions
    {

        public static void AddTo<TKey, TElement>(this IDictionary<TKey,List<TElement>> dict,TKey key, IEnumerable<TElement> elements)
        {
            if (dict.TryGetValue(key, out List<TElement> existingList))
            {
                existingList.AddRange(elements);
            }
            else
            {
                dict.Add(key, new List<TElement>(elements));
            }
        }

        public static void AddTo<TKey, TElement>(this IDictionary<TKey, List<TElement>> dict, TKey key, TElement element)
        {
            if (!dict.TryGetValue(key, out List<TElement> set))
            {
                set = new List<TElement>();
                dict.Add(key, set);
            }
            set.Add(element);
        }
    }
}
