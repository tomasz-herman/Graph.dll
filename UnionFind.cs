using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Struktura danych dla problemu Union-Find
    /// </summary>
    /// <remarks>
    /// W C# to oczywiście klasa, a nie struktura.<para/>
    /// To nie jest optymalna wydajnościowo implementacja (ale calkiem dobra !). Implementuje kompresję ścieżek,
    /// nie implementuje łączenia według rang.
    /// Implementacja ta jest za to zdecydowanie najprostsza.<para/>
    /// Wymaga aby badane elementy były identyfikowane za pomocą liczb całkowitych 0,1,...,n-1 (dla problemu rozmiaru n).
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class UnionFind
    {
        private readonly int[] _gr;

        /// <summary>
        /// Inicjuje dane
        /// </summary>
        /// <param name="n">Rozmiar problemu</param>
        /// <remarks>Każdy element jest swoim własnym reprezentantem (jest w oddzielnym zbiorze).</remarks>
        /// <seealso cref="UnionFind"/>
        /// <seealso cref="ASD.Graphs"/>
        public UnionFind(int n)
        {
            _gr = new int[n];
            for (var i = 0; i < n; i++)
            {
                _gr[i] = i;
            }
        }

        /// <summary>
        /// Łączy dwa zbiory
        /// </summary>
        /// <param name="n1">Element pierwszego zbioru</param>
        /// <param name="n2">Element drugiego zbioru</param>
        /// <returns>Zwraca true jeśli rzeczywiście dokonano połączenia (parametry reprezentowały rózne zbiory)</returns>
        /// <seealso cref="UnionFind"/>
        /// <seealso cref="ASD.Graphs"/>
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

        /// <summary>
        /// Znajduje reprezentanta zbioru (elementu)
        /// </summary>
        /// <param name="n">Dowolny element</param>
        /// <returns>Reprezentant danego elementu</returns>
        /// <remarks>Wykonuje kompresję ścieżki.</remarks>
        /// <seealso cref="UnionFind"/>
        /// <seealso cref="ASD.Graphs"/>
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
