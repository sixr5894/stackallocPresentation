using StackCalculated;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;


namespace PerformanceOptimization
{
    public unsafe class CustomMergeSort
    {

        public static void Main()
        {
            Random r = new Random();
            IPtoGateway[] load = new IPtoGateway[100_000];
            for (int i = 0; i < 100_000; i++)
            {
                load[i] = new IPtoGateway
                {
                    IP = (uint)r.Next(),
                    Gateway = r.Next(1, 10),
                    SubnetMask = r.Next(1, 32)
                };
            }

            IPtoGateway[] routes = new IPtoGateway[100_000];
            for (int i = 0; i < 100_000; i++)
            {
                routes[i] = new IPtoGateway
                {
                    IP = (uint)r.Next(),
                    Gateway = -1,
                };
            }


            Thread bigstack = new Thread(() =>
            {
                //GC.Collect(GC.MaxGeneration,GCCollectionMode.Aggressive, true, true);
                while (!GC.TryStartNoGCRegion(100_000_000)) ;

                long beforeThreadAllocation = GC.GetAllocatedBytesForCurrentThread();

                Stopwatch sw = Stopwatch.StartNew();
                HeapCalculated.Trie routing = new HeapCalculated.Trie();
                routing.SetGateways(load, routes);
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

                sw = Stopwatch.StartNew();
                StackCalculated.Trie stackrouting = new StackCalculated.Trie();
                stackrouting.SetGateways(load, routes);
                sw.Stop();
                Console.WriteLine(sw.ElapsedTicks);

                long afterThreadAllocation = GC.GetAllocatedBytesForCurrentThread();
                long threadAllocatedBytes = afterThreadAllocation - beforeThreadAllocation;

                Console.WriteLine($"Thread allocated exactly {threadAllocatedBytes} bytes.");
                Console.WriteLine(sw.ElapsedTicks);
                #region ensure
                for(int i=0;i<routes.Length;i++)
                {
                    uint ip = routes[i].IP;
                    string ipString = $"{(ip >> 24) & 0xFF}.{(ip >> 16) & 0xFF}.{(ip >> 8) & 0xFF}.{ip & 0xFF}";
                    Console.WriteLine($"{ipString} goes to -> {routes[i].Gateway}");
                }
                #endregion

            }, 1000_000_000);

            bigstack.Start();
            bigstack.Join();

        }


        private static int Segregate(List<int> items)
        {
            int left= 0;
            int right  = items.Count -1;

            while (left < right)
            {
                if (items[left] % 2 == 0)
                {
                    left++;
                    continue;
                }
                if (items[right] % 2 == 1)
                {
                    right--;
                    continue;
                }
                int temp = items[left];
                items[left] = items[right];
                items[right] = temp;
                left++;
                right--;
            }
            Console.WriteLine(left);
            return left;
        }

        private static void RecursiveUpwards(ref int value, int iteration)
        {
            if (iteration == 100)
            {
                while (true)
                {
                    Thread.Sleep(50);
                    Console.WriteLine(value);
                }
            }
            RecursiveUpwards(ref value, iteration + 1);
        }
    }

}