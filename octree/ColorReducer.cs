using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace octree
{
    class ColorReducer
    {
        private class Tree
        {
            public Tree[] next = new Tree[8];
            public Color? c;
            public int counter;
            public int childrenCounter;
            public int nonEpmtyBranchesCount = 0;
            public bool IsEmpty() { return c == null; }

            internal int leastPopularIndex()
            {
                int leastPopIndex = 0;
                while (leastPopIndex <= 7 && next[leastPopIndex] == null)
                    leastPopIndex++;
                for (int i = leastPopIndex; i < 8; i++)
                    if (next[i] != null && next[i].counter < next[leastPopIndex].counter)
                        leastPopIndex = i;
                return leastPopIndex;
            }

        }
        private Tree octreeHead;
        private Dictionary<int, LinkedList<Tree>> treesByLevels;
        public WriteableBitmap ReduceColorsAfterConst(WriteableBitmap wbmp, int nrOfColors)
        {
            WriteableBitmap reduced = new WriteableBitmap(wbmp);
            octreeHead = new Tree();
            for (int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    InsertTree(octreeHead, wbmp.GetPixel(j, i), 0);
                }
            }

            reduce(nrOfColors);

            for (int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    reduced.SetPixel(j, i, findReducedColor(wbmp.GetPixel(j, i)));
                }
            }
            return reduced;
        }

        private void reduce(int nrOfColors)
        {
            Queue<Tree> q = new Queue<Tree>();
            Stack<Tree> s = new Stack<Tree>();
            Traverse(q, s, octreeHead);

            while (octreeHead.childrenCounter > nrOfColors)
            {
                var t = s.Pop();
                if (t.nonEpmtyBranchesCount > 0)
                {
                    for (int i = 0; i < t.next.Length; i++)
                    {
                        if (t.next[i] != null)
                        {
                            octreeHead.childrenCounter--;
                            t.next[i] = null;
                            if (octreeHead.childrenCounter <= nrOfColors) break;
                        }
                    }
                }

            }
        }

        internal ImageSource ReduceColorsAlongConst(WriteableBitmap wbmp, int nrOfColors)
        {
            WriteableBitmap reduced = new WriteableBitmap(wbmp);
            octreeHead = new Tree();
            treesByLevels = new Dictionary<int, LinkedList<Tree>>();
            treesByLevels[0] = new LinkedList<Tree>();
            treesByLevels[0].AddFirst( octreeHead );
            for(int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    InsertAndReduceTree(octreeHead, wbmp.GetPixel(j, i),0, nrOfColors);
                }
            }
            for(int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    reduced.SetPixel(j, i, findReducedColor(wbmp.GetPixel(j,i)));
                }
            }
            return reduced;
        }
        private void InsertAndReduceTree(Tree tree, Color c, byte level, int nrOfColors)
        {
            if (tree.IsEmpty()) {
                tree.c = c;
                tree.counter++;
                octreeHead.childrenCounter++;
            }
            else if (c == tree.c)
            {
                tree.counter++;
            }
            else
            {
                byte pos = GetOctalPosFromPixel(c, level);
                if (tree.next[pos] == null)
                {
                    tree.next[pos] = new Tree();
                    if(tree.nonEpmtyBranchesCount == 0)
                    {
                        if (treesByLevels.ContainsKey(level + 1))
                        {
                            treesByLevels[level].AddFirst(tree);
                        }
                        else
                        {
                            treesByLevels[level] = new LinkedList<Tree>();
                            treesByLevels[level].AddFirst(tree);
                        }
                    }
                    tree.nonEpmtyBranchesCount++;
                    tree.next[pos].c = c;
                    tree.next[pos].counter++;
                    octreeHead.childrenCounter++;
                    if(octreeHead.childrenCounter > nrOfColors)
                    {
                        //tree.next[tree.leastPopularIndex()] = null;
                        reduceUsingDict(nrOfColors);
                    }
                }
                else
                {
                    InsertAndReduceTree(tree.next[pos], c, ++level, nrOfColors);
                }
            }
        }

        private void reduceUsingDict(int nrOfColors)
        {
            bool isDone = nrOfColors >= octreeHead.childrenCounter;
            for(int i = 6; i >= 0; i--)
            {
                if (!treesByLevels.ContainsKey(i)) continue;
                var tl = treesByLevels[i];
                foreach(var t in tl)
                {
                   while(t.nonEpmtyBranchesCount > 0 && !isDone)
                   {
                        int k = t.leastPopularIndex();
                        if(t.next[k] != null)
                        {
                            t.next[k] = null;
                            t.nonEpmtyBranchesCount--;
                            if(treesByLevels.ContainsKey(i+1))
                                treesByLevels[i+1].Remove(t.next[k]);
                            if(t.nonEpmtyBranchesCount == 0)
                                treesByLevels[i].Remove(t);
                            isDone = --octreeHead.childrenCounter <= nrOfColors;
                        }
                        if (isDone) return;
                   }
                }
            }
        }

        private void Traverse(Queue<Tree> q, Stack<Tree> s, Tree tree)
        {
            q.Enqueue(tree);
            while(q.Count != 0)
            {
                Tree it = q.Dequeue();
                s.Push(it);
                if (it.nonEpmtyBranchesCount == 0) continue;
                foreach (var tt in it.next)
                    if (tt != null)
                        q.Enqueue(tt);
            }
        }

        internal Color findReducedColor(Color c)
        {
            Tree it = octreeHead;
            byte l = 0;
            byte pos = GetOctalPosFromPixel(c, l);
            while (it.next[pos] != null)
            {
                it = it.next[pos];
                if (l >= 7) break;
                pos = GetOctalPosFromPixel(c, ++l);
            }
            return it.c.Value;
        }
        private void InsertTree(Tree tree, Color c, byte level)
        {
            if (tree.IsEmpty()) {
                tree.c = c;
                tree.counter++;
                octreeHead.childrenCounter++;
            }
            else if (c == tree.c)
            {
                tree.counter++;
            }
            else
            {
                byte pos = GetOctalPosFromPixel(c, level);
                if (pos == 7 && level == 0)
                {

                    c = c;
                    pos = GetOctalPosFromPixel(c, level);
                }
                if (tree.next[pos] == null)
                {
                    tree.next[pos] = new Tree();
                    tree.nonEpmtyBranchesCount++;
                }
                InsertTree(tree.next[pos], c, ++level);
            }
        }

        //private void updateNrOfChildren(Tree tree)
        //{
        //    while(tree != null)
        //    {
        //        tree.childrenCounter++;
        //        tree = tree.parent;
        //    }
        //}

        private Tree NewAndInit(Color c)
        {
            Tree t = new Tree();
            t.c = c;
            t.counter++;
            return t;

        }

        private byte GetNthBit(byte r, byte pos)
        {
            byte one = 1;
            byte shifted = (byte)(r >> pos);
            return (byte)(shifted & one);
        }

        private void reduce(Tree tree, int level, int nrOfColors)
        {
            if (tree.nonEpmtyBranchesCount == 0) return;
            if (level < 8)
            {
                foreach (var t in tree.next)
                {
                    if (t != null) reduce(t, level + 1, nrOfColors);
                }
            }
            //if (tree.nonEpmtyBranchesCount == 0) return;
            //if (level < 8)
            //{
            //    foreach (var t in tree.next)
            //        if (t != null) reduce(t, level + 1, nrOfColors);
            //}
            //while (tree.nonEpmtyBranchesCount > 0 && nrOfColors < octreeHead.childrenCounter)
            //{
            //    int lpi = tree.leastPopularIndex(); 
            //    if (lpi > 7) break;
            //    tree.next[lpi] = null;
            //    octreeHead.childrenCounter--;
            //    tree.nonEpmtyBranchesCount--;
            //}

        }

        private byte GetOctalPosFromPixel(Color c, byte pos)
        {
            if (pos >= 8 || pos < 0) 
                throw new InvalidOperationException($"Position {pos} does not exist in byte" );
            pos = (byte) (0x7 - pos);
            byte rBit = GetNthBit(c.R, pos);
            byte gBit = GetNthBit(c.G, pos);
            byte bBit = GetNthBit(c.B, pos);
            return (byte)((rBit << 2) + (gBit << 1) + bBit);

        }
        private void BitTest()
        {
            TestColorPos(Color.FromArgb(0, 1, 1, 1), 0, 0b111);
            TestColorPos(Color.FromArgb(0, 0b10, 0b01, 0b11), 1, 0b101);
            TestColorPos(Color.FromArgb(0, 0b100, 0b001, 0b110), 2, 0b101);

        }
        private void TestColorPos(Color c, byte pos, byte exp)
        {
            var octalC11 = GetOctalPosFromPixel(c, pos);

            if (octalC11 != exp)
            {
                MessageBox.Show(octalC11.ToString() + "!=" + exp);
            }
        }
    }
}
