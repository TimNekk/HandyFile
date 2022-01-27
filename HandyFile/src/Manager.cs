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
        private DirectoryInfo _currentDirectory = new(Directory.GetCurrentDirectory());
        private int _selected;
        private int _startFrom;
        private string _mask = "";
        private int _depth;

        private readonly string[][] _menuActions =
        {
            new[] {"Enter", "Откр"},
            new[] {"alt+C", "Коп"},
            new[] {"alt+V", "Вств"},
            new[] {"E", "Кдр"},
            new[] {"Del", "Удл"},
            new[] {"N", "Сзд"},
            new[] {"Пробл", "Вдл"},
            new[] {"alt+D", "Уб вдл"},
            new[] {"K", "Кнкт"},
            new[] {"M", "Мск"},
            new[] {"B", "Глб"},
            new[] {"D", "Diff"},
            new[] {"Alt+Esc", "Вхд"},
        };

        private readonly string[][] _fileActions =
        {
            new[] {"Esc", "Выйти"},
        };

        private readonly string[] _encodings = {"UTF-8", "UTF-32", "ASCII", "Unicode"};

        private List<string> _addedFiles = new();
        private List<string> _copyFiles = new();

        private readonly ConsoleKey[] _bannedKeys =
        {
            ConsoleKey.Tab,
            ConsoleKey.LeftWindows,
            ConsoleKey.RightWindows,
            ConsoleKey.DownArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.RightArrow,
            ConsoleKey.UpArrow,
            ConsoleKey.Delete,
            ConsoleKey.Backspace
        };

        /// <summary>
        /// Prints the file manager
        /// </summary>
        public void Print()
        {
            Console.SetCursorPosition(0, 0);
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);

            string directory = _currentDirectory is null ? "" :
                _currentDirectory.FullName.Length > width - 20 ? ".." + _currentDirectory.FullName[^(width - 20)..] :
                _currentDirectory.FullName;

            ConsoleExtension.Print("┌" + new string('─', width - 2) + "┐");
            Console.SetCursorPosition((width - directory.Length) / 2 - 2, 0);
            ConsoleExtension.Print($" {directory} ", ConsoleColor.Black, ConsoleColor.White);

            var directoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth);

            for (int lineIndex = 0; lineIndex < height - 3; lineIndex++)
            {
                Console.SetCursorPosition(0, lineIndex + 1);
                ConsoleExtension.Print("│" + new string(' ', width - 2) + "│");
                Console.SetCursorPosition(1, lineIndex + 1);
                if (lineIndex + _startFrom < directoriesAndFiles.Count)
                {
                    PathInfo path = directoriesAndFiles[lineIndex + _startFrom];
                    string item = path.NameForPrint.Length > width - 20 ? path.NameForPrint[..(width - 20)] + ".." : path.NameForPrint;
                    ConsoleColor foregroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.Black : ConsoleColor.White;
                    ConsoleColor backgroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.White : ConsoleColor.Black;

                    if (path.IsFile && _addedFiles.Contains(path.FullName))
                    {
                        foregroundColor = ConsoleColor.White;
                        backgroundColor = _selected == lineIndex + _startFrom ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
                    }
                    ConsoleExtension.Print(item + new string(' ', width - item.Length - 2), foregroundColor, backgroundColor);
                }
            }
            PrintInfoText();
            PrintAvailableActions(_menuActions);
        }

        /// <summary>
        /// Prints alert with selector
        /// </summary>
        /// <param name="title">Text that will be displayed at title</param>
        /// <param name="options">Options that user must select</param>
        /// <param name="selected">Currently selected option</param>
        private void PrintChoiceAlert(string title, string[] options, int selected)
        {
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);
            int sizeX = Math.Max(options.Max(x => x.Length) + 2, title.Length) + 8;
            int sizeY = options.Length + 4;

            for (int lineIndex = 0; lineIndex < sizeY; lineIndex++)
            {
                Console.SetCursorPosition(width / 2 - sizeX / 2, height / 2 - sizeY / 2 + lineIndex);

                if (lineIndex == 1)
                {
                    ConsoleExtension.Print(
                        new string(' ', (sizeX - title.Length) / 2) + title + 
                        new string(' ', sizeX - (sizeX - title.Length) / 2 - title.Length),
                        ConsoleColor.Black, ConsoleColor.White);
                    continue;
                }

                if (lineIndex >= 3 && lineIndex < options.Length + 3)
                {
                    ConsoleExtension.Print(new string(' ', 2), ConsoleColor.Black, ConsoleColor.White);
                    ConsoleExtension.Print(" " + options[lineIndex - 3] + " ",
                        selected == lineIndex - 3 ? ConsoleColor.White : ConsoleColor.Black,
                        selected == lineIndex - 3 ? ConsoleColor.Black : ConsoleColor.White);
                    ConsoleExtension.Print(new string(' ', sizeX - options[lineIndex - 3].Length - 4), 
                        ConsoleColor.Black, ConsoleColor.White);
                    continue;
                }
                ConsoleExtension.Print(new string(' ', sizeX), ConsoleColor.Black, ConsoleColor.White);
            }
            Console.SetCursorPosition(0, height - 1);
        }

        /// <summary>
        /// Prints alert with user input
        /// </summary>
        /// <param name="title">Text that will be displayed at title</param>
        /// <param name="userInput">Current user input</param>
        private void PrintInputAlert(string title, string userInput)
        {
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);
            int sizeX = Math.Max(title.Length, userInput.Length) + 8;
            sizeX += sizeX % 2;
            int sizeY = 5;

            for (int lineIndex = 0; lineIndex < sizeY; lineIndex++)
            {
                Console.SetCursorPosition(width / 2 - sizeX / 2, height / 2 - sizeY / 2 + lineIndex);
                
                switch (lineIndex)
                {
                    case 1:
                        ConsoleExtension.Print(" ");
                        ConsoleExtension.Print(
                            new string(' ', (sizeX - title.Length) / 2 - 1) + title +
                            new string(' ', sizeX - (sizeX - title.Length) / 2 - title.Length - 1),
                            ConsoleColor.Black, ConsoleColor.White);
                        ConsoleExtension.Print(" ");
                        continue;
                    case 3:
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

        /// <summary>
        /// Prints some information in bottom line of manager
        /// </summary>
        private void PrintInfoText()
        {
            var (width, height) = (Console.WindowWidth, Console.WindowHeight);

            string infoText = "";
            if (_addedFiles.Count > 0) infoText += $" Выделено: {_addedFiles.Count} ";
            if (_copyFiles.Count > 0) infoText += $" Скопировано: {_copyFiles.Count} ";

            Console.SetCursorPosition(0, height - 2);
            ConsoleExtension.Print("└" + new string('─', width - 2)  + "┘");
            Console.SetCursorPosition((width - infoText.Length) / 2 - 2, height - 2);
            ConsoleExtension.Print($" {infoText} ", ConsoleColor.Black, ConsoleColor.White);
        }

        /// <summary>
        /// Prints actions that is available in bottom line
        /// </summary>
        /// <param name="actions">Actions that are available</param>
        /// <param name="resetHeight">If height should be reset</param>
        private void PrintAvailableActions(string[][] actions, bool resetHeight = true)
        {
            int height = Console.WindowHeight;

            Console.SetCursorPosition(0, resetHeight ? height - 1 : Console.GetCursorPosition().Top);
            ConsoleExtension.Print(" ");

            foreach (var action in actions)
            {
                ConsoleExtension.Print(action[0], ConsoleColor.Black, ConsoleColor.White);
                ConsoleExtension.Print(" " + action[1] + " ");
            }

            ConsoleExtension.Print("");
            if (resetHeight)
            {
                Console.SetCursorPosition(0, 0);
                Console.SetCursorPosition(0, height - 1);
            }
        }

        /// <summary>
        /// Creates alert with selector
        /// </summary>
        /// <param name="title">Text that will be displayed at title</param>
        /// <param name="options">Options that user must select</param>
        /// <param name="selected">Currently selected option</param>
        /// <returns>Chosen option index</returns>
        private int CreateChoiceAlert(string title, string[] options, int selected = 0)
        {
            while (true)
            {
                PrintChoiceAlert(title, options, selected);

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

        /// <summary>
        /// Creates alert with user input
        /// </summary>
        /// <param name="title">Text that will be displayed at title</param>
        /// <param name="regex">Keys that is allowed</param>
        /// <param name="userInput">Current user input</param>
        /// <param name="allowEmpty">Is empty input allowed</param>
        /// <param name="useTab">Is tab addition active</param>
        /// <returns>User input</returns>
        private string CreateInputAlert(string title, Regex regex, string userInput = "", bool allowEmpty = false, bool useTab = false)
        {
            while (true)
            {
                PrintInputAlert(title, userInput);
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter && (userInput.Length != 0 || allowEmpty)) return userInput;
                if (key.Key == ConsoleKey.Backspace && userInput.Length > 0) userInput = userInput[..^1];
                else if (useTab && key.Key == ConsoleKey.Tab)
                {
                    PathInfo[] directoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth)
                        .Where(item => item.Name.StartsWith(userInput)).ToArray();

                    if (directoriesAndFiles.Length == 1)
                    {
                        userInput = directoriesAndFiles.First().Name + (directoriesAndFiles.First().IsDirectory ? "/" : "");
                    }
                    else if (directoriesAndFiles.Length > 1)
                    {
                        string[] names = directoriesAndFiles.Select(item => item.Name)
                            .OrderBy(name => name.Length).ToArray();

                        int endIndex = Enumerable.Range(0, names.First().Length)
                            .Select(i => names.Select(x => x[i]).Distinct().Count() == 1)
                            .ToList().FindIndex(x => x == false);
                        endIndex = endIndex == -1 ? names.First().Length : endIndex;
                        userInput = names.First()[..endIndex];
                    }
                }
                else if (regex.IsMatch(key.KeyChar.ToString()) && userInput.Length < Console.WindowWidth - 12 &&
                         _bannedKeys.Contains(key.Key) is false)
                {
                    userInput += key.KeyChar.ToString();
                }
            }
        }

        /// <summary>
        /// Gets key that represents action
        /// </summary>
        public bool ListenForButtonPress()
        {
            var key = Console.ReadKey();
            try
            {
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
                        GoToParentDirectory();
                        break;
                    case ConsoleKey.C:
                        if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && _selected != 0) Copy();
                        break;
                    case ConsoleKey.V:
                        if ((key.Modifiers & ConsoleModifiers.Alt) != 0 && _copyFiles.Count != 0) Paste();
                        break;
                    case ConsoleKey.Delete:
                        if (_selected != 0) DeleteSelected();
                        break;
                    case ConsoleKey.E:
                        if (_selected != 0) OpenFileWithEncoding();
                        break;
                    case ConsoleKey.N:
                        CreateNewFile();
                        break;
                    case ConsoleKey.Spacebar:
                        if (_selected != 0) AddSelectedFile();
                        break;
                    case ConsoleKey.A:
                        if ((key.Modifiers & ConsoleModifiers.Alt) != 0) SelectAll((key.Modifiers & ConsoleModifiers.Shift) != 0);
                        break;
                    case ConsoleKey.D:
                        if ((key.Modifiers & ConsoleModifiers.Alt) != 0) DeselectAll();
                        else if (_addedFiles.Count == 2) GetDiff();
                        break;
                    case ConsoleKey.K:
                        OpenFiles(_addedFiles.ToArray());
                        break;
                    case ConsoleKey.M:
                        SetUpMask();
                        break;
                    case ConsoleKey.B:
                        SetUpDepth();
                        break;
                    case ConsoleKey.Escape:
                        if ((key.Modifiers & ConsoleModifiers.Alt) != 0) return true;
                        break;
                }
            }
            catch (Exception e){}
            Print();
            return false;
        }

        /// <summary>
        /// Sets depth with user input
        /// </summary>
        private void SetUpDepth()
        {
            PathInfo selectedItem = GetSelectedItem();

            string depth = CreateInputAlert("Введите глубину ('*' - бесконечно)", new Regex(@"[\d*]"),
                _depth == int.MaxValue ? "*" : _depth.ToString());
            bool result = int.TryParse(depth, out _depth);

            if (result is false)
            {
                if (depth == "*")
                {
                    _depth = int.MaxValue;
                }
                else
                {
                    return;
                }
            }

            bool oldItemDoesNotExist = SetSelectedItem(selectedItem.FullName);
            if (oldItemDoesNotExist is false)
            {
                (_startFrom, _selected) = (0, 0);
            }

            AdjustHeight();
        }

        /// <summary>
        /// Sets mask with user input
        /// </summary>
        private void SetUpMask()
        {
            PathInfo selectedItem = GetSelectedItem();
            _mask = CreateInputAlert("Введите маску", new Regex(@"[^\/]"), _mask, true, true).TrimEnd('/');

            bool result = SetSelectedItem(selectedItem.FullName);

            if (result is false)
            {
                (_startFrom, _selected) = (0, 0);
            }

            AdjustHeight();
        }

        /// <summary>
        /// Resets added files
        /// </summary>
        private void DeselectAll()
        {
            _addedFiles = new List<string>();
        }

        /// <summary>
        /// Adds every file in directory
        /// </summary>
        /// <param name="useDepth"></param>
        private void SelectAll(bool useDepth = false)
        {
            var files = GetDirectoriesAndFiles(_currentDirectory, useDepth ? int.MaxValue : 0)
                .Where(item => item.IsFile)
                .Select(file => file.FullName);
            _addedFiles = new List<string>(files);
        }

        /// <summary>
        /// Adds selected file in directory
        /// </summary>
        private void AddSelectedFile()
        {
            PathInfo selectedItem = GetSelectedItem();

            if (selectedItem.IsFile)
            {
                if (_addedFiles.Contains(selectedItem.FullName))
                {
                    _addedFiles.Remove(selectedItem.FullName);
                }
                else
                {
                    _addedFiles.Add(selectedItem.FullName);
                }
            }
        }

        /// <summary>
        /// Creates new file in current directory
        /// </summary>
        private void CreateNewFile()
        {
            string fileName = CreateInputAlert("Введите название файла", new Regex(@"[\w,.\-_а-яА-я ';\[\]{}()!@#$%^&+=~`]")).Trim();
            string filePath = Path.Join(_currentDirectory.FullName, fileName);

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

        /// <summary>
        /// Opens file with encoding
        /// </summary>
        private void OpenFileWithEncoding()
        {
            PathInfo selectedItem = GetSelectedItem();
            if (selectedItem.IsFile is false)
            {
                return;
            }

            Encoding encoding = GetEncoding();
            OpenFile(selectedItem.File, encoding);
        }

        /// <summary>
        /// Gets encoding selected by user
        /// </summary>
        /// <returns>Selected encoding</returns>
        private Encoding GetEncoding()
        {
            int encodingIndex = CreateChoiceAlert("Выберите кодировку", _encodings);

            return encodingIndex switch
            {
                0 => Encoding.UTF8,
                1 => Encoding.UTF32,
                2 => Encoding.ASCII,
                3 => Encoding.Unicode,
                _ => Encoding.UTF8
            };
        }

        /// <summary>
        /// Moves selection down
        /// </summary>
        private void MoveListDown()
        {
            var directoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth);

            if (_selected + 1 < directoriesAndFiles.Count)
            {
                _selected++;
                int height = Console.WindowHeight;
                if (_selected - _startFrom > height - 4) _startFrom++;
            }
        }

        /// <summary>
        /// Moves selection up
        /// </summary>
        private void MoveListUp()
        {
            if (_selected - 1 >= 0)
            {
                _selected--;
                if (_selected < _startFrom) _startFrom--;
            }
        }

        /// <summary>
        /// Goes to currently selected directory
        /// </summary>
        private void GoToSelectedDirectory()
        {
            if (_selected == 0 && _currentDirectory != null)
            {
                GoToParentDirectory();
                return;
            }

            PathInfo selectedItem = GetSelectedItem();
            if (selectedItem.CanBeAccessed())
            {
                if (selectedItem.IsDirectory)
                {
                    (_currentDirectory, _selected, _startFrom) = (selectedItem.Directory, 0, 0);
                }
                else if (selectedItem.IsFile)
                {
                    OpenFile(selectedItem.File, Encoding.UTF8);
                }
                else
                {
                    (_currentDirectory, _selected, _startFrom) = (new DirectoryInfo(selectedItem.Name), 0, 0);
                }
            }
        }

        /// <summary>
        /// Opens given files with selected encoding
        /// </summary>
        /// <param name="files">List of files</param>
        private void OpenFiles(string[] files)
        {
            string text = "";
            
            foreach (var file in files)
            {
                if (File.Exists(file) is false)
                {
                    return;
                }
                
                text += File.ReadAllText(file);
            }

            WaitTillFileClose(text);
        }

        /// <summary>
        /// Opens given file with selected encoding
        /// </summary>
        /// <param name="file">File to open</param>
        /// <param name="encoding">Encoding for opening</param>
        private void OpenFile(FileInfo file, Encoding encoding)
        {
            if (file.Exists is false)
            {
                return;
            }

            string text = File.ReadAllText(file.FullName, encoding);
            
            WaitTillFileClose(text);
        }

        /// <summary>
        /// Waits till user closes file
        /// </summary>
        /// <param name="text">Text that will be displayed</param>
        private void WaitTillFileClose(string text)
        {
            ConsoleExtension.Print("");
            Console.Clear();

            while (true)
            {
                Console.SetCursorPosition(0, 0);
                ConsoleExtension.Print(text + "\n");
                PrintAvailableActions(_fileActions, text.Split("\n").Length <= Console.WindowHeight - 1);

                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }

            Console.Clear();
        }

        /// <summary>
        /// Gets every directories and files with given depth
        /// </summary>
        /// <param name="root">Path for reading</param>
        /// <param name="depth">Depth of reading</param>
        /// <returns>List of directories and files</returns>
        private List<PathInfo> GetDirectoriesAndFiles(DirectoryInfo root, int depth = 0)
        {
            var list = new List<PathInfo>();
            if (root is null) return Directory.GetLogicalDrives().Select(drive => new PathInfo(new DriveInfo(drive), 0, _depth)).ToList();
            if (_depth - depth == 0) list.Add(new PathInfo(root.Parent ?? root.Root, _depth - depth - 1, _depth));

            try
            {
                var directoriesWithMask = root.EnumerateDirectories(_mask).Select(directory => directory.FullName);
                foreach (var directory in root.EnumerateDirectories())
                {
                    if (directoriesWithMask.Contains(directory.FullName)) list.Add(new PathInfo(directory, _depth - depth, _depth));
                    if (depth != 0) list.AddRange(GetDirectoriesAndFiles(directory, depth - 1));
                }

                list.AddRange(root.EnumerateFiles(_mask).Select(file => new PathInfo(file, _depth - depth, _depth)));
            }
            catch (UnauthorizedAccessException) {}

            return list;
        }

        /// <summary>
        /// Gets currently selected file or directory
        /// </summary>
        /// <returns>Selected file or directory</returns>
        private PathInfo GetSelectedItem()
        {
            var directoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth);
            return directoriesAndFiles[_selected];
        }

        /// <summary>
        /// Goes to parent of current directory
        /// </summary>
        private void GoToParentDirectory()
        {
            if (_currentDirectory is null)
            {
                return;
            }

            DirectoryInfo previousDirectory = _currentDirectory;
            _currentDirectory = _currentDirectory.Parent;
            SetSelectedItem(previousDirectory.FullName);
            AdjustHeight();
        }

        /// <summary>
        /// Adjusts hieght
        /// </summary>
        private void AdjustHeight()
        {
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

        /// <summary>
        /// Sets selection to given file or directory
        /// </summary>
        /// <param name="path">Path to file or directory</param>
        /// <returns>If setting was successful</returns>
        private bool SetSelectedItem(string path)
        {
            var directoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth);
            var itemIndex = directoriesAndFiles.FindIndex(item => item.FullName == path);
            if (itemIndex != -1)
            {
                _selected = itemIndex;
                return true;
            }

            _selected = 0;
            return false;
        }

        /// <summary>
        /// Copies current file
        /// </summary>
        private void Copy()
        {
            PathInfo selectedItem = GetSelectedItem();

            if (_addedFiles.Count == 0)
            {
                if (selectedItem.IsFile)
                {
                    _copyFiles = new List<string> {selectedItem.FullName};
                }
            }
            else
            {
                _copyFiles = new List<string>(_addedFiles);
            }
        }

        /// <summary>
        /// Pastes copied files
        /// </summary>
        private void Paste()
        {
            foreach (var fileName in _copyFiles)
            {
                FileInfo file = new FileInfo(fileName);
                string newPath = Path.Join(_currentDirectory.FullName, file.Name);

                if (File.Exists(newPath))
                {
                    int index = 1;

                    while (true)
                    {
                        string pathWithIndex = Path.Join(_currentDirectory.FullName, $"{file.Name}_{index}");
                        if (file.Extension != "")
                        {
                            pathWithIndex = Path.Join(_currentDirectory.FullName,
                                $"{file.Name[..^file.Extension.Length]}_{index}{file.Extension}");
                        }

                        if (File.Exists(pathWithIndex) is false)
                        {
                            newPath = pathWithIndex;
                            break;
                        }

                        index++;
                    }
                }

                file.CopyTo(newPath);
            }
        }

        /// <summary>
        /// Deletes selected files and directories
        /// </summary>
        private void DeleteSelected()
        {
            PathInfo selectedItem = GetSelectedItem();

            selectedItem.Delete();

            var currentDirectoriesAndFiles = GetDirectoriesAndFiles(_currentDirectory, _depth);

            if (_selected >= currentDirectoriesAndFiles.Count)
            {
                _selected--;
            }
        }

        /// <summary>
        /// Gets Longest Common Subsequence of given texts
        /// </summary>
        /// <param name="oldText">Old text</param>
        /// <param name="newText">New text</param>
        /// <returns>Longest Common Subsequence</returns>
        private List<string> GetLongestCommonSubsequence(List<string> oldText, List<string> newText)
        {
            var subsequence = new List<string>();
            int oldTextIndex = 0;

            foreach (var line in oldText)
            {
                int index = newText.IndexOf(line, oldTextIndex);

                if (index != -1)
                {
                    subsequence.Add(line);
                    oldTextIndex = index;
                }
            }

            return subsequence;
        }

        /// <summary>
        /// Prints difference between to selected files
        /// </summary>
        private void GetDiff()
        {
            List<string> oldText = File.ReadLines(_addedFiles[0]).ToList();
            List<string> newText = File.ReadLines(_addedFiles[1]).ToList();
            List<string> subsequence = GetLongestCommonSubsequence(oldText, newText);
            var (oldIndex, newIndex, space, subsequenceIndex) = (0, 0, 9, 0);
            Console.Clear();

            while (oldIndex < oldText.Count || newIndex < newText.Count)
            {
                bool isChanged = false;
                try
                {
                    var oldLine = oldText[oldIndex];
                    if (subsequence[subsequenceIndex] != oldLine)
                    {
                        ConsoleExtension.Print(" " + (oldIndex + 1) + new string(' ', space - (oldIndex + 1).ToString().Length + 1),
                            backgroundColor: ConsoleColor.Red);
                        ConsoleExtension.Print($" - {oldLine}\n", backgroundColor: ConsoleColor.DarkRed);
                        (isChanged, oldIndex) = (true, oldIndex + 1);
                    }
                } catch (Exception) {}
                try
                {
                    var newLine = newText[newIndex];
                    if (subsequence[subsequenceIndex] != newLine)
                    {
                        ConsoleExtension.Print(new string(' ', space - (newIndex + 1).ToString().Length + 1) + (newIndex + 1) + " ",
                            backgroundColor: ConsoleColor.Green);
                        ConsoleExtension.Print($" + {newLine}\n", backgroundColor: ConsoleColor.DarkGreen);
                        (isChanged, newIndex) = (true, newIndex + 1);
                    }
                } 
                catch (Exception) {}
                if (isChanged) continue;
                ConsoleExtension.Print(" " + (oldIndex + 1) +
                                       new string(' ', space - (newIndex + 1).ToString().Length - (oldIndex + 1).ToString().Length) +
                                       (newIndex + 1) + " ",
                    backgroundColor: ConsoleColor.DarkGray);
                try {ConsoleExtension.Print($"   {newText[newIndex]}\n");} catch (Exception) {ConsoleExtension.Print($"   {oldText[oldIndex]}\n");}
                (oldIndex, newIndex, subsequenceIndex) = (oldIndex + 1, newIndex + 1, subsequenceIndex < subsequence.Count - 1 ? subsequenceIndex + 1 : subsequenceIndex);
            }
            Console.ReadKey();
        }
    }
}