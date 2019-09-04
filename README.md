# Graph.dll deobfuscated
**Graf** – podstawowy obiekt rozważań [teorii grafów](https://pl.wikipedia.org/wiki/Teoria_graf%C3%B3w "Teoria grafów"), [struktura matematyczna](https://pl.wikipedia.org/wiki/Struktura_matematyczna "Struktura matematyczna") służąca do przedstawiania i badania relacji między obiektami. W uproszczeniu graf to zbiór [wierzchołków](https://pl.wikipedia.org/wiki/Wierzcho%C5%82ek_(teoria_graf%C3%B3w) "Wierzchołek (teoria grafów)"), które mogą być połączone [krawędziami](https://pl.wikipedia.org/wiki/Kraw%C4%99d%C5%BA_grafu "Krawędź grafu") w taki sposób, że każda krawędź kończy się i zaczyna w którymś z wierzchołków.

W świecie informatyki graf jest abstrakcyjnym typem danych, który implementuje graf skierowany lub nieskierowany. Niniejsza biblioteka umożliwia zapis grafu za pomocą listy lub macierzy sąsiedztwa oraz późniejszą jego obróbkę za pomocą licznych algorytmów takich jak:
- [Depth First Search](https://github.com/tomasz-herman/Graph.dll/blob/master/DFSGraphExtender.cs "DFSGraphExtender.cs")
- [Breadth First Search](https://github.com/tomasz-herman/Graph.dll/blob/master/GeneralSearchGraphExtender.cs "GeneralSearchGraphExtender.cs")
- [General Search](https://github.com/tomasz-herman/Graph.dll/blob/master/GeneralSearchGraphExtender.cs "GeneralSearchGraphExtender.cs")
- Najkrótsze ścieżki:
-- [Ford-Bellman](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [FindNegativeCostCycle](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [Dijkstra](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [DAG](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [BFPaths](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [Floyd-Warshall](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
-- [Johnson](https://github.com/tomasz-herman/Graph.dll/blob/master/ShortestPathsGraphExtender.cs "ShortestPathsGraphExtender.cs")
- Aproksymacyjne TSP:
-- [Simple Greedy](https://github.com/tomasz-herman/Graph.dll/blob/master/AproxTSPGraphExtender.cs "AproxTSPGraphExtender.cs")
-- [Kruskal Based](https://github.com/tomasz-herman/Graph.dll/blob/master/AproxTSPGraphExtender.cs "AproxTSPGraphExtender.cs")
-- [Tree Based](https://github.com/tomasz-herman/Graph.dll/blob/master/AproxTSPGraphExtender.cs "AproxTSPGraphExtender.cs")
-- [Include](https://github.com/tomasz-herman/Graph.dll/blob/master/AproxTSPGraphExtender.cs "AproxTSPGraphExtender.cs")
-- [Three-Optimal](https://github.com/tomasz-herman/Graph.dll/blob/master/AproxTSPGraphExtender.cs "AproxTSPGraphExtender.cs")
- TSP:
-- [Backtracking](https://github.com/tomasz-herman/Graph.dll/blob/master/BacktrackingTSPGraphExtender.cs "BacktrackingTSPGraphExtender.cs")
-- [Branch and Bound](https://github.com/tomasz-herman/Graph.dll/blob/master/BranchAndBoundTSPGraphExtender.cs "BranchAndBoundTSPGraphExtender.cs")
- [Euler Path](https://github.com/tomasz-herman/Graph.dll/blob/master/EulerPathGraphExtender.cs "EulerPathGraphExtender.cs")
- Minimalne Drzewo Rozpinające:
-- [Prim](https://github.com/tomasz-herman/Graph.dll/blob/master/MSTGraphExtender.cs "MSTGraphExtender.cs")
-- [Kruskal](https://github.com/tomasz-herman/Graph.dll/blob/master/MSTGraphExtender.cs "MSTGraphExtender.cs")
-- [Boruvka](https://github.com/tomasz-herman/Graph.dll/blob/master/MSTGraphExtender.cs "MSTGraphExtender.cs")
- [Biconnected Components](https://github.com/tomasz-herman/Graph.dll/blob/master/BiconnectedGraphExtender.cs "BiconnectedGraphExtender.cs")
- [Strongly Connected Components](https://github.com/tomasz-herman/Graph.dll/blob/master/SCCGraphExtender.cs "SCCGraphExtender.cs")
- [Sortowanie Topologiczne](https://github.com/tomasz-herman/Graph.dll/blob/master/GraphHelperExtender.cs "GraphHelperExtender.cs")
- [Odwrotność](https://github.com/tomasz-herman/Graph.dll/blob/master/SCCGraphExtender.cs "SCCGraphExtender.cs")
- [Izomorfizmy](https://github.com/tomasz-herman/Graph.dll/blob/master/IsomorphismGraphExtender.cs "IsomorphismGraphExtender.cs")
- Maksymalny Przepływ:
-- [Ford-Fulkerson](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
-- [Dinic](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
-- [BF](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
-- [DFS](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
-- [Malhotra-Kumar-Maheshwari](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
-- [Push-Relabel](https://github.com/tomasz-herman/Graph.dll/blob/master/MaxFlowGraphExtender.cs "MaxFlowGraphExtender.cs")
- [Maksymalny Przepływ o Minimalnym Koszcie](https://github.com/tomasz-herman/Graph.dll/blob/master/MinCostGraphExtender.cs "MinCostGraphExtender.cs")
- [Generowanie Grafów](https://github.com/tomasz-herman/Graph.dll/blob/master/RandomGraphGenerator.cs "RandomGraphGenerator.cs")

# Motywacja

Dostarczona studentom wersja biblioteka uniemożliwia użycie jej na systemach innych niż Windows, takich jak systemy bazujące na jądrze Linux czy MacOS. Zgodnie z otrzymanym mailem nie jest także planowana wersja kompatybilna z innymi systemami. Zdeobfuskowana wersja biblioteki umożliwia korzystanie z niej na innych sytemach. **Jest to jedyny cel powstania tego projektu.** Natomiast stwierdzenie, że "system Windows jest obecnie systemem dominującym i ktoś kto nie porusza się w nim sprawnie, po prostu nie jest dobrym informatykiem" jest po prostu obraźliwe i aroganckie. Wielu ludzi świadomie nie korzysta z systemu Windows, gdyż jest to na przykład sprzeczne z ich światopoglądem, a zmuszanie ich do tego(celowe bądź nie) jest niewłaściwe. Nie każdy chce być szpiegowany przez system operacyjny, nie każdy też zgadza się z polityką Microsoftu. Poniżej załączona część wypowiedzi:
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

