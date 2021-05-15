# Graph.dll deobfuscated
**Graf** – podstawowy obiekt rozważań [teorii grafów](https://pl.wikipedia.org/wiki/Teoria_graf%C3%B3w "Teoria grafów"), [struktura matematyczna](https://pl.wikipedia.org/wiki/Struktura_matematyczna "Struktura matematyczna") służąca do przedstawiania i badania relacji między obiektami. W uproszczeniu graf to zbiór [wierzchołków](https://pl.wikipedia.org/wiki/Wierzcho%C5%82ek_(teoria_graf%C3%B3w) "Wierzchołek (teoria grafów)"), które mogą być połączone [krawędziami](https://pl.wikipedia.org/wiki/Kraw%C4%99d%C5%BA_grafu "Krawędź grafu") w taki sposób, że każda krawędź kończy się i zaczyna w którymś z wierzchołków.

W świecie informatyki graf jest abstrakcyjnym typem danych, który implementuje graf skierowany lub nieskierowany. Niniejsza biblioteka umożliwia zapis grafu za pomocą listy lub macierzy sąsiedztwa oraz późniejszą jego obróbkę za pomocą licznych algorytmów takich jak:
- [Depth First Search](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/DFSGraphExtender.cs#L53 "DFSGraphExtender.cs")
- [Breadth First Search](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/GeneralSearchGraphExtender.cs#L60 "GeneralSearchGraphExtender.cs")
- [General Search](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/GeneralSearchGraphExtender.cs#L60 "GeneralSearchGraphExtender.cs")
- Najkrótsze ścieżki:
  - [Ford-Bellman](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L33 "ShortestPathsGraphExtender.cs")
  - [FindNegativeCostCycle](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L163 "ShortestPathsGraphExtender.cs")
  - [Dijkstra](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L223 "ShortestPathsGraphExtender.cs")
  - [DAG](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L279 "ShortestPathsGraphExtender.cs")
  - [BFPaths](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L321 "ShortestPathsGraphExtender.cs")
  - [Floyd-Warshall](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L357 "ShortestPathsGraphExtender.cs")
  - [Johnson](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/ShortestPathsGraphExtender.cs#L489 "ShortestPathsGraphExtender.cs")
- Aproksymacyjne TSP:
  - [Simple Greedy](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/AproxTSPGraphExtender.cs#L27 "AproxTSPGraphExtender.cs")
  - [Kruskal Based](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/AproxTSPGraphExtender.cs#L75 "AproxTSPGraphExtender.cs")
  - [Tree Based](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/AproxTSPGraphExtender.cs#L141 "AproxTSPGraphExtender.cs")
  - [Include](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/AproxTSPGraphExtender.cs#L216 "AproxTSPGraphExtender.cs")
  - [Three-Optimal](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/AproxTSPGraphExtender.cs#L357 "AproxTSPGraphExtender.cs")
- TSP:
  - [Backtracking](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/BacktrackingTSPGraphExtender.cs#L32 "BacktrackingTSPGraphExtender.cs")
  - [Branch and Bound](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/BranchAndBoundTSPGraphExtender.cs#L31 "BranchAndBoundTSPGraphExtender.cs")
- [Euler Path](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/EulerPathGraphExtender.cs#L26 "EulerPathGraphExtender.cs")
- Minimalne Drzewo Rozpinające:
  - [Prim](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MSTGraphExtender.cs#L29 "MSTGraphExtender.cs")
  - [Kruskal](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MSTGraphExtender.cs#L66 "MSTGraphExtender.cs")
  - [Boruvka](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MSTGraphExtender.cs#L112 "MSTGraphExtender.cs")
- [Biconnected Components](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/BiconnectedGraphExtender.cs#L27 "BiconnectedGraphExtender.cs")
- [Strongly Connected Components](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/SCCGraphExtender.cs#L10 "SCCGraphExtender.cs")
- [Sortowanie Topologiczne](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/GraphHelperExtender.cs#L129 "GraphHelperExtender.cs")
- [Odwrotność](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/SCCGraphExtender.cs#L25 "SCCGraphExtender.cs")
- [Izomorfizmy](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/IsomorphismGraphExtender.cs#L11 "IsomorphismGraphExtender.cs")
- Maksymalny Przepływ:
  - [Ford-Fulkerson](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L50 "MaxFlowGraphExtender.cs")
  - [Dinic](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L218 "MaxFlowGraphExtender.cs")
  - [BF](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L128 "MaxFlowGraphExtender.cs")
  - [DFS](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L336 "MaxFlowGraphExtender.cs")
  - [Malhotra-Kumar-Maheshwari](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L278 "MaxFlowGraphExtender.cs")
  - [Push-Relabel](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MaxFlowGraphExtender.cs#L514 "MaxFlowGraphExtender.cs")
- [Maksymalny Przepływ o Minimalnym Koszcie](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/MinCostGraphExtender.cs#L51 "MinCostGraphExtender.cs")
- [Generowanie Grafów](https://github.com/tomasz-herman/Graph.dll/blob/master/Graph.dll/RandomGraphGenerator.cs#L11 "RandomGraphGenerator.cs")

# Używanie biblioteki w projekcie

Żeby użyć tej biblioteki wytarczy dodać referecję paczki nugeta w pliku projektu:

```
<PackageReference Include="Graph.dll" Version="7.2.3" />
```

# Motywacja

Dostarczona studentom wersja biblioteka uniemożliwia użycie jej na systemach innych niż Windows, takich jak systemy bazujące na jądrze Linux czy MacOS. Zgodnie z otrzymanym mailem nie jest także planowana wersja kompatybilna z innymi systemami. Zdeobfuskowana wersja biblioteki umożliwia korzystanie z niej na innych sytemach. **Jest to jedyny cel powstania tego projektu. Biblioteka tu zamieszczona powinna być używana wyłącznie w tym celu, oraz tylko w wypadku posiadania licencji na używanie niezobfuskowanej wersji biblioteki.** Ponadto stwierdzenie, że "system Windows jest obecnie systemem dominującym i ktoś kto nie porusza się w nim sprawnie, po prostu nie jest dobrym informatykiem" jest po prostu obraźliwe i aroganckie. Wielu ludzi świadomie nie korzysta z systemu Windows, gdyż jest to na przykład sprzeczne z ich światopoglądem, a zmuszanie ich do tego(celowe bądź nie) jest niewłaściwe. Nie każdy chce być szpiegowany przez system operacyjny, nie każdy też zgadza się z polityką Microsoftu. Poniżej załączona część wypowiedzi:
>**Biblioteka Graph działa jedynie w systemie Windows (i tak niestety musi być).**
Nie jest moim celem zmuszanie Państwa do używania Windows,  
opisana cecha biblioteki jest efektem ubocznym obfuskacji. A obfuskacja jest konieczna.  
  
>Niestety platforma .NET ma taką przykrą cechę, że na podstawie niezabezpieczonych plików binarnych  
można przy pomocy ogólnie dostępnych narzędzi w prosty sposób odzyskać kod źródłowy programu.  
A na podanie Państwu na tacy gotowych do wykorzystania kodów źródłowych algorytmów z biblioteki  
zgodzić się niestety nie mogę.  
  
>Jako studenci naszego wydziału macie Państwo prawo do bezpłatnego korzystania z Windows  
do celów uczelnianych.  
Wiem, że niektórzy z Państwa preferują inne systemy, ale fakty są takie, że system Windows  
jest obecnie systemem dominującym i ktoś kto nie porusza się w nim sprawnie, po prostu nie jest  
dobrym informatykiem, a laboratorium z ASD2 może być okazją na poćwiczenie korzystania z Windows  
dla tych, którzy nie robią tego na co dzień.  
**Dostosowanie biblioteki Graph do innych systemów nie jest planowane.**

