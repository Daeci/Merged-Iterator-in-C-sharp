using System.Collections;
using System.Diagnostics;

namespace MergedIterator
{
    class Program
    {
        public class MergedIterator : IEnumerator<int>
        {
            private readonly List<IEnumerator<int>> IteratorsList;
            private Dictionary<int, bool> finishedIteratorDict = new Dictionary<int, bool>(); // int: iterator list index, bool: has reached end of collection

            public MergedIterator(List<IEnumerator<int>> iterators)
            {
                IteratorsList = iterators;
                for (int i = 0; i < IteratorsList.Count; ++i)
                {
                    /*
                     * Interesting quirk I've noticed about List<int> to IEnumerator<int> in C#:
                     * The IEnumerator.Current value is not set to the first item of the list, but seemingly the item before so it defaults to 0,
                     * so in order to maintain the expected output each iterator must have MoveNext() called to set it to the actual first
                     * element. Also, an empty List<int> initialized as [] will also have this issue that Current will return 0 instead of 
                     * nothing/undefined, so it must be checked for its MoveNext() returning false so that we can mark that iteratoras complete
                     * in the dictionary.
                     *
                     * Example output without doing initial MoveNext() check & dict setting: [0, 0, 0, 0, 0, 1, 2, 2, 3, 4, 4, 5, 6, 8, 10, 100, 1000]
                     * Example output with doing intiial MoveNext() check & dict setting:    [0, 1, 2, 2, 3, 4, 4, 5, 6, 8, 10, 100, 1000]
                     */
                    finishedIteratorDict.Add(i, !IteratorsList[i].MoveNext());
                }
            }

            public int Current
            {
                get
                {
                    int indexer = 0;
                    int iteratorToMove = -1;
                    int minValue = int.MaxValue;
                    foreach (IEnumerator<int> iter in IteratorsList)
                    {
                        if (finishedIteratorDict[indexer] == false)
                        {
                            if (iter.Current < minValue)
                            {
                                minValue = iter.Current;
                                iteratorToMove = indexer;
                            }
                        }
                        ++indexer;
                    }

                    if (!IteratorsList[iteratorToMove].MoveNext())
                    {
                        finishedIteratorDict[iteratorToMove] = true;
                    }

                    return minValue;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool MoveNext()
            {
                // checking that there is an iterator that has not reached the end of its collection
                return finishedIteratorDict.ContainsValue(false);
            }

            public void Reset()
            {
                for (int i = 0; i < IteratorsList.Count; ++i)
                {
                    IteratorsList[i].Reset();
                    finishedIteratorDict[i] = false;
                }

                // optional
                // throw new NotSupportedException();
            }

        }

        static void Main(string[] args)
        {
            List<List<int>> numLists =
            [
                [1, 2, 3, 4, 5],
                [],
                [0, 2, 4, 6, 8],
                [10, 100, 1000],
            ];

            List<IEnumerator<int>> iterators = numLists.Select(x => (IEnumerator<int>)x.GetEnumerator()).ToList();

            MergedIterator merged = new MergedIterator(iterators);

            string result = "[";
            while (merged.MoveNext())
            {
                result += $"{merged.Current}, ";
            }
            result = result.Substring(0, result.Length - 2) + "]";

            // Equivalency test:
            string check = "[0, 1, 2, 2, 3, 4, 4, 5, 6, 8, 10, 100, 1000]";
            Debug.Assert(check.Equals(result));
        }

    }
}