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

        public readonly bool IsBitSet(int index)
        {
            if (index < 0 || index > 32)
            {
                throw new InvalidOperationException("Index is out of range.");
            }
            return (IP & (FirstLeftBit >> index)) == (FirstLeftBit >> index);
        }
    }

    public unsafe struct TrieNode
    {
        public TrieNode()
        {
            GateWay = -1;
            Left = null;
            Right = null;
        }

        public TrieNode* Left;
        public TrieNode* Right;

        public int GateWay;
    }

    public unsafe struct TrieStatemachine(int level, TrieNode* startNode, IPtoGateway tobeLoaded)
    {
        public int Level = level;
        public TrieNode* CurrentNode = startNode;
        public IPtoGateway Load = tobeLoaded;

        public bool MoveNext(TrieNode* allocatedSpace,bool spaceProvided, out bool spaceNeeded)
        {
            TrieNode* next = null;
            spaceNeeded = false;
            int offset = 0;
            if (!Load.IsBitSet(Level))
            {
                next = CurrentNode->Left;
            }
            else
            {
                next = CurrentNode->Right;
                offset++;
            }
            if(next is null)
            {
                if (spaceProvided)
                {
                    //init
                    allocatedSpace[0] = new TrieNode();
                    allocatedSpace[1] = new TrieNode();
                    CurrentNode->Left = &allocatedSpace[0];
                    CurrentNode->Right = &allocatedSpace[1];
                    next = &allocatedSpace[offset];
                }
                else
                {
                    spaceNeeded = true;
                    return true;
                }
            }

            Level++;
            CurrentNode = next;
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
            int gateway = -1;

            for (int i = 0; i < 32 && current != null; i++)
            {
                TrieNode* next;
                gateway = current->GateWay >= 0 ? current->GateWay : gateway;
                if (!request.IsBitSet(i))
                {
                    next = current->Left;
                }
                else
                {
                    next = current->Right;
                }
                current = next;
            }

            request.Gateway = gateway;
        }

        public void SetGateways(IPtoGateway[] loads, IPtoGateway[] toBeSet)
        {
            int cunt = 0;
            for (int i = 0; i < loads.Length; i++)
            {
                TrieStatemachine loader = new TrieStatemachine(0, ZeroLevel, loads[i]);

                TrieNode* allocatedSpace = null;
                bool spaceProvided = false;

                while (loader.MoveNext(allocatedSpace, spaceProvided, out bool spaceNeeded))
                {
                    if (spaceNeeded)
                    {
                        Span<TrieNode> temp  = stackalloc TrieNode[2];
                        allocatedSpace = (TrieNode*)Unsafe.AsPointer(ref temp[0]);
                        spaceProvided = true;
                        cunt++;
                    }
                    else
                    {
                        allocatedSpace = null;
                        spaceProvided = false;
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

        private void Traverse(TrieNode* node)
        {
            if (node is null)
            {
                return;
            }
            if (node->GateWay != -1)
            {
                Console.Write(node->GateWay + " ");
            }

            Traverse(node->Left);

            Traverse(node->Right);
        }
    }

}
