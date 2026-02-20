namespace Experiment2
{
    internal unsafe class Program
    {
        public static void Goback()
        {
            byte temp = 113;
        }


        static void Main(string[] args)
        {
            byte first = 0;

            Goback();
            byte* ptr = &first;
            ptr -= 80;
            Console.WriteLine(*ptr);
        }
    }
}
