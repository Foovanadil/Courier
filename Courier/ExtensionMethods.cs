using System;
using System.Collections.Generic;
using System.Linq;

namespace Courier
{
	internal static class ExtensionMethods
    {

        //Reimplement a few List<T> methods that are missing in silverlight so that I can avoid branching the core library to support Silverlight
        internal static List<T> FindAllSL<T>(this List<T> source, Predicate<T> match)
        {
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            var list = new List<T>();
            list.AddRange((from T t in source where match(t) select t).Select(t => (T) t));
            return list;
        }

		internal static Int32 RemoveAllSL<T>(this List<T> input, Predicate<T> match)
        {
            int removed = 0;

            for (int i = input.Count - 1; i >= 0; i--)
            {
                if (match(input[i]))
                {
                    input.RemoveAt(i);
                    removed++;
                }
            }
            return removed;

        }


    }
}
