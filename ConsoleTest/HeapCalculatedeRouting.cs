using StackCalculated;



namespace HeapCalculated
{
    public class TrieNode
    {
        public TrieNode()
        {
            Left = default!;
            Right = default!;
            GateWay = -1;
        }

        public TrieNode Left;
        public TrieNode Right;

        public int GateWay;
    }

    public class Trie
    {
        public Trie()
        {
            ZeroLevel = new TrieNode
            {
                GateWay = 0
            };
        }

        public TrieNode ZeroLevel;

        public void LoadOne(ref IPtoGateway relation)
        {
            TrieNode current = ZeroLevel;

            for (int i = 0; i < relation.SubnetMask; i++)
            {
                TrieNode next;
                if (!relation.IsBitSet(i))
                {
                    next = (current.Left ??= new TrieNode());
                }
                else
                {
                    next = (current.Right ??= new TrieNode());
                }
                current = next;
            }
            current.GateWay = relation.Gateway;
        }


        public void SetOne(ref IPtoGateway request)
        {
            TrieNode current = ZeroLevel;
            int gateway = -1;

            for (int i = 0; i < 32 && current is not null; i++)
            {
                TrieNode next;
                gateway = current.GateWay >= 0 ? current.GateWay : gateway;
                if (!request.IsBitSet(i))
                {
                    next = current.Left;
                }
                else
                {
                    next = current.Right;
                }
                current = next;
            }

            request.Gateway = gateway;
        }

        public void SetGateways(IPtoGateway[] loads, IPtoGateway[] toBeSet)
        {
            for (int i = 0; i < loads.Length; i++)
            {
                ref IPtoGateway load = ref loads[i];
                LoadOne(ref load);
            }

            for (int i = 0; i < toBeSet.Length; i++)
            {
                ref IPtoGateway request = ref toBeSet[i];
                SetOne(ref request);
            }

        }




        private void Traverse(TrieNode node)
        {
            if (node is null)
            {
                return;
            }
            if (node.GateWay != -1)
            {
                Console.Write(node.GateWay + " ");
            }

            Traverse(node.Left);
            Traverse(node.Right);
        }
    }
}

