using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HandyFile
{
    class Program
    {
        static void Main(string[] args)
        { 
            // var _addedFiles = new Dictionary<string, string[]>();
            // _addedFiles.Add("path1", new [] {"file1", "file2"});
            //
            // Console.Write(_addedFiles.ContainsKey());
            // return;
            
            Manager manager = new Manager();
            manager.Print();
            while (true)
            {
                manager.ListenForButtonPress();
            }
        }
    }
}