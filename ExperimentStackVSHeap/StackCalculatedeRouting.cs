using System.Management;
using System.Runtime.CompilerServices;

namespace StackCalculated
{
    public struct IPtoGateway
    {
        private const uint FirstLeftBit = 2147483648u;
        public uint IP;
        public int SubnetMask;
        public int Gateway;

        public readonly int IsBitSet(int index)
        {
            if (index < 0 || index > 32)
            {
                throw new InvalidOperationException("Index is out of range.");
            }
            var temp = (int)(IP & (FirstLeftBit >> index));
            return temp / (int)(FirstLeftBit >> index);
        }
    }

    public unsafe struct TrieNode
    {
        public TrieNode()
        {
            GateWay = -1;
            Next = null;
        }

        public TrieNode* Next;

        public bool InitializeNext(TrieNode* allocatedSpace)
        {
            this.Next = allocatedSpace;
            this.Next[0] = new TrieNode();
            this.Next[1] = new TrieNode();
            return false;
        }

        public int GateWay;
    }

    public unsafe struct TrieStatemachine(int level, TrieNode* startNode, IPtoGateway tobeLoaded)
    {
        public int Level = level;
        public TrieNode* CurrentNode = startNode;
        public IPtoGateway Load = tobeLoaded;

        public bool MoveNext(TrieNode* allocatedSpace, out bool spaceNeeded)
        {
            if (spaceNeeded = CurrentNode->Next is null &&
                (allocatedSpace == null || CurrentNode->InitializeNext(allocatedSpace)))
            {
                return true;
            }

            CurrentNode = &CurrentNode->Next[Load.IsBitSet(Level)];
            Level++;
            return Level != Load.SubnetMask;
        }
        public void Finilize()
        {
            CurrentNode->GateWay = Load.Gateway;
        }
    }

    public unsafe struct Trie
    {
        public Trie()
        {
            this.ZeroLevelValue = new TrieNode();
            this.ZeroLevel = (TrieNode*)Unsafe.AsPointer(ref ZeroLevelValue);
        }

        public TrieNode ZeroLevelValue;
        public TrieNode* ZeroLevel;

        public void SetOne(ref IPtoGateway request)
        {
            TrieNode* current = ZeroLevel;
            int gateway = 1;

            for (int i = 0; i < 32 && current->Next != null; i++)
            {
                gateway = current->GateWay >= 0 ? current->GateWay : gateway;

                current = &current->Next[request.IsBitSet(i)];
            }
            gateway = current->GateWay >= 0 ? current->GateWay : gateway;
            request.Gateway = gateway;
        }

        public void SetGateways(IPtoGateway[] loads, IPtoGateway[] toBeSet)
        {
            int cunt = 0;
            TrieStatemachine loader = new TrieStatemachine();
            for (int i = 0; i < loads.Length; i++)
            {
                loader.Load = loads[i];
                loader.CurrentNode = ZeroLevel;
                loader.Level = 0;

                TrieNode* allocatedSpace = null;

                while (loader.MoveNext(allocatedSpace, out bool spaceNeeded))
                {
                    if (spaceNeeded)
                    {
                        Span<TrieNode> temp  = stackalloc TrieNode[2];
                        allocatedSpace = (TrieNode*)Unsafe.AsPointer(ref temp[0]);
                        cunt++;
                    }
                    else
                    {
                        allocatedSpace = null;
                    }

                }
                loader.Finilize();
            }

            for (int i = 0; i < toBeSet.Length; i++)
            {
                ref IPtoGateway request = ref toBeSet[i];
                SetOne(ref request);
            }
        }

        //private void Traverse(TrieNode* node)
        //{
        //    if (node is null)
        //    {
        //        return;
        //    }
        //    if (node->GateWay != -1)
        //    {
        //        Console.Write(node->GateWay + " ");
        //    }

        //    Traverse(node->Left);

        //    Traverse(node->Right);
        //}
    }

}
