namespace HandyFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Manager manager = new Manager();
            manager.Print();
            while (true)
            { 
                bool exit = manager.ListenForButtonPress();
                if (exit) break;
            }
        }
    }
}