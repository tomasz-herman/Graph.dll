using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Struktura danych dla problemu Union-Find
    /// </summary>
    /// <remarks>
    /// W C# to oczywiście klasa, a nie struktura.<para/>
    /// To nie jest optymalna wydajnościowo implementacja (ale calkiem dobra !). Implementuje kompresję ścieżek, nie implementuje łączenia według rang.
    /// Implementacja ta jest za to zdecydowanie najprostsza.<para/>
    /// Wymaga aby badane elementy były identyfikowane za pomocą liczb całkowitych 0,1,...,n-1 (dla problemu rozmiaru n).
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class UnionFind
    {
        private readonly int[] _gr;

        public UnionFind(int num)
        {
            _gr = new int[num];
            for (var i = 0; i < num; i++)
            {
                _gr[i] = i;
            }
        }

        public bool Union(int n1, int n2)
        {
            n1 = Find(n1);
            n2 = Find(n2);
            if (n1 == n2)
            {
                return false;
            }
            _gr[n2] = n1;
            return true;
        }

        public int Find(int n)
        {
            if (_gr[_gr[n]] != _gr[n])
            {
                _gr[n] = Find(_gr[n]);
            }
            return _gr[n];
        }

    }
}
