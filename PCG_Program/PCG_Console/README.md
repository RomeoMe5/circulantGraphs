# ENG      
## Launch and configure the PCG Console program      
**COULD WORK AS IS.**               
The program is launched by running PCG Console.exe. Without changing the configuration file, it will be launched with the basic parameters specified in the file.

## Configuration         

There are several ways to change the input / output of information.

### PCGConsole.exe.config
The main configuration file of .NET application. Parameters:
```xml
    <add key="FirstTaskPath" value="Input\task1.xml"/>
    <add key="LastStatePath" value="laststate.xml"/>
```

* **LastStatePath** - State of program`s last execution. Progress result is saved if it was interrupted without completing the entire search cycle. It does not always work correctly. It works when stopped by typing command ('stop' command in the console). If you cannot start the program with the saved configuration, you must delete this file (then the last state of the program will be lost).
* **FirstTaskPath** - parameters of synthesis of circulant graphs. 'Task' is meant as a list of parametric descriptions (consisting of the number of nodes and the number of generators, there are also hidden parameters that have not been added). Description of mutable variables:

### Task attribute description
The contents of the Task.xml file is as follows:
```xml
<Task xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
nodesDescription="6-750,1000,2500,5000,7500" dimension="3" threadsCount="6" 
outputFolderPath="Output/only1" fullName="Glukhikh A." isFullLogging="false" isFullReport="false" />
```

* **nodesDescription** - count of nodes in the graph. For simple tasks, you can create lists, similar to: 100, 250-340, 1029, 2090. If any unprocessed characters are contained program should ignore them and process the record without them.
* **dimension** - count of generators. It can be any positive number, but it is better to set 2 or greater.
* **threadsCount** - count of threads. If the attribute is absent or less than 1, then the default is an integer divided by the number of logical processors by 2. For large values, the optimal number of threads will be selected. The program itself runs in at least 3 threads (tracking command input, queue checking, and synthesis).
* **fullName** - User name. There may also be some kind of unique name used in the compilation of the report.
* **output** - Path to save the data. The data is saved in a specialized format, and to receive it in a “formatted” state, you must use the program that I gave to Romanov A.Yu. (lays on his github).
* **isFullLogging** - full logging of the entire synthesis process to a .csv file. Not working yet.
* **isFullReport** - full logging of the entire synthesis process to a .bin file. Not working yet.
* **mode** - program operation mode. Not added.

### About the program
The executor configures the file (by default, it is located in the Input folder located in the root directory of the program) containing the xml markup for Task. What items can be changed:

* **fullname**: Your identifier is indicated. The record is not trimmed by spaces, case insensitive;
* **grade**: the number of generators.
* **nodesDescription**: range of the number of nodes.
* **threadsCount** - number of threads is indicated 0, if you are not sure how much you want to allocate for the program

For the correct termination of the program and for saving the results, type **"stop"**. 
In this case, the program will save the intermediate results. With the same file configuration, the program will continue to work the next time it starts from a breakpoint. Results are saved in laststate.xml.
If laststate.xml remains after starting the task with other parameters (number of nodes, number of generators), you need to delete the file, otherwise there will be errors when starting.
Upon completion of the program (successful or unsuccessful) .csv files (only if successful) and .bin appear in the  (**Output**) folder.

***
# RUS
# Запуск и конфигурация программы PCG Console
Работает из коробки.            
Запуск программы выполняется посредством запуска PCG Console.exe. Без изменения файла конфигурации будет запущена с базовыми параметрами, указанными в файле.

## Изменение конфигурации

Есть несколько способов изменить ввод/вывод информации.

### PCGConsole.exe.config

Основной файл конфигурации, который создается при сборке приложения. Основные пункты для изменения:

    \&lt;addkey=&quot;FirstTaskPath&quot; value=&quot;Input\task1.xml&quot;/\&gt;

    \&lt;add key=&quot;LastStatePath&quot; value=&quot;laststate.xml&quot;/\&gt;

**LastStatePath** – последнее состояние. Здесь сохраняются последнее состояние программы, если она была прервана не завершив весь цикл поиска. Не всегда работает корректно. Срабатывает при остановке (команда stop в консоли). Если не удается запустить для сохраненной конфигурации программу необходимо удалить этот файл (тогда будут утрачены последнее состояние программы).

**FirstTaskPath** – Параметры поиска. Под задачей подразумевается список параметрических описаний (состоящих из кол-ва узлов и кол-ва образующих, также есть скрытые параметры, которые не были добавлены, т.к. недоработаны). Описание изменяемых переменных:

### Описание атрибутов Task

Содержимое файла Task.xml выглядит следующим образом:

\&lt;Task xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; nodesDescription=&quot;200&quot; grade=&quot;5&quot; threadsCount=&quot;4&quot; outputFolderPath=&quot;Output&quot; fullName=&quot;Иванко И.И.&quot; isFullLogging=&quot;false&quot; isFullReport=&quot;false&quot; /\&gt;

**nodesDescription** – кол-во узлов в графе. Для простых задач можно формировать списки, на подобии: 100, 250-340, 1029, 2090. Если какие-то необрабатываемые символы присутствуют, то программа должна их проигнорировать и обработать запись без них.

**grade** – кол-во образующих. Может быть любым положительным, но лучше задавать 2 и более.

**threadsCount** – кол-во потоков. Если атрибут отсутствует или меньше чем 1, то по умолчанию считается целое от деления количества логических процессоров на 2. При больших значениях, будет выбрано оптимальное количество потоков. Сама программа работает как минимум в 3 потоках (отслеживание ввода команды, проверка очереди и синтез).

**fullName**  **–** Имя исполнителя. Может также быть какое-то уникальное имя, используется при составлении отчета.

**output**  **–** куда сохранять данные.

**isFullLogging** – полное логирование всего процесса синтеза в .csv-файл. Пока не работает.

**isFullReport** – полное логирование всего процесса синтеза в .bin-файл. Пока не работает.

**mode** – режим работы программы. Не добавлено.

### О работе программы

Исполнитель конфигурирует файл (по умолчанию находится в папке Input, находящейся в корневой директории программы), содержащий xml-разметку Task. Какие пункты можно изменить:

- **fullname** : Указывается Ваш идентификатор. Запись не обрезается по пробелам, нечувствительно к регистру;

- **grade** : количестов образующих.

- **nodesDescription** : диапазон количества узлов.

- **threadsCount** – кол-во потоков указывается 0, если не уверены, сколько хотите выделить на работу программы

Для корректного завершения программы и для сохранения результатов необходимо набрать **stop****.** В этом случае программа произведет сохранение промежуточных результатов. При той же конфигурации файла, программа продолжит работу при следующем запуске с точки останова. Результаты сохраняются в laststate.xml.

Если остался laststate.xml после запуска задачи с другими параметрами (кол-во узлов, кол-во образующих), необходимо файл удалить, иначе возникнут ошибки при запуске.

По завершению работы программы (успешного или не успешного) в папке **Output** появятся файлы  .csv (только в случае успешного завершения) и .bin.      

# Задание:
**(Для тех, кто хочет пополнить датасет результатами новых генераций)**
1. Выбрать параметры генерации из [файла с заданиями](https://docs.google.com/spreadsheets/d/10N8XH53xGTVuzbEtp1AKWbubBUpvprfqvThP8Bm6G_Y/edit?usp=sharing), указав свои ФИО и группу напротив него. Если Вы взяли чужое задание, то баллы получит другой человек. Файл постоянно пополняется. Одновременно можно выполнять только 1 задание.
2. Скачать PCGConsole.
3. Настроить генератор и запустить (nodesDescription можно разбивать на части. Например, если надо сгенерировать графы от 530 до 1024, то можно разбить диапазон на части и генерировать в несколько сессий: 530-600, 601-700 и т.д.). Процесс генерации может быть длительным. В мануале ниже дано описание, как можно прервать и продолжить процесс генерации.        
3а. Попробуйте сначала сгенерировать небольшой диапазон, например из 10 шт. (530-540), оцените время, сколько у вас уйдет, и потом задавайте большой, или делайте весь диапазон из небольших шажков. Чем больше количество узлов в графе и грейд, тем дольше считает. Скорость растет экспоненциально.
4. Результат в виде .bin файла и .csv загрузить на гитхаб в репозитарий в папку [RawData](https://github.com/RomeoMe5/circulantGraphs/tree/master/PCG_Console/RawData) (путем комита). Если файлов много - единым архивом. Незавершенный синтез не присылать. Продублировать в ЛС [https://vk.com/romeome](https://vk.com/romeome), чтобы я сделал проверку. В [файл с заданиями](https://docs.google.com/spreadsheets/d/10N8XH53xGTVuzbEtp1AKWbubBUpvprfqvThP8Bm6G_Y/edit?usp=sharing) поставить отметку завершения.
5. За что даются баллы: 0,25 балла за 24 часа работы генератора в режиме работы не менее 4-х потоков.
6. При обнаружении попытки мухлевать с файлами, возможность работать с генератором будет закрыта, а вы - наказаны отрицательными баллами.
