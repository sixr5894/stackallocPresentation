namespace Experiment3
{
    internal unsafe class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                int c1 = 0;
                int c2 = 0;
                int c3 = 0;
                int c4 = 0;
                Console.WriteLine((uint)&c1);
            }
        }
    }
}
