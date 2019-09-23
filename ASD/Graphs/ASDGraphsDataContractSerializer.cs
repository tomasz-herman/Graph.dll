using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace ASD.Graphs
{
	/// <summary>Serializator biblioteki Graph</summary>
	/// <remarks>
	/// Serializator zbudowany jest na podstawie <see cref="DataContractSerializer"/> (ale nie jest klasą pochodną)
	/// i należy go używać w analogiczny sposób.<para/>
	/// UWAGA: Poprawnie serializują się jedynie pola delegacyjne skojarzone z metodami statycznymi.
	/// </remarks>
	/// <seealso cref="ASD.Graphs"/>
	public class ASDGraphsDataContractSerializer
	{
		private static readonly Type[] Types;
		private readonly DataContractSerializer _dataContractSerializer;
		
		/// <summary>
		/// Konstruktor obiektu serializatora
		/// </summary>
		/// <param name="type">Typ serializowanego obiektu</param>
		/// <param name="knownTypes"></param>
		/// <remarks>Nie trzeba deklarować jako znane podstawowych typów z biblioteki (i tak są znane).</remarks>
		/// <seealso cref="ASDGraphsDataContractSerializer"/>
		/// <seealso cref="ASD.Graphs"/>
		public ASDGraphsDataContractSerializer(Type type, IEnumerable<Type> knownTypes = null)
		{
			var knownList = new List<Type>(Types);
			if (knownTypes != null)
				foreach (var t in knownTypes)
					if (!knownList.Contains(t))
						knownList.Add(t);
			_dataContractSerializer = new DataContractSerializer(type, knownList, int.MaxValue, false, true, null, new Resolver());
		}

		/// <summary>
		/// Zapis danych (serializacja)
		/// </summary>
		/// <param name="stream">Strumień, na którym zapisywane są dane</param>
		/// <param name="obj">Zapamiętywany obiekt</param>
		/// <seealso cref="ASDGraphsDataContractSerializer"/>
		/// <seealso cref="ASD.Graphs"/>
		public virtual void WriteObject(Stream stream, object obj)
		{
			using (var xmlDictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream))
			{
				_dataContractSerializer.WriteObject(xmlDictionaryWriter, obj);
			}
		}

		/// <summary>
		/// Odczyt danych (deserializacja)
		/// </summary>
		/// <param name="stream">Strumień, z którego odczytywane są dane</param>
		/// <returns>Odtworzony obiekt</returns>
		/// <seealso cref="ASDGraphsDataContractSerializer"/>
		/// <seealso cref="ASD.Graphs"/>
		public virtual object ReadObject(Stream stream)
		{
			object result;
			using (var xmlDictionaryReader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
			{
				result = _dataContractSerializer.ReadObject(xmlDictionaryReader);
			}
			return result;
		}

		static ASDGraphsDataContractSerializer()
		{
			Types = new Type[10];
			Types[0] = typeof(EdgesStack);
			Types[1] = typeof(EdgesQueue);
			Types[2] = typeof(EdgesPriorityQueue);
			Types[3] = typeof(EdgesMaxPriorityQueue);
			Types[4] = typeof(EdgesMinPriorityQueue);
			Types[5] = typeof(SimpleList<int, double>);
			Types[6] = typeof(HashTable<int, double>);
			Types[7] = typeof(AVL<int, double>);
			Types[8] = typeof(Edge);
			Types[9] = typeof(UnionFind);
		}
	}

	internal class Resolver : DataContractResolver
	{
		public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
		{
			var xmlDictionary = new XmlDictionary();
			switch (type.Namespace + "." + type.Name)
			{
				case "ASD.Graphs.AdjacencyMatrixGraph":
					typeName = xmlDictionary.Add("AdjacencyMatrixGraph");
					typeNamespace = xmlDictionary.Add("http://schemas.datacontract.org/2004/07/ASD.Graphs");
					return true;
				case "ASD.Graphs.AdjacencyListsGraph`1":
					typeName = xmlDictionary.Add(type.Name + "[[" + type.GenericTypeArguments[0].Name + "]]");
					typeNamespace = xmlDictionary.Add("http://schemas.datacontract.org/2004/07/ASD.Graphs");
					return true;
				case "ASD.Graphs.SimpleAdjacencyList":
					typeName = xmlDictionary.Add("SimpleAdjacencyList");
					typeNamespace = xmlDictionary.Add("http://schemas.datacontract.org/2004/07/ASD.Graphs");
					return true;
				case "ASD.Graphs.HashTableAdjacencyList":
					typeName = xmlDictionary.Add("HashTableAdjacencyList");
					typeNamespace = xmlDictionary.Add("http://schemas.datacontract.org/2004/07/ASD.Graphs");
					return true;
				case "ASD.Graphs.AVLAdjacencyList":
					typeName = xmlDictionary.Add("AVLAdjacencyList");
					typeNamespace = xmlDictionary.Add("http://schemas.datacontract.org/2004/07/ASD.Graphs");
					return true;
				default:
					return knownTypeResolver.TryResolveType(type, declaredType, knownTypeResolver, out typeName, out typeNamespace);
			}
		}

		public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
		{
			var text = typeName + "::" + typeNamespace;
			var hash = Hash(text);
			if (hash <= 1178924210u)
			{
				if (hash != 837049805u)
				{
					if (hash != 1000958293u)
					{
						if (hash != 1178924210u)
							return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
						if (text == "HashTableAdjacencyList::http://schemas.datacontract.org/2004/07/ASD.Graphs")
							return Type.GetTypeFromHandle(typeof(HashTableAdjacencyList).TypeHandle);
					}
					else if (text == "AdjacencyListsGraph`1[[AVLAdjacencyList]]::http://schemas.datacontract.org/2004/07/ASD.Graphs")
						return typeof(AdjacencyListsGraph<AVLAdjacencyList>);
				}
				else if (text == "AdjacencyMatrixGraph::http://schemas.datacontract.org/2004/07/ASD.Graphs")
					return typeof(AdjacencyMatrixGraph);
			}
			else if (hash <= 1593056421u)
			{
				if (hash != 1575937462u)
				{
					if (hash != 1593056421u)
						return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
					if (text == "AVLAdjacencyList::http://schemas.datacontract.org/2004/07/ASD.Graphs")
						return typeof(AVLAdjacencyList);
				}
				else if (text == "SimpleAdjacencyList::http://schemas.datacontract.org/2004/07/ASD.Graphs")
					return typeof(SimpleAdjacencyList);
			}
			else if (hash != 3073243566u)
			{
				if (hash != 3300024250u)
					return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
				if (text == "AdjacencyListsGraph`1[[HashTableAdjacencyList]]::http://schemas.datacontract.org/2004/07/ASD.Graphs")
					return typeof(AdjacencyListsGraph<HashTableAdjacencyList>);
			}
			else if (text == "AdjacencyListsGraph`1[[SimpleAdjacencyList]]::http://schemas.datacontract.org/2004/07/ASD.Graphs")
				return typeof(AdjacencyListsGraph<SimpleAdjacencyList>);

			return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, knownTypeResolver);
		}

		private static uint Hash(string s)
		{
			return s?.Aggregate(2166136261u, (current, t) => ((uint) t ^ current) * 16777619u) ?? (uint) 0;
		}
	}
}