using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Random"/> o wygodne generowanie całkowitych lub zmiennopozycyjnych liczb pseudolosowych z zadanego przedziału
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class RandomExtender
    {
        /// <summary>
        /// Generowanie całkowitych lub zmiennopozycyjnych liczb pseudolosowych z zadanego przedziału
        /// </summary>
        /// <param name="rnd">Wykorzystywany (rozszerzany) generator liczb psuedolosowych</param>
        /// <param name="minValue">Dolna granica przedziału</param>
        /// <param name="maxValue">Górna granica przedziału</param>
        /// <param name="integer">Informacja czy mają być generowane jedynie wartości całkowite</param>
        /// <returns>Wygenerowana liczba pseudolosowa</returns>
        /// <remarks>
        /// Liczby generowane są zgodnie z rozkładem jednostajnym.<para/>
        /// Przedział jest przedziałem domkniętym (zarówno dolna jak i górna granica może być wygenerowana.<para/>
        /// Nie jest sprawdzana sensowność podanych granic przedziału.
        /// </remarks>
        /// <seealso cref="RandomExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static double Next(this Random rnd, double minValue, double maxValue, bool integer)
        {
            if (integer) return rnd.Next((int)(minValue + 0.5), (int)(maxValue + 1.5));
            var number = 2.0 * rnd.NextDouble();//why????
            if (number <= 1.0) return minValue + number * (maxValue - minValue);
            number -= 1.0;
            return minValue + number * (maxValue - minValue);
        }
    }
}
