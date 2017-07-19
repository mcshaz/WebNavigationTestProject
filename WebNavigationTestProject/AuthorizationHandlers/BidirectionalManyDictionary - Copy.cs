using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebNavigationTestProject.AuthorizationHandlers
{
    public class BidirectionalManyDictionary<TFirst, TSecond>
    {
        public BidirectionalManyDictionary(): this(null, null)
        { }

        public BidirectionalManyDictionary(IEqualityComparer<TFirst> firstComparer) : this(firstComparer, null)
        { }

        public BidirectionalManyDictionary(IEqualityComparer<TSecond> secondComparer) : this(null, secondComparer)
        { }

        public BidirectionalManyDictionary(IEqualityComparer<TFirst> firstComparer, IEqualityComparer<TSecond> secondComparer)
        {
            _firstToSecond = new Dictionary<TFirst, HashSet<TSecond>>(firstComparer ?? EqualityComparer<TFirst>.Default);
            _secondToFirst = new Dictionary<TSecond, HashSet<TFirst>>(secondComparer ?? EqualityComparer<TSecond>.Default);
        }
        /// <summary>
        /// This is a dictionary guaranteed to have only one of each value and key. 
        /// It may be searched either by TFirst or by TSecond, giving a unique answer because it is 1 to 1.
        /// </summary>
        /// <typeparam name="TFirst">The type of the "key"</typeparam>
        /// <typeparam name="TSecond">The type of the "value"</typeparam>

        IDictionary<TFirst, HashSet<TSecond>> _firstToSecond;
        IDictionary<TSecond, HashSet<TFirst>> _secondToFirst;


        /// <summary>
        /// Tries to add the pair to the dictionary.
        /// Throws an exception if either element is already in the dictionary
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        public void Add(TFirst first, IEnumerable<TSecond> second)
        {
            if (_firstToSecond.TryGetValue(first, out HashSet<TSecond> existingTseconds))
            {
                existingTseconds.UnionWith(existingTseconds);
                //exists in first, must exist in second
                _secondToFirst[second].Add(first);
            }
            else
            {
                _firstToSecond.Add(first, new HashSet<TSecond>(new[] { second }));
                _secondToFirst.Add(second, new HashSet<TFirst>(new[] { first }));
            }
        }

        public void AddEmptyFirst(TFirst first)
        {
            if (!_firstToSecond.ContainsKey(first))
            {
                _firstToSecond.Add(first, new HashSet<TSecond>());
            }
        }

        public void AddEmptySecond(TSecond second)
        {
            if (!_secondToFirst.ContainsKey(second))
            {
                _secondToFirst.Add(second, new HashSet<TFirst>());
            }
        }

        /// <summary>
        /// Find the TSecond corresponding to the TFirst first
        /// Throws an exception if first is not in the dictionary.
        /// </summary>
        /// <param name="first">the key to search for</param>
        /// <returns>the value corresponding to first</returns>
        public IEnumerable<TSecond> GetByFirst(TFirst first)
        {
            _firstToSecond.TryGetValue(first, out HashSet<TSecond> second);
            return second;
        }

        /// <summary>
        /// Find the TFirst corresponing to the Second second.
        /// Throws an exception if second is not in the dictionary.
        /// </summary>
        /// <param name="second">the key to search for</param>
        /// <returns>the value corresponding to second</returns>
        public IEnumerable<TFirst> GetBySecond(TSecond second)
        {
            _secondToFirst.TryGetValue(second, out HashSet<TFirst> first);
            return first;
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            _firstToSecond.Clear();
            _secondToFirst.Clear();
        }
    }
}
