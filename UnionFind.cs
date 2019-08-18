using System;

namespace ASD.Graphs
{
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
