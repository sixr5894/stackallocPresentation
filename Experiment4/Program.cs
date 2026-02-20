using System.Runtime.CompilerServices;

namespace Experiment4
{
    internal unsafe class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                Span<int> stackallocated = stackalloc int[4];// pay attention to warning
                Console.WriteLine((uint)(Unsafe.AsPointer(ref stackallocated[0])));// 16 bytes difference each time
            }
        }

    }
}
