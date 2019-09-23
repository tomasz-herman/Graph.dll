using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD.Graphs
{
    /// <summary>
    /// Generator grafów losowych
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public class RandomGraphGenerator
    {
        private Random rand;
        
        /// <summary>
        /// Tworzy generator grafów
        /// </summary>
        /// <param name="seed">ziarno początkowe generatora</param>
        /// <remarks>Jeśli seed==null to do inicjowania ziarna generatora wykorzystywany jest czas systemowy.</remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public RandomGraphGenerator(int? seed = null)
        {
            SetSeed(seed);
        }

        /// <summary>
        /// Ustawia ziarno generatora grafów
        /// </summary>
        /// <param name="seed">nowe ziarno początkowe</param>
        /// <remarks>Jeśli seed==null to do inicjowania ziarna generatora wykorzystywany jest czas systemowy.</remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public void SetSeed(int? seed)
        {
            rand = seed == null ? new Random() : new Random(seed.Value);
        }

        /// <summary>
        /// Tworzy nowy graf uzyskany w wyniku losowej permutacji wierzchołków danego grafu
        /// </summary>
        /// <param name="g">Zadany graf</param>
        /// <returns>Wynikowy graf</returns>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph Permute(Graph g)
        {
            var verticesCount = g.VerticesCount;
            var permutation = new int[verticesCount];
            var result = g.IsolatedVerticesGraph();
            var list = new List<int>(verticesCount);
            for (var i = 0; i< verticesCount; i++) list.Add(i);
            for (var i = 0; i < verticesCount; i++)
            {
                var index = rand.Next(verticesCount - i);
                permutation[i] = list[index];
                list.RemoveAt(index);
            }
            for (var i = 0; i < verticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    result.AddEdge(permutation[i], permutation[edge.To], edge.Weight);
            return result;
        }

        /// <summary>
        /// Tworzy ważony graf nieskierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, prawdopodonieństwo istnienia krawędzi pomiędzy
        /// dowolnymi wierzchołkami wynosi density.<para/>
        /// Oczekiwana liczba krawędzi wynosi density*n*(n-1)/2<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph UndirectedGraph(Type tg, int n, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 0 || density < 0.0 || density > 1.0 || minWeight > maxWeight)
                throw new ArgumentException("Invalid random graph generator argument");
            
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }

            var result = Graph.Create(false, n, tg);
            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
                if (rand.NextDouble() < density)
                    result.AddEdge(i, j, rand.Next(minWeight, maxWeight, integer));
            
            return result;
        }

        /// <summary>
        /// Tworzy ważony graf skierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, prawdopodonieństwo istnienia krawędzi pomiędzy
        /// dowolnymi wierzchołkami wynosi density.<para/>
        /// Oczekiwana liczba krawędzi wynosi density*n*(n-1)<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph DirectedGraph(Type tg, int n, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 0 || density < 0.0 || density > 1.0 || minWeight > maxWeight)
                throw new ArgumentException("Invalid random graph generator argument");
            
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }
            
            var result = Graph.Create(true, n, tg);
            
            for (var i = 0; i < n; i++)
            for (var j = 0; j < n; j++)
                if (i != j && rand.NextDouble() < density)
                    result.AddEdge(i, j, rand.Next(minWeight, maxWeight, integer));
            
            return result;
        }

        /// <summary>
        /// Tworzy rzadki ważony graf nieskierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="minDegree">Minimalny stopień wierzchołków</param>
        /// <param name="maxDegree">Maksymalny stopień wierzchołków</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, liczba krawędzi wynosi (n*minDegree)/2 + (n*(maxDegree-minDegree)+3)/4.<para/>
        /// Dla grafów o nieparzystej liczbie wierzchołków, dla których minDegree==maxDegree
        /// i są one nieparzyste dla jednego wierzchołka minimalny stopień nie
        /// jest osiągnięty (nie da się skonstruować odpowiedniego grafu).<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według
        /// rozkładu jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph SparseUndirectedGraph(Type tg, int n, int minDegree, int maxDegree, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            return SparseGraphGeneral(tg, false, n, minDegree, maxDegree, minWeight, maxWeight, integer);
        }

        /// <summary>
        /// Tworzy rzadki ważony graf skierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="minDegree">Minimalny stopień wierzchołków</param>
        /// <param name="maxDegree">Maksymalny stopień wierzchołków</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, liczba krawędzi wynosi n*minDegree + (n*(maxDegree-minDegree))/2.<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według
        /// rozkładu jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph SparseDirectedGraph(Type tg, int n, int minDegree, int maxDegree, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            return SparseGraphGeneral(tg, true, n, minDegree, maxDegree, minWeight, maxWeight, integer);
        }

        /// <summary>
        /// Tworzy ważony skierowany graf acykliczny (DAG)
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Prawdopodonieństwo istnienia krawędzi pomiędzy dowolnymi wierzchołkami wynosi density.<para/>
        /// Oczekiwana liczba krawędzi wynosi density*n*(n-1)/2<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według
        /// rozkładu jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph DAG(Type tg, int n, double density, double minWeight, double maxWeight, bool integer = true)
        {
            if (n < 0 || density < 0.0 || density > 1.0 || minWeight > maxWeight)
                throw new ArgumentException("Invalid random graph generator argument");
            
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }
            
            var result = Graph.Create(true, n, tg);
            for (var i = 0; i < n; i++)
            for (var j = i + 1; j < n; j++)
                if (rand.NextDouble() < density)
                    result.AddEdge(i, j, rand.Next(minWeight, maxWeight, integer));
            
            return Permute(result);
        }

        /// <summary>
        /// Tworzy nieskierowany graf euklidesowy
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minx">Dolna granica wartości współrzędnej x</param>
        /// <param name="maxx">Górna granica wartości współrzędnej x</param>
        /// <param name="miny">Dolna granica wartości współrzędnej y</param>
        /// <param name="maxy">Górna granica wartości współrzędnej y</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, prawdopodonieństwo istnienia krawędzi
        /// pomiędzy dowolnymi wierzchołkami wynosi density.<para/>
        /// Oczekiwana liczba krawędzi wynosi density*n*(n-1)/2<para/>
        /// Wagi krawędzi odpowiadają odległościom euklidesowym między wierzchołkami
        /// rozmieszczonymi losowo na płaszczyznie (dla parametru integer==true zaokrąglonym
        /// w górę do wartości całkowitej), rozkład punktów jednostajny na obszarze [minx,maxx]*[miny,maxy].<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph UndirectedEuclidGraph(Type tg, int n, double density, double minx, double maxx, double miny, double maxy, bool integer = true)
        {
            if (n < 0 || density < 0.0 || density > 1.0 || !(minx < maxx) || !(miny < maxy))
                throw new ArgumentException("Invalid random graph generator argument");
            
            var axisX = new double[n];
            var axisY = new double[n];
            
            var result = Graph.Create(false, n, tg);
            
            for (var i = 0; i < n; i++)
            {
                axisX[i] = rand.NextDouble() * (maxx - minx) + minx;
                axisY[i] = rand.NextDouble() * (maxy - miny) + miny;
            }
            for (var j = 0; j < n; j++)
            for (var k = j + 1; k < n; k++)
            {
                if (!(rand.NextDouble() < density)) continue;
                var weight = Math.Sqrt((axisX[j] - axisX[k]) * (axisX[j] - axisX[k]) +
                                       (axisY[j] - axisY[k]) * (axisY[j] - axisY[k]));
                if (integer) weight = Math.Ceiling(weight);
                result.AddEdge(j, k, weight);
            }

            return result;
        }

        /// <summary>
        /// Tworzy nieskierowany graf euklidesowego sąsiedztwa
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minx">Dolna granica wartości współrzędnej x</param>
        /// <param name="maxx">Górna granica wartości współrzędnej x</param>
        /// <param name="miny">Dolna granica wartości współrzędnej y</param>
        /// <param name="maxy">Górna granica wartości współrzędnej y</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Graf tworzony jest według nastepujących reguł:<para/>
        /// Wierzchołki rozmieszczane są losowo na płaszczyznie,
        /// rozkład punktów jednostajny na obszarze [minx,maxx]*[miny,maxy].<para/>
        /// Przez maxd oznaczmy odległość euklidesową punktu (minx,miny) od punktu (maxx,maxy).<para/>
        /// Wierzchołki połączone są krawędzią wtedy i tylko wtedy gdy są różne (pętle wykluczamy)
        /// i ich euklidesowa odległość jest mniejsza lub równa d*maxd.<para/>
        /// Wagi krawędzi odpowiadają odległościom euklidesowym między
        /// wierzchołkami zaokrąglonym w górę do wartości całkowitej.<para/>
        /// Wagi krawędzi odpowiadają odległościom euklidesowym między wierzchołkami
        /// (dla parametru integer==true zaokrąglonym w górę do wartości całkowitej).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Współczynnik d musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph UndirectedEuclidNeighbourhoodGraph(Type tg, int n, double density, double minx, double maxx, double miny, double maxy, bool integer = true)
        {
            if (n < 0 || !(density >= 0.0) || !(density <= 1.0) || !(minx < maxx) || !(miny < maxy))
                throw new ArgumentException("Invalid random graph generator argument");
            
            var axisX = new double[n];
            var axisY = new double[n];
            var maximum = density * Math.Sqrt((maxx - minx) * (maxx - minx) + (maxy - miny) * (maxy - miny));
            
            var result = Graph.Create(false, n, tg);
            
            for (var i = 0; i < n; i++)
            {
                axisX[i] = rand.NextDouble() * (maxx - minx) + minx;
                axisY[i] = rand.NextDouble() * (maxy - miny) + miny;
            }
            for (var j = 0; j < n; j++)
            for (var k = j + 1; k < n; k++)
            {
                var weight = Math.Sqrt((axisX[j] - axisX[k]) * (axisX[j] - axisX[k]) +
                                       (axisY[j] - axisY[k]) * (axisY[j] - axisY[k]));
                if (integer) weight = Math.Ceiling(weight);
                if (weight <= maximum) result.AddEdge(j, k, weight);
            }

            return result;
        }

        /// <summary>
        /// Tworzy ważony graf dwudzielny
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków w pierwszym podzbiorze</param>
        /// <param name="m">Liczba wierzchołków w drugim podzbiorze</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf ma n+m wierzchołków, jest to nieskierowany graf (n,m)-dwudzielny.<para/>
        /// Prawdopodonieństwo istnienia krawędzi pomiędzy wierzchołkami z różnych podzbiorów wynosi density.<para/>
        /// Oczekiwana liczba krawędzi wynosi density*n*m/2<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph BipariteGraph(Type tg, int n, int m, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 0 || m < 0 || !(density >= 0.0) || !(density <= 1.0) || !(minWeight <= maxWeight))
                throw new ArgumentException("Invalid random graph generator argument");
            
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }
            
            var result = Graph.Create(false, n + m, tg);
            
            for (var i = 0; i < n; i++)
            for (var j = n; j < n + m; j++)
                if (rand.NextDouble() < density)
                    result.AddEdge(i, j, rand.Next(minWeight, maxWeight, integer));
            
            return Permute(result);
        }

        /// <summary>
        /// Tworzy ważony graf eulerowski
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość krawędzi (parametr density) określona jest jedynie w
        /// sposób przybliżony (wartość parametru musi być z przedziału [0,1]).
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph EulerGraph(Type tg, bool directed, int n, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            return GeneralEulerGraph(tg, directed, false, n, density, minWeight, maxWeight, integer);
        }

        /// <summary>
        /// Tworzy ważony graf półeulerowski
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość krawędzi (parametr density) określona jest jedynie w
        /// sposób przybliżony (wartość parametru musi być z przedziału [0,1]).
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph SemiEulerGraph(Type tg, bool directed, int n, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            return GeneralEulerGraph(tg, directed, true, n, density, minWeight, maxWeight, integer);
        }

        /// <summary>
        /// Tworzy ważone drzewo nieskierowane
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="density">Prawdopodobieństwo istnienia krawędzi</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf jest drzewem (lasem).<para/>
        /// Oczekiwana liczba krawędzi wynosi density*(n-1)<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.<para/>
        /// Gęstość density musi być z przedziału [0,1].
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph TreeGraph(Type tg, int n, double density, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 0 || !(density >= 0.0) || !(density <= 1.0) || !(minWeight <= maxWeight))
                throw new ArgumentException("Invalid random graph generator argument");
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }
            
            var result = Graph.Create(false, n, tg);
            var hashSet = new HashSet<HashSet<int>>();
            for (var i = 0; i < n; i++)
            {
                hashSet.Add(new HashSet<int>
                {
                    i
                });
            }
            for (var j = 1; j < n; j++)
            {
                if (!(rand.NextDouble() < density)) continue;
                
                var index = rand.Next(hashSet.Count);
                var i = rand.Next(hashSet.Count - 1);
                
                if (i >= index) i++;
                
                var from = hashSet.ElementAt(index).ElementAt(rand.Next(hashSet.ElementAt(index).Count));
                var to = hashSet.ElementAt(i).ElementAt(rand.Next(hashSet.ElementAt(i).Count));
                
                result.AddEdge(from, to, rand.Next(minWeight, maxWeight, integer));
                
                hashSet.ElementAt(index).Union(hashSet.ElementAt(i));
                hashSet.ElementAt(i).Clear();
                
                hashSet.RemoveWhere(set => set.Count == 0);
            }
            return result;
        }

        /// <summary>
        /// Tworzy ważony cykl nieskierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Stworzony graf jest n-elementowym cyklem nieskierowanym (kolejność wierzchołków w cyklu losowa).<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph UndirectedCycle(Type tg, int n, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 2 || minWeight > maxWeight)
            {
                throw new ArgumentException("Invalid random graph generator argument");
            }
            if (integer && (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight)))
            {
                throw new ArgumentException("Invalid random graph generator argument");
            }
            var result = Graph.Create(false, n, tg);
            for (var i = 1; i < n; i++) 
                result.AddEdge(i - 1, i, rand.Next(minWeight, maxWeight, integer));
            result.AddEdge(n - 1, 0, rand.Next(minWeight, maxWeight, integer));
            return Permute(result);
        }

        /// <summary>
        /// Tworzy ważony cykl skierowany
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Stworzony graf jest n-elementowym cyklem skierowanym (kolejność wierzchołków w cyklu losowa).<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu
        /// jednostajnego z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph DirectedCycle(Type tg, int n, double minWeight = 1.0, double maxWeight = 1.0, bool integer = true)
        {
            if (n < 2 || minWeight > maxWeight)
            {
                throw new ArgumentException("Invalid random graph generator argument");
            }
            if (integer && (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight)))
            {
                throw new ArgumentException("Invalid random graph generator argument");
            }
            var result = Graph.Create(true, n, tg);
            for (var i = 1; i < n; i++) 
                result.AddEdge(i - 1, i, rand.Next(minWeight, maxWeight, integer));
            result.AddEdge(n - 1, 0, rand.Next(minWeight, maxWeight, integer));
            return Permute(result);
        }

        /// <summary>
        /// Tworzy graf będący siecią przepływową ze źródłem i ujściem
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="nn">Oczekiwana liczba krawędzi "w stronę ujścia" z każdego z wierzchołków</param>
        /// <param name="d">Prawdopodobieństwo istnienia krawędzi "w stronę źródła</param>
        /// <param name="capacity">Typowa przepustowość krawędzi</param>
        /// <param name="twoWay">Informacja czy dozwolone są krawędzie w obu kierunkach</param>
        /// <returns>Krotka (network, source, target) składająca się z utworzonej sieci oraz jej źródła i ujścia</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Sieć tworzona jest według nastepujących reguł:<para/>
        /// Wierzchołki rozmieszczane są zgodnie z rozkładem jednostajnym na przedziale [0,1].<para/>
        /// Jako źródło przyjmowany jest wierzchołek o największej współrzędnej,
        /// a jako ujście wierzchołek o najmniejszej współrzędnej.<para/>
        /// Krawędziami połączone są wierzchołki odległe o nie więcej niż nn/n.<para/>
        /// Jeśli wierzchołki spełniają tą zależność to krawędź "w stronę ujścia" wprowadzana jest zawsze,
        /// a krawędź "w stronę źródła" z prawdopodonieństwem d.<para/>
        /// Wagi krawędzi generowane są odrębnie (zawsze generowane są wartości całkowite).<para/>
        /// Współczynnik d musi być z przedziału [0,1].<para/>
        /// Jeśli generowana sieć ma być wykorzystywana jako sieć z kosztami
        /// to parametr twoWay musi mieć wartość false (w tym przypadku z prawdopodonieństwem
        /// d istnieje jedynie krawędź "w stronę źródła").
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public (Graph network, int source, int target) FlowNetwork(Type tg, int n, int nn, double d, int capacity, bool twoWay = true)
        {
            if (n < 2 || nn < 1 || nn > n || capacity < 1 || !(d >= 0.0) || !(d <= 1.0))
                throw new ArgumentException("Invalid random graph generator argument");
            
            var source = -1;
            var target = -1;
            var maxDistance = nn / (double)n;
            var distribution = new double[n];
            var capacities = new int[n];
            var max = 0.0;
            var min = 1.0;
            
            var result = Graph.Create(true, n, tg);
            for (var i = 0; i < n; i++)
            {
                capacities[i] = rand.Next(capacity) + 1;
                distribution[i] = rand.NextDouble();
                if (distribution[i] > max)
                {
                    max = distribution[i];
                    source = i;
                }
                if (distribution[i] < min)
                {
                    min = distribution[i];
                    target = i;
                }
            }
            capacities[source] += capacity;
            capacities[target] += capacity;
            for (var i = 0; i < n; i++)
            {
                for (var j = i + 1; j < n; j++)
                {
                    var distance = Math.Abs(distribution[i] - distribution[j]);
                    if (i == source || j == source || i == target || j == target) 
                        distance /= 2.0;

                    if (!(distance <= maxDistance)) continue;

                    var weight = 1 + rand.Next(capacities[i]) + rand.Next(capacities[j]) + rand.Next((int)((1.0 - distance) * capacity));
                    
                    if (distribution[i] > distribution[j])
                    {
                        result.AddEdge(i, j, weight);
                    }
                    if (distribution[j] > distribution[i])
                    {
                        result.AddEdge(j, i, weight);
                    }
                    
                    if (!(rand.NextDouble() < d)) continue;
                        
                    weight = 1 + (rand.Next(capacities[i]) + rand.Next(capacities[j]) + rand.Next((int)((1.0 - distance) * capacity))) / 2;
                    
                    if (distribution[i] > distribution[j])
                    {
                        if (!twoWay)
                        {
                            result.DelEdge(i, j);
                        }
                        result.AddEdge(j, i, weight);
                    }
                    if (distribution[j] > distribution[i])
                    {
                        if (!twoWay)
                        {
                            result.DelEdge(j, i);
                        }
                        result.AddEdge(i, j, weight);
                    }
                }
            }
            return (result, source, target);
        }

        /// <summary>
        /// Tworzy graf kosztów
        /// </summary>
        /// <param name="g">Graf bazowy</param>
        /// <param name="minCost">Dolna granica wagi krawędzi (kosztu)</param>
        /// <param name="maxCost">Górna granica wagi krawędzi (kosztu)</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Utworzony graf ma identyczną strukturę i identyczną reprezentację jak graf bazowy.<para/>
        /// Wagi krawędzi (koszty) są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu jednostajnego
        /// z zakresu minCost..maxCost (włącznie).<para/>
        /// Dla parametru integer==true parametry minCost i maxCost muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph CostGraph(Graph g, double minCost, double maxCost, bool integer = true)
        {
            if (g == null || !g.Directed || minCost > maxCost)
                throw new ArgumentException("Invalid random graph generator argument");
            if (integer && (minCost != Math.Round(minCost) || maxCost != Math.Round(maxCost)))
                throw new ArgumentException("Invalid random graph generator argument");
            var result = g.IsolatedVerticesGraph();
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    result.AddEdge(edge.From, edge.To, rand.Next(minCost, maxCost, integer));
            return result;
        }

        /// <summary>
        /// Metoda do tworzenia wszelkiego rodzaju grafów rzadkich
        /// </summary>
        /// <param name="tg">Sposób reprezentacji grafu</param>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="n">Liczba wierzchołków</param>
        /// <param name="minDegree">Minimalny stopień wierzchołków</param>
        /// <param name="maxDegree">Maksymalny stopień wierzchołków</param>
        /// <param name="minWeight">Dolna granica wagi krawędzi</param>
        /// <param name="maxWeight">Górna granica wagi krawędzi</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Utworzony graf</returns>
        /// <exception cref="ArgumentException">Gdy argumenty funkcji są nieprawidłowe</exception>
        /// <remarks>
        /// Tworzony graf nie zawiera pętli, liczba krawędzi wynosi n*minDegree + (n*(maxDegree-minDegree))/2
        /// dla grafów skierowanych (n*minDegree)/2 + (n*(maxDegree-minDegree)+3)/4 dla grafów nieskierowanych.<para/>
        /// Dla grafów nieskierowanych o nieparzystej liczbie wierzchołków, dla których minDegree==maxDegree
        /// i są one nieparzyste dla jednego wierzchołka mminimalny stopień nie jest osiągnięty
        /// (nie da się skonstruować odpowiedniego grafu).<para/>
        /// Wagi krawędzi są losowymi liczbami całkowitymi lub zmiennopozycyjnymi
        /// (w zależności od wartości parametru integer) wygenerowanymi według rozkładu jednostajnego
        /// z zakresu minWeight..maxWeight (włącznie).<para/>
        /// Dla parametru integer==true parametry minWeight i maxWeight muszą mieć wartości całkowite.
        /// </remarks>
        /// <seealso cref="RandomGraphGenerator"/>
        /// <seealso cref="ASD.Graphs"/>
        public Graph SparseGraphGeneral(Type tg, bool directed, int n, int minDegree, int maxDegree, double minWeight, double maxWeight, bool integer = true)
        {
            if (n < 0) throw new ArgumentException("Invalid random graph generator argument");
            if (minDegree > maxDegree || maxDegree >= n || !(minWeight <= maxWeight))
                throw new ArgumentException("Invalid random graph generator argument");
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                {
                    throw new ArgumentException("Invalid random graph generator argument");
                }
            }
            var result = Graph.Create(directed, n, tg);
            var list0 = new List<int>(n);
            var list1 = new List<int>(n);
            var list2 = new List<int>();
            var list = new List<int>();
            for (var i = 0; i < n; i++)
            {
                list0.Clear();
                for (var j = 0; j < n; j++)
                    if (result.InDegree(j) < minDegree && j != i)
                        list0.Add(j);
                
                while (result.OutDegree(i) < minDegree)
                {
                    if (list0.Count == 0)
                        break;
                    var index = rand.Next(list0.Count);
                    result.AddEdge(i, list0[index], rand.Next(minWeight, maxWeight, integer));
                    list0.RemoveAt(index);
                }

                if (result.OutDegree(i) == minDegree) continue;
                {
                    list0.AddRange(result.OutEdges(i).Select(edge => edge.To));

                    list2.Clear();
                    
                    if (directed)
                        for (var j = 0; j < n; j++)
                        {
                            if (result.InDegree(j) < minDegree) list2.Add(j);
                        }
                    else
                        for (var j = 0; j < n; j++)
                            if ((result.InDegree(j) < minDegree - 1) || (result.InDegree(j) < minDegree && j != i))
                                list2.Add(j);

                    while (result.OutDegree(i) < minDegree)
                    {
                        if (!directed && result.EdgesCount >= n * minDegree / 2)
                            break;
                        var index3 = rand.Next(list2.Count);
                        list1.Clear();
                        
                        for (var j = 0; j < i; j++)
                            if (j != list2[index3])
                                list1.Add(j);
                        
                        var flag = true;
                        while (flag && list1.Count > 0)
                        {
                            var index2 = rand.Next(list1.Count);
                            flag = false;
                            list.Clear();
                            foreach (var edge in result.OutEdges(list1[index2]))
                            {
                                if (edge.To == list2[index3])
                                {
                                    flag = true;
                                    break;
                                }
                                if (!list0.Contains(edge.To) && edge.To != i)
                                {
                                    list.Add(edge.To);
                                }
                            }
                            if (flag)
                            {
                                list1.RemoveAt(index2);
                                continue;
                            }
                            var index = rand.Next(list.Count);
                            result.DelEdge(list1[index2], list[index]);
                            result.AddEdge(i, list[index], rand.Next(minWeight, maxWeight, integer));
                            result.AddEdge(list1[index2], list2[index3], rand.Next(minWeight, maxWeight, integer));
                            list0.Add(list[index]);
                            if (result.InDegree(list2[index3]) == minDegree) 
                                list2.RemoveAt(index3);
                        }
                    }
                }
            }
            
            var xD = !directed
                ? (n * (maxDegree - minDegree) + 3) / 4
                : (n * (maxDegree - minDegree)) / 2;
            
            void SomeMethod(int degree)
            {
                list0.Clear();
                for (var i = 0; i < n; i++)
                {
                    if (result.OutDegree(i) < degree)
                    {
                        list0.Add(i);
                    }
                }
                while (list0.Count > 0 && xD > 0)
                {
                    var index = rand.Next(list0.Count);
                    list1.Clear();
                    
                    foreach (var edge in result.OutEdges(list0[index]))
                        list1.Add(edge.To);
                    
                    list2.Clear();
                    for (var j = 0; j < n; j++)
                    {
                        if (result.InDegree(j) >= maxDegree || list1.Contains(j)) continue;
                        if (j != list0[index])
                        {
                            list2.Add(j);
                        }
                    }

                    if (list2.Count == 0)
                        list0.RemoveAt(index);
                    else
                    {
                        var index2 = rand.Next(list2.Count);
                        result.AddEdge(list0[index], list2[index2], rand.Next(minWeight, maxWeight, integer));
                        if (result.OutDegree(list0[index]) == maxDegree)
                            list0.RemoveAt(index);
                        if (result.OutDegree(list2[index2]) == maxDegree)
                            list0.Remove(list2[index2]);
                        xD--;
                    }
                }
            }
            
            if (!directed && (n * minDegree & 1) == 1)
            {
                SomeMethod(minDegree);
            }
            SomeMethod(maxDegree);
            return result;
        }

        private Graph GeneralEulerGraph(Type tg, bool directed, bool semi, int n, double density, double minWeight, double maxWeight, bool integer = true)
        {
            if (n < 2 || density < 0.0 || density > 1.0 || minWeight > maxWeight)
            {
                throw new ArgumentException("Invalid random graph generator argument");
            }
            if (integer)
            {
                if (minWeight != Math.Round(minWeight) || maxWeight != Math.Round(maxWeight))
                    throw new ArgumentException("Invalid random graph generator argument");
            }
            if (n == 2 && !directed && !semi)
            {
                throw new ArgumentException("There is no undirected Euler graph with 2 vertices");
            }
            
            var result = Graph.Create(directed, n, tg);
            var connections = new List<int>[n];
            var vertices = new List<int>();
            var processed = new bool[n];
            var edgesCount = !directed ? 0.5 * n * (n - 1 & -2) * density : n * (n - 1) * density;
            edgesCount = (int)(edgesCount + (semi ? 0.5 : -0.5));
            
            var edge = new Edge(-1, -1, -1.0);
            for (var i = 0; i < n; i++)
            {
                vertices.Add(i);
                connections[i] = new List<int>();
                for (var j = 0; j < n; j++)
                {
                    if (j == i) continue;
                    connections[i].Add(j);
                }
            }
            
            int start;
            var from = (start = rand.Next(n));
            
            processed[start] = true;
            
            vertices.Remove(start);
            
            var running = true;
            while (running)
            {
                int to;
                if (result.EdgesCount >= edgesCount - vertices.Count)
                {
                    if (vertices.Count > 0)
                    {
                        to = GetRandom(vertices);
                        connections[from].Remove(to);
                        processed[to] = true;
                        if (!directed) connections[to].Remove(from);
                    }
                    else if (!semi)
                    {
                        if (from == start)
                            break;
                        if (connections[from].Contains(start))
                        {
                            to = start;
                            running = false;
                        }
                        else
                        {
                            to = GetRandom(connections[from]);
                            if (!directed) connections[to].Remove(from);
                        }
                    }
                    else
                    {
                        if (from == start) result.DelEdge(edge);
                        break;
                    }
                }
                else
                {
                    if (connections[from].Count == 0)
                        break;
                    to = GetRandom(connections[from]);
                    if (!directed) connections[to].Remove(from);
                    if (!processed[to])
                    {
                        vertices.Remove(to);
                        processed[to] = true;
                    }
                }
                
                edge = new Edge(from, to, rand.Next(minWeight, maxWeight, integer));
                result.AddEdge(edge);
                if ((n & 1) == 0 && (!semi || result.EdgesCount < edgesCount - 1))
                {
                    NotSureWhatItDoesButItHasSomethingToDoWithEulerGraphGeneration(connections, from);
                    NotSureWhatItDoesButItHasSomethingToDoWithEulerGraphGeneration(connections, to);
                }
                
                from = to;
            }
            return result;
        }

        private int GetRandom(IList<int> list)
        {
            var index = rand.Next(list.Count);
            var result = list[index];
            list.RemoveAt(index);
            return result;
        }

        private void NotSureWhatItDoesButItHasSomethingToDoWithEulerGraphGeneration(IReadOnlyList<List<int>> listArray, int listIndex)
        {
            while (listArray[listIndex].Count == 1)
            {
                var num = listArray[listIndex][0];
                listArray[listIndex].Clear();
                listIndex = num;
            }
        }
        
    }
}
