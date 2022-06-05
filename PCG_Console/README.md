# circulant-generators
# Greed Circulants Generator (GCG) implementation

## Launch and configure the PCG Console program
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
Upon completion of the program (successful or unsuccessful) .csv files (only if successful) and .bin appear in the  (Output) folder.
