# ЗАПУСК ПРОГРАММЫ


Открыть Program.cs
Главный файл
`..\HandyFile\src\Program.cs`

Запускать нужно в отдельной консоли *(не в встроенном окне Rider)*

[Инструкция как сделать это в Rider](https://i.imgur.com/pRQ5Hoc.gif)


# ДОСТУПНЫЕ ДЕЙСТВИЯ


1. Перемещение по директориям

      `↑` Переместиться вверх
      
      `↓` Переместиться вних
      
      `←` Вернуться назад
      
      `→` Зайти в папки или открыть файл

2. Выделение файла(ов) *(Сохраняется при смене диретории)*

      `Пробел` Выделяет или снимает выделение с файла
      
      `Alt+A` Выделет все файлы в папке
      
      `Alsh+A` Выделет все файлы в папке и подпапких с учетом глубины `Alt + Shift + A`
      
      `Alt+D` Снимает выделение со всех файлов
      
3. Открытие файла или папки

      `Enter` Открывает выбранный файл
      
      `E` Дает выбрать кодировку и открывает в ней выбранный файл
      
      `K` Конкатенирует и выводит содержимое всех выделенных файлов

4. Копирование, вставка и перемещение файла(ов)

      `Alt+C` Копирует либо выбранный файл, либо все выделенные файлы
      
      `Alt+V` Вставка в текущую директорию *(с помощью этих действий можно сделать "перемещение")*

5. Удаление файла

      `Delete` Удаляет выбранный файл

6. Создание файла

      `N` Дает выбрать кодировку и имя файла. Создает файл в тек. дир. и пишет пустую строку в кодировке

7. Маска

      `M` Дает ввести и устанавливает маску *(нужно ввести пустую маску чтобы увидеть все файлы)*

8. Глубина

      `B` Дает ввести и устанавливает гублину просмотра *(чтобы вывести все поддиректории введите `*` **НЕ СТОИТ ИСПОЛЬЗОВАТЬ ГДЕ МНОГО ФАЙЛОВ, НАПРИМЕР C:\\***

9. Diff

      `D` Выводит разницу между двумя выделенными файлами *(файл выделеный 1-ым будет принят за старый, а 2-ой за новый)*


10. Завершение работы

     `Alt+Esc` Завершает работу программы


# КОММЕНТАРИИ


Есть метод более 40 строк ListenForButtonPress()

Но так switch большой и разбивать нет смысла *(и вообще это рекомендация)*  :)

Перемещение реализуется с помощью копирование, вставки и удаления

Выбрать диск можно если много раз нажать назад `←`

2 доп. - Нужно ввести маску `M` и выбрать глубину * `B`

3 доп. - Нужно ввести маску `M`, выбрать глубину * `B`, выделить все поддиректории `Alt + Shift + A` и скопировать `Alt + C`. Затем перейти в нужную директорию и вставить `Alt + V`

5 доп. - Можно испоьзовать `Tab` для дополнение при вводе маски (типа поиск)

Интрефейс адаптируется под размер консоли (после изменение размера нажми `↑`, чтобы обновить интерфейс)

В архиве есть файлы для тестирования Diff (сначала выдели `old.txt` и потом `new.txt` и нажми `D`)
