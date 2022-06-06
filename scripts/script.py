import glob
import os
# for filename in glob.glob(r'C:\Users\Иван\Desktop\Romanov2\circulantGraphs\Output\Dimension 4\ring optimal circulant\*.csv'):
#     with open(os.path.join(os.getcwd(), filename), 'r') as final:
#         data = final.read()
#     with open(filename, 'w') as final:
#         final.writelines(['Сигнатура; Диаметр; Среднее расстояние; Время генерации; Кол-во соединений \n'])
#         final.writelines(['Signature; Diameter; Average distance; Generation time; Number of connections \n'])
#         final.write(data)

for filename in glob.glob(r'C:\Users\Иван\Desktop\Romanov2\circulantGraphs\Output\Examples\N=square(x)\*.csv'):
    with open(os.path.join(os.getcwd(), filename), 'r') as final:
        data = final.readlines()
    with open(filename, 'w') as final:
        final.writelines(['Кол-во Образующих; Сигнатура; Диаметр; Среднее расстояние; Время генерации; Кол-во соединений \n'])
        final.writelines(['Nodes count; Signature; Diameter; Average distance; Generation time; Number of connections \n'])
        final.writelines(data[1:])