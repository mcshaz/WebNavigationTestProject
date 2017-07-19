using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class LookupDistinct<TKey, TElement>
    {
        public LookupDistinct(): this(null, null)
        { }

        public LookupDistinct(IEqualityComparer<TKey> keyComparer) : this(keyComparer, null)
        { }

        public LookupDistinct(IEqualityComparer<TElement> elementComparer) : this(null, elementComparer)
        { }

        public LookupDistinct(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TElement> elementComparer)
        {
            _elementComparer = elementComparer ?? EqualityComparer<TElement>.Default;
            Sets = new Dictionary<TKey, HashSet<TElement>>(keyComparer ?? EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// This is a dictionary guaranteed to have only one of each value and key. 
        /// </summary>
        /// <typeparam name="TFirst">The type of the "key"</typeparam>
        /// <typeparam name="TSecond">The type of the "value"</typeparam>
        public IDictionary<TKey, HashSet<TElement>> Sets { get; private set; }
        IEqualityComparer<TElement> _elementComparer;


        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Throws an exception if either element is already in the dictionary
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void Add(TKey key, IEnumerable<TElement> elements)
        {
            if (Sets.TryGetValue(key, out HashSet<TElement> existingSet))
            {
                existingSet.UnionWith(elements);
                //exists in first, must exist in second
            }
            else
            {
                Sets.Add(key, new HashSet<TElement>(elements, _elementComparer));
            }
        }

        public void Add(TKey key, TElement element)
        {
            if (!Sets.TryGetValue(key, out HashSet<TElement> set))
            {
                set = new HashSet<TElement>(_elementComparer);
                Sets.Add(key, set);
            }
            set.Add(element);
        }
    }
}
