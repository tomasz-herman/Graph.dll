using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ASD.Graphs
{
    /// <summary>
    /// Klasa pomocnicza do serializacji delegacji
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class DelegateSerializationHelper
    {
        /// <summary>
        /// Serializacja delegacji
        /// </summary>
        /// <param name="delegate">Serializowane pole delegacyjne</param>
        /// <returns>string zawierający zserializowany obiekt</returns>
        /// <seealso cref="DelegateSerializationHelper"/>
        /// <seealso cref="ASD.Graphs"/>
        public static string Serialize(Delegate @delegate)
        {
            if (@delegate == null)
                return null;
            var invocationList = @delegate.GetInvocationList();
            var text = @delegate + "(";
            for (int num = 0; num < invocationList.Length; num++)
            {
                if (num > 0) text += ",";
                string[] array2 = new string[6];
                array2[0] = text;
                array2[1] = invocationList[num].Method.DeclaringType.Namespace;
                array2[2] = ".";
                array2[3] = invocationList[num].Method.DeclaringType.Name;
                array2[4] = ".";
                array2[5] = invocationList[num].Method.Name;
                text = string.Concat(array2);
            }
            return text + ")";
        }

        /// <summary>
        /// Deserializacja delegacji
        /// </summary>
        /// <param name="serializedDelegate">string zawierający zserializowany obiekt</param>
        /// <returns>Odtworzony obiekt</returns>
        /// <seealso cref="DelegateSerializationHelper"/>
        /// <seealso cref="ASD.Graphs"/>
        public static object Deserialize(string serializedDelegate)
        {
            if (serializedDelegate == null)
                return null;
            int num = serializedDelegate.IndexOf('(');
            string string_ = serializedDelegate.Substring(0, num);
            string text = serializedDelegate.Substring(num + 1, serializedDelegate.Length - 2 - num);
            string[] array = text.Split(',');
            Delegate @delegate = null;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (string string_2 in array)
            {
                Delegate b = Method(string_, string_2, assemblies.Where(assembly => !assembly.GlobalAssemblyCache).ToArray());
                @delegate = Delegate.Combine(@delegate, b);
            }
            return @delegate;
        }

        private static Delegate Method(string string0, string string_1, IReadOnlyList<Assembly> assembly_0)
        {
            int num = string_1.LastIndexOf('.');
            string b = string_1.Substring(0, num);
            string method = string_1.Substring(num + 1, string_1.Length - 1 - num);
            for (int i = 0; i < assembly_0.Count; i++)
            {
                Type[] types = assembly_0[i].GetTypes();
                for (int j = 0; j < types.Length; j++)
                {
                    if (types[j].Namespace + "." + types[j].Name == b)
                    {
                        Type type = Type.GetType(string0);
                        Type[] array = types;
                        return Delegate.CreateDelegate(type, array[j], method);
                    }
                }
            }

            return null;
        }
        
    }
}
