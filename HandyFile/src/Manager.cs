using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HandyFile
{
    public class Manager
    {
        private string _currentPath = @"E:\ЕГЭ";
        // private string _currentDirectory = Directory.GetCurrentDirectory();
        private int _selected;
        private int _startFrom;
        private string _copyPath = "";

        private string[][] _menuActions =
        {
            new [] {"↑↓←→", "Навиг"},
            new [] {"Enter", "Откр"},
            new [] {"alt+C", "Коп"},
            new [] {"alt+V", "Встав"},
            new [] {"E", "Кодир"},
            new [] {"Del", "Удал"},
            new [] {"N", "Созд"},
            new [] {"Пробл", "Выдел"},
            new [] {"alt+D", "Уб выд"},
            new [] {"K", "Конкат"},
        };
        
        private string[][] _fileActions =
        {
            new [] {"Esc", "Выйти"},
        };
        
        private string[] _encodings = {"UTF-8", "UTF-32", "ASCII", "Unicode"};

        private Dictionary<string, List<string>> _addedFiles = new ();
        
        public void Print()
        {
            Console.SetCursorPosition(0, 0);
            
            int width = Console.WindowWidth - 1;
            int height = Console.WindowHeight;

            string currentDirectoryToPrint = _currentPath.Length > width - 20
                ? ".." + _currentPath[^(width - 20)..]
                : _currentPath;
            
            ConsoleExtension.Print("┌" + new string('─', (width - currentDirectoryToPrint.Length) / 2 - 2 + (currentDirectoryToPrint.Length % 2 == 0 ? 1 : 0)));
            ConsoleExtension.Print($" {currentDirectoryToPrint} ", ConsoleColor.Black, ConsoleColor.White);
            ConsoleExtension.Print(new string('─', (width - currentDirectoryToPrint.Length) / 2 - 2) + "┐\n");
            
            string[] directoriesAndFiles = GetCurrentDirectoriesAndFiles(true);

            for (int lineIndex = 0; lineIndex < height - 3; lineIndex++)
            {
                ConsoleExtension.Print("│");
                if (lineIndex + _startFrom < directoriesAndFiles.Length)
                {
                    string item = directoriesAndFiles[lineIndex + _startFrom].Length > width - 20
                        ? directoriesAndFiles[lineIndex + _startFrom][..(width - 20)] + ".."
                        : directoriesAndFiles[lineIndex + _startFrom];

                    ConsoleColor foregroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.Black : ConsoleColor.White;
                    ConsoleColor backgroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.White : ConsoleColor.Black;

                    if (_addedFiles.ContainsKey(_currentPath) && _addedFiles[_currentPath].Contains(directoriesAndFiles[lineIndex + _startFrom].TrimStart()))
                    {
                        foregroundColor = ConsoleColor.White;
                        backgroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                    }
                    
                    ConsoleExtension.Print(item + new string(' ', width - item.Length - 2), foregroundColor, backgroundColor);
                }
                else
                {
                    ConsoleExtension.Print(new string(' ', width - 2));
                }
            
                ConsoleExtension.Print("│\n");
            }

            if (_copyPath == "")
            {
                ConsoleExtension.Print("└" + new string('─', width - 2) + "┘");
            }
            else
            {
                string copyText = $"{Path.GetFileName(_copyPath)}";
                string copyTextPrint = " Copied: " + (copyText.Length > width - 30
                    ? ".." + copyText[^(width - 30)..]
                    : copyText) + " ";
                
                ConsoleExtension.Print("└" + new string('─', (width - copyTextPrint.Length) / 2 - 2 + (copyTextPrint.Length % 2 == 0 ? 1 : 0)));
                ConsoleExtension.Print($" {copyTextPrint} ", ConsoleColor.Black, ConsoleColor.White);
                ConsoleExtension.Print(new string('─', (width - copyTextPrint.Length) / 2 - 2) + "┘");
            }

            showAvailableActions(_menuActions);
            // ConsoleExtension.Print("└" + $"sel:{_selected} sf:{_startFrom} h:{height}" + new string('─', width - 20) + "┘");
        }

        private void ShowChoiceAlert(string title, string[] options, int selected)
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            int sizeX = Math.Max(options.Max(x => x.Length) + 2, title.Length) + 8;
            int sizeY = options.Length + 4;
            
            for (int lineIndex = 0; lineIndex < sizeY; lineIndex++)
            {
                Console.SetCursorPosition(width / 2 - sizeX / 2, height / 2 - sizeY / 2 + lineIndex);

                if (lineIndex == 1)
                {
                    ConsoleExtension.Print(new string(' ', (sizeX - title.Length) / 2) + title + new string(' ', sizeX - (sizeX - title.Length) / 2 - title.Length), 
                        ConsoleColor.Black, ConsoleColor.White);
                    continue;
                }
                
                if (lineIndex >= 3 && lineIndex < options.Length + 3)
                {
                    ConsoleExtension.Print(new string(' ', 2), ConsoleColor.Black, ConsoleColor.White);
                    ConsoleExtension.Print(" " + options[lineIndex - 3] + " ", 
                        selected == lineIndex - 3 ? ConsoleColor.White : ConsoleColor.Black,
                        selected == lineIndex - 3 ? ConsoleColor.Black : ConsoleColor.White);
                    ConsoleExtension.Print(new string(' ', sizeX - options[lineIndex - 3].Length - 4), ConsoleColor.Black, ConsoleColor.White);
                    continue;
                }
                
                ConsoleExtension.Print(new string(' ', sizeX), ConsoleColor.Black, ConsoleColor.White);
            }
            
            Console.SetCursorPosition(0, height - 1);
        }

        public void ShowInputAlert(string title, string userInput)
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight;

            int sizeX = Math.Max(title.Length, userInput.Length) + 8;
            sizeX += sizeX % 2;
            int sizeY = 5;
            
            for (int lineIndex = 0; lineIndex < sizeY; lineIndex++)
            {
                Console.SetCursorPosition(width / 2 - sizeX / 2, height / 2 - sizeY / 2 + lineIndex);

                if (lineIndex == 1)
                {
                    ConsoleExtension.Print(" ");
                    ConsoleExtension.Print(new string(' ', (sizeX - title.Length) / 2 - 1) + title + new string(' ', sizeX - (sizeX - title.Length) / 2 - title.Length - 1), 
                        ConsoleColor.Black, ConsoleColor.White);
                    ConsoleExtension.Print(" ");
                    continue;
                }
                
                if (lineIndex == 3)
                {
                    ConsoleExtension.Print(" ");
                    ConsoleExtension.Print(" ", ConsoleColor.Black, ConsoleColor.White);
                    ConsoleExtension.Print(" " + userInput + new string(' ', sizeX - userInput.Length - 5));
                    ConsoleExtension.Print(" ", ConsoleColor.Black, ConsoleColor.White);
                    ConsoleExtension.Print(" ");

                    continue;
                }
                
                ConsoleExtension.Print(" ");
                ConsoleExtension.Print(new string(' ', sizeX - 2), ConsoleColor.Black, ConsoleColor.White);
                ConsoleExtension.Print(" ");
            }
            
            Console.SetCursorPosition(width / 2 - sizeX / 2 + userInput.Length + 3, height / 2 - sizeY / 2 + 3);
        }

        private void showAvailableActions(string[][] actions, bool resetHeight = true)
        {
            int height = Console.WindowHeight;

            Console.SetCursorPosition(0, resetHeight ? height-1 : Console.GetCursorPosition().Top);
            ConsoleExtension.Print(" ");

            foreach (var action in actions)
            {
                ConsoleExtension.Print(action[0],
                    ConsoleColor.Black, ConsoleColor.White);
                ConsoleExtension.Print(" " + action[1] + " ");
            }
            
            ConsoleExtension.Print("");
            if (resetHeight)
            {
                Console.SetCursorPosition(0, 0);
                Console.SetCursorPosition(0, height-1);
            }
        }

        private int CreateChoiceAlert(string title, string[] options, int selected = 0)
        {
            while (true)
            {
                ShowChoiceAlert(title, options, selected);

                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter)
                {
                    return selected;
                }
                
                if (key.Key == ConsoleKey.DownArrow && selected + 1 < options.Length)
                {
                    selected++;
                }
                else if (key.Key == ConsoleKey.UpArrow && selected - 1 >= 0)
                {
                    selected--;
                }
            }
        }
        
        private string CreateInputAlert(string title, string userInput = "")
        {
            while (true)
            {
                ShowInputAlert(title, userInput);

                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Enter && userInput.Length != 0)
                {
                    return userInput;
                }

                if (key.Key == ConsoleKey.Backspace && userInput.Length > 0)
                {
                    userInput = userInput[..^1];
                    continue;
                }

                if (new Regex(@"[\w,.\-_а-яА-я ';\[\]{}()!@#$%^&+=~`]").IsMatch(key.KeyChar.ToString())  && userInput.Length < Console.WindowWidth - 12)
                {
                    userInput += key.KeyChar.ToString();
                }
            }
        }

        public void ListenForButtonPress()
        {
            var key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    MoveListDown();
                    break;
                case ConsoleKey.UpArrow:
                    MoveListUp();
                    break;
                case ConsoleKey.Enter or ConsoleKey.RightArrow:
                    GoToSelectedDirectory();
                    break;
                case ConsoleKey.LeftArrow:
                    GoToLowerDirectory();
                    break;
                case ConsoleKey.C:
                    if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && _selected != 0)
                    {
                        CopySelected();
                    }
                    break;
                case ConsoleKey.V:
                    if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && _copyPath != "")
                    {
                        Paste();
                    }
                    break;
                case ConsoleKey.Delete:
                    if (_selected != 0)
                    {
                        DeleteSelected();
                    }
                    break;
                case ConsoleKey.E:
                    if (_selected != 0)
                    {
                        OpenFileWithEncoding();
                    }
                    break;
                case ConsoleKey.N:
                    CreateNewFile();
                    break;
                case ConsoleKey.Spacebar:
                    if (_selected != 0)
                    {
                        AddSelectedFile();
                    }
                    break;
                case ConsoleKey.D:
                    if ((key.Modifiers & ConsoleModifiers.Alt) != 0)
                    {
                        DeselectAll();
                    }
                    break;
                case ConsoleKey.K:
                    Concatenate();
                    break;
            }
            Print();
        }

        private void Concatenate()
        {
            
        }

        private void DeselectAll()
        {
            _addedFiles = new Dictionary<string, List<string>>();
        }

        private void AddSelectedFile()
        {
            string selectedItem = GetSelectedItem();

            if (File.Exists(selectedItem) is false)
            {
                return;
            }
            
            string itemName = Path.GetFileName(selectedItem);
            string itemRoot = Path.GetDirectoryName(selectedItem);

            if (_addedFiles.ContainsKey(itemRoot))
            {
                if (_addedFiles[itemRoot].Contains(itemName))
                {
                    _addedFiles[itemRoot].Remove(itemName);
                }
                else
                {
                    _addedFiles[itemRoot].Add(itemName);
                }
            }
            else
            {
                _addedFiles.Add(itemRoot, new List<string> {itemName});
            }
        }

        private void CreateNewFile()
        {
            string fileName = CreateInputAlert("Введите название файла", "").Trim();
            string filePath = Path.Join(_currentPath, fileName);
            
            Print();
            Encoding encoding = GetEncoding();
            
            using (FileStream fs = File.Create(filePath))
            {
                using (StreamWriter writer = new StreamWriter(fs, encoding))
                {
                    writer.Write("");
                }
            }
        }

        private void OpenFileWithEncoding()
        {
            string selectedItem = GetSelectedItem();
            if (File.Exists(selectedItem) is false)
            {
                return;
            }
            
            Encoding encoding = GetEncoding();
            OpenFile(selectedItem, encoding);
        }

        private Encoding GetEncoding()
        {
            int encodingIndex = CreateChoiceAlert("Выберите кодировку", _encodings);

            switch (encodingIndex)
            {
                case 0 : {return Encoding.UTF8;}
                case 1: {return Encoding.UTF32;}
                case 2: {return Encoding.ASCII;}
                case 3: {return Encoding.Unicode;}
            }

            return Encoding.UTF8;
        }

        private void MoveListDown()
        {
            string[] directoriesAndFiles = GetCurrentDirectoriesAndFiles();

            if (_selected + 1 < directoriesAndFiles.Length)
            {
                _selected++;
                int height = Console.WindowHeight;
                if (_selected - _startFrom > height - 4) _startFrom++;
            }
        }
        
        private void MoveListUp()
        {
            if (_selected - 1 >= 0)
            {
                _selected--;
                if (_selected < _startFrom) _startFrom--;
            }
        }

        private void GoToSelectedDirectory()
        {
            if (_selected == 0 && _currentPath != "")
            {
                GoToLowerDirectory();
                return;
            }
            
            string selectedItem = GetSelectedItem();
            if (Directory.Exists(selectedItem) && CanDirectoryBeAccessed(selectedItem))
            {
                (_currentPath, _selected, _startFrom) = (selectedItem, 0, 0);
            }
            else
            {
                OpenFile(selectedItem, Encoding.UTF8);
            }
        }

        private void OpenFile(string filePath, Encoding encoding)
        {
            if (File.Exists(filePath) is false)
            {
                return;
            }

            string text;
            try
            {
                text = File.ReadAllText(filePath, encoding);
            }
            catch (Exception)
            {
                return;
            }
            
            ConsoleExtension.Print("");
            Console.Clear();

            while (true)
            {
                Console.SetCursorPosition(0, 0);
                ConsoleExtension.Print(text + "\n");
                showAvailableActions(_fileActions, text.Split("\n").Length <= Console.WindowHeight - 1 );

                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }
            
            Console.Clear();
        }

        private string[] GetCurrentDirectoriesAndFiles(bool withSymbols = false)
        {
            if (_currentPath == "")
            {
                return Directory.GetLogicalDrives();
            }
            
            string[] previous = {".."};
            string[] directories = Directory.GetDirectories(_currentPath).Select(directory => (withSymbols ? "► " : "") + Path.GetFileName(directory)).ToArray();
            string[] files = Directory.GetFiles(_currentPath).Select(file => (withSymbols ? "  " : "") + Path.GetFileName(file)).ToArray();
            string[] directoriesAndFiles = previous.Concat(directories.Concat(files)).ToArray();

            return directoriesAndFiles;
        }
        
        private string GetSelectedItem()
        {
            string[] directoriesAndFiles = GetCurrentDirectoriesAndFiles();
            string currentItem = Path.Join(_currentPath, directoriesAndFiles[_selected]);
            return currentItem;
        }

        private void GoToLowerDirectory()
        {
            if (_currentPath == "")
            {
                return;
            }
            
            string previousDirectory = Path.GetFileName(_currentPath);
            
            try
            {
                _currentPath = Directory.GetParent(Path.TrimEndingDirectorySeparator(_currentPath)).ToString();
            }
            catch (NullReferenceException)
            {
                string previousDrive = _currentPath;
                (_currentPath, _selected, _startFrom) = ("", 0, 0);
                SetSelectedItem(previousDrive);
                return;
            }
            
            SetSelectedItem(previousDirectory);
            
            int height = Console.WindowHeight;
            if (_selected - (height - 4) >= 0)
            {
                _startFrom = _selected - (height - 4);
            }
            else
            {
                _startFrom = 0;
            }
        }

        private void SetSelectedItem(string itemName)
        {
            string[] directoriesAndFiles = GetCurrentDirectoriesAndFiles();
            var itemIndex = Array.FindIndex(directoriesAndFiles, item => item == itemName);
            if (itemIndex != -1)
            {
                _selected = itemIndex;
            }
        }

        private bool CanDirectoryBeAccessed(string directory)
        {
            try
            {
                Directory.GetFiles(directory);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private void CopySelected()
        {
            string selectedItem = GetSelectedItem();

            if (File.Exists(selectedItem))
            {
                _copyPath = GetSelectedItem();
            }
        }

        private void Paste()
        {
            string fileName = Path.GetFileName(_copyPath);
            string extention = Path.GetExtension(fileName);
            string newPath = Path.Join(_currentPath, fileName);

            if (File.Exists(newPath))
            {
                int index = 1;

                while (true)
                {
                    string pathWithIndex = Path.Join(_currentPath, $"{fileName}_{index}");
                    if (extention != null)
                    {
                        pathWithIndex = Path.Join(_currentPath, $"{fileName[..^extention.Length]}_{index}{extention}");
                    }

                    if (File.Exists(pathWithIndex) is false)
                    {
                        newPath = pathWithIndex;
                        break;
                    }

                    index++;
                }
            }

            File.Copy(_copyPath, newPath);
        }

        private void DeleteSelected()
        {
            string selectedItem = GetSelectedItem();

            if (Directory.Exists(selectedItem))
            {
                Directory.Delete(selectedItem);
            }
            else if (File.Exists(selectedItem))
            {
                File.Delete(selectedItem);
            }

            string[] currentDirectoriesAndFiles = GetCurrentDirectoriesAndFiles();

            if (_selected >= currentDirectoriesAndFiles.Length)
            {
                _selected--;
            }
        }
    }
}