using System;
using System.Collections.Generic;

namespace KiwiDb.Util
{
    public static class EnumerableExtensions
    {
        public static Tuple<int, int> EqualRange<T>(this IList<T> list, T value)
        {
            return list.EqualRange(v => v, value);
        }

        public static Tuple<int, int> EqualRange<T>(this IList<T> list, T value, IComparer<T> comparer)
        {
            return list.EqualRange(v => v, value, comparer);
        }

        public static Tuple<int, int> EqualRange<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value)
        {
            return list.EqualRange(convert, value, Comparer<T>.Default);
        }

        public static Tuple<int, int> EqualRange<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value,
                                                             IComparer<T> comparer)
        {
            var lb = list.LowerBound(convert, value, comparer);
            return Tuple.Create(lb, list.UpperBound(convert, value, lb, list.Count, comparer));
        }


        // Returns index of the first element in the sorted range [0,list.Count) which compares greater than value.
        public static int LowerBound<T>(this IList<T> list, T value)
        {
            return list.LowerBound(v => v, value, Comparer<T>.Default);
        }

        public static int LowerBound<T>(this IList<T> list, T value, IComparer<T> comparer)
        {
            return list.LowerBound(v => v, value, comparer);
        }

        public static int LowerBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value)
        {
            return list.LowerBound(convert, value, Comparer<T>.Default);
        }

        public static int LowerBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value,
                                                 IComparer<T> comparer)
        {
            return list.LowerBound(convert, value, 0, list.Count, comparer);
        }

        public static int LowerBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value, int first,
                                                 int last, IComparer<T> comparer)
        {
            var count = last - first;
            while (count > 0)
            {
                var step = count/2;
                var i = first + step;
                if (comparer.Compare(convert(list[i]), value) < 0)
                {
                    first = ++i;
                    count -= step + 1;
                }
                else
                {
                    count = step;
                }
            }
            return first;
        }


        public static int UpperBound<T>(this IList<T> list, T value)
        {
            return list.UpperBound(v => v, value);
        }

        public static int UpperBound<T>(this IList<T> list, T value, IComparer<T> comparer)
        {
            return list.UpperBound(v => v, value, comparer);
        }

        public static int UpperBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value)
        {
            return list.UpperBound(convert, value, Comparer<T>.Default);
        }

        public static int UpperBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value,
                                                 IComparer<T> comparer)
        {
            return list.UpperBound(convert, value, 0, list.Count, comparer);
        }

        public static int UpperBound<TSource, T>(this IList<TSource> list, Func<TSource, T> convert, T value, int first,
                                                 int last, IComparer<T> comparer)
        {
            var count = last - first;
            while (count > 0)
            {
                var step = count/2;
                var i = first + step;
                if (!(comparer.Compare(value, convert(list[i])) < 0))
                {
                    first = ++i;
                    count -= step + 1;
                }
                else
                {
                    count = step;
                }
            }
            return first;
        }

        /*
        template <class ForwardIterator, class T>
          pair<ForwardIterator,ForwardIterator>
            equal_range ( ForwardIterator first, ForwardIterator last, const T& value )
        {
          ForwardIterator it = lower_bound (first,last,value);
          return make_pair ( it, upper_bound(it,last,value) );
        }
        template <class ForwardIterator, class T>
          ForwardIterator lower_bound ( ForwardIterator first, ForwardIterator last, const T& value )
        {
          ForwardIterator it;
          iterator_traits<ForwardIterator>::distance_type count, step;
          count = distance(first,last);
          while (count>0)
          {
            it = first; step=count/2; advance (it,step);
            if (*it<value)                   // or: if (comp(*it,value)), for the comp version
              { first=++it; count-=step+1;  }
            else count=step;
          }
          return first;
        }
        template <class ForwardIterator, class T>
          ForwardIterator upper_bound ( ForwardIterator first, ForwardIterator last, const T& value )
        {
          ForwardIterator it;
          iterator_traits<ForwardIterator>::distance_type count, step;
          count = distance(first,last);
          while (count>0)
          {
            it = first; step=count/2; advance (it,step);
            if (!(value<*it))                 // or: if (!comp(value,*it)), for the comp version
              { first=++it; count-=step+1;  }
            else count=step;
          }
          return first;
        }         
         */


        public static IEnumerable<IList<T>> Chunked<T>(this IEnumerable<T> enumerable, int maxChunkSize)
        {
            List<T> chunk = null;
            foreach (var elem in enumerable)
            {
                if (chunk == null)
                {
                    chunk = new List<T>();
                }
                chunk.Add(elem);

                if (chunk.Count == maxChunkSize)
                {
                    yield return chunk;
                    chunk = null;
                }
            }
            if (chunk != null)
            {
                yield return chunk;
            }
        }

/*
        // Get insertion position of element in ordered list such that ordering in preserved
        public static int LowerBound<T>(this IList<T> list, Func<T, int> comparer)
        {
            var len = list.Count;
            var first = 0;
            while (len > 0)
            {
                var half = len/2;
                var middle = first + half;
                if (comparer(list[middle]) <= 0)
                {
                    len = half;
                }
                else
                {
                    first = middle + 1;
                    len -= (half + 1);
                }
            }
            return first;
        }
 */
    }
}