namespace Experiment1
{
    internal unsafe class Program
    {
        public static void Goback()
        {
            byte temp = 0;
            byte* ptr = &temp;
            ptr += 80;
            *ptr = 111;
        }

        static void Main(string[] args)
        {
            byte _190 = 190;

            Goback();

            Console.WriteLine(_190);
        }
    }
}
