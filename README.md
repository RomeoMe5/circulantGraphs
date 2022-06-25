# ENG

## GCG: Greedy Circulants Generator. Optimal circulants dataset		
**Project files:**    
**Dataset** - DATASET: topologies generation results with different parameters   
**PCG_Programm** - Program for circulants generation, source codes    
**Visualization** - graphs comparing different topologies   
**Scripts** - files editing scripts   

## Generator   
Generating software: master/PCG_Program/PCG_Console    

## Dataset		
master/Dataset		  
Dimension K -- count of the generatrices of the graph C(N; s1, s2, .., sk).		

## Video about GCG
master/visualization/GCG_Romanov_Glukhikh_2020_compressed.mp4

## ABOUT
More additional information about the first version of the GCG in the articles:     
A.Y. Romanov, I.I. Romanova, A.Y. Glukhikh, **Development of a Universal Adaptive Fast Algorithm for the Synthesis of Circulant Topologies for Networks-on-Chip Implementations**, in: 2018 IEEE 38th Int. Conf. Electron. Nanotechnol., IEEE, Kyiv, 2018: pp. 110–115. doi:10.1109/ELNANO.2018.8477462.  
https://ieeexplore.ieee.org/document/8477462/       

**Abstract:**   
In this article, the feasibility of realization of optimal circulant topologies in networks-on-chip was researched. The software for automating the synthesis of circulant topologies of various dimensions and of any number of generatrices is presented. The implemented methods to speed up the synthesis process, based on the properties of circulants, as well as improving the algorithm for calculation of the distance between nodes and caching the adjacency matrix to achieve an acceptable search speed, are proposed. The efficiency and correctness of the proposed algorithm were verified. The proposed algorithm and methods allow designing networks-on-chip with improved characteristics of diameter, average distance between nodes, edges count, and, as a result, reducing the area, occupied by network-on-chip, and other characteristics, in comparison with analogues based on other widespread regular topologies.    

**Additional articles:**
1. A.Y. Romanov, **Development of routing algorithms in networks-on-chip based on ring circulant topologies**, Heliyon. 5 (2019) e01516.
https://doi.org/10.1016/j.heliyon.2019.e01516   
2. A.Y. Romanov, A.A. Amerikanov, E.V. Lezhnev, **Analysis of Approaches for Synthesis of Networks-on-chip by Using Circulant Topologies**, J. Phys. Conf. Ser. 1050 (2018). doi:10.1088/1742-6596/1050/1/012071. https://www.researchgate.net/publication/326643138_Analysis_of_Approaches_for_Synthesis_of_Networks-on-chip_by_Using_Circulant_Topologies   

**CSV files format:**
new format: 		   
nodes count; graph signature; Diameter; average distance; generating time; connections count    
**For old format output files**   
signature; Diameter; average distance; generating time; connections count 
***
# RUS
## Датасет оптимальных циркулянтов. Генерирующее ПО
**Файлы проекта:**    
**Output** - ДАТАСЕТ Циркулянтов с разными параметрами. C(N; s1, s2, .., sk), Dimension (кол-во образующих) = k, N - количество узлов.    	
**PCG_Programm** - программа для генерации топологий, там же исходники    
**Visualization** - графики сравнения различных топологий    
**Scripts** - скрипты для редактирования файлов   
## Оптимальные циркулянтные графы, генерирующие набор данных результатов
Создание программного обеспечения: https://github.com/RomeoMe5/circulantGraphs/tree/master/PCG_Console
столбец K -- количество образующих графа C(N; s1, s2, .., sk).

## Видео о GCG
https://github.com/RomeoMe5/circulantGraphs/blob/master/GCG_Romanov_Glukhikh_2020_compressed.mp4

## Дополнительная информация
Больше дополнительной информации о первой версии GCG в статьях:
А.Ю. Романов, И.И. Романова, А.Ю. Глухих, **Разработка универсального адаптивного быстрого алгоритма синтеза циркулирующих топологий для реализаций сетей на кристалле**, в: 2018 IEEE 38th Int. конф. Электрон. Нанотехнологии., IEEE, Киев, 2018. С. 110–115. doi: 10.1109/ELNANO.2018.8477462.
https://ieeexplore.ieee.org/document/8477462/

**О статье:**
В данной статье исследована возможность реализации оптимальных циркулянтных топологий в сетях на кристалле. Представлено программное обеспечение для автоматизации синтеза циркулянтных топологий различной размерности и любого количества образующих. Предложены реализованные методы ускорения процесса синтеза, основанные на свойствах циркулянтов, а также улучшения алгоритма расчета расстояния между узлами и кэширования матрицы смежности для достижения приемлемой скорости поиска. Проверена работоспособность и корректность предложенного алгоритма. Предложенные алгоритм и методы позволяют проектировать сети на кристалле с улучшенными характеристиками диаметра, среднего расстояния между узлами, количества ребер и, как следствие, уменьшить площадь, занимаемую сетью на кристалле, и другими характеристиками по сравнению с с аналогами на основе других распространенных регулярных топологий.

**Дополнительные статьи:**
1. А.Ю. Романов, **Разработка алгоритмов маршрутизации в сетях на кристалле на основе кольцевых циркулянтных топологий**, Heliyon. 5 (2019) e01516.
https://doi.org/10.1016/j.heliyon.2019.e01516
2. А.Ю. Романов, А.А. Американов, Е.В. Лежнев, **Анализ подходов к синтезу сетей на кристалле с использованием циркулянтных топологий**, J. Phys. конф. сер. 1050 (2018). дои: 10.1088/1742-6596/1050/1/012071. https://www.researchgate.net/publication/326643138_Analysis_of_Approaches_for_Synthesis_of_Networks-on-chip_by_Using_Circulant_Topologies   


**csv files format:**   
nodes count; graph signature; Diameter; average distance; generating time; connections count    
Кол-во вершин;Конфигурация графа;Диаметр;Средний путь;Время (мс);Кол-во соединений    
**Для устаревших версий программы**      
**csv files format:**   
signature; Diameter; average distance; generating time; connections count    
Конфигурация графа;Диаметр;Средний путь;Время (мс);Кол-во соединений
