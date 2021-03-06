﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace octree
{
    class ColorReducer
    {
        private class Tree:IComparable<Tree> 
        {
            public Tree parent;
            public int ind;
            public int level;
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
            public int CompareTo(Tree other)
            {
                int fc = this.level.CompareTo(other.level);
                if (fc != 0) return fc;
                int sc =  -this.counter.CompareTo(other.counter);
                if (sc != 0) return sc;
                int th = this.c?.R >= other.c?.R ? this.c?.R == other.c?.R ? 0 : 1 : -1;
                if (th != 0) return th;
                int tnh = this.c?.G >= other.c?.G ? this.c?.G == other.c?.G ? 0 : 1 : -1;
                if (tnh != 0) return tnh;
                int tlh = this.c?.B >= other.c?.B ? this.c?.B == other.c?.B ? 0 : 1 : -1;
                return tlh;
            }

        }
        private Tree octreeHead;
        private SortedSet<Tree> sortedTrees = new SortedSet<Tree>();
        public WriteableBitmap ReduceColorsAfterConst(WriteableBitmap wbmp, int nrOfColors, Progress<int> pb)
        {
            WriteableBitmap reduced;
            reduced = new WriteableBitmap(wbmp);
            octreeHead = new Tree();
            sortedTrees = new SortedSet<Tree>();
            double prog = 0;
            double step = 40.0 / wbmp.PixelHeight;
            for (int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    InsertTree(octreeHead, wbmp.GetPixel(j, i), 0);
                }
                prog += step;
                ((IProgress<int>)pb).Report((int)prog);
            }

            reduceUsingDict(nrOfColors);
                prog += 20;
            ((IProgress<int>)pb).Report((int)prog);

            for (int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    reduced.SetPixel(j, i, findReducedColor(wbmp.GetPixel(j, i)));
                }
                prog += step;
                ((IProgress<int>)pb).Report((int)prog);
            }

            prog = 100;
            ((IProgress<int>)pb).Report((int)prog);

            return reduced;
        }
        internal WriteableBitmap ReduceColorsAlongConst(WriteableBitmap wbmp, int nrOfColors, Progress<int> pb) {
            WriteableBitmap reduced = new WriteableBitmap(wbmp);
            octreeHead = new Tree();
            sortedTrees = new SortedSet<Tree>();
            double prog = 0;
            double step = 50.0 / wbmp.PixelHeight;
            for(int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    InsertAndReduceTree(octreeHead, wbmp.GetPixel(j, i),0, nrOfColors);
                }
                prog += step;
                ((IProgress<int>)pb).Report((int)prog);
            }
            for(int i = 0; i < wbmp.PixelHeight; i++)
            {
                for (int j = 0; j < wbmp.PixelWidth; j++)
                {
                    reduced.SetPixel(j, i, findReducedColor(wbmp.GetPixel(j,i)));
                }
                prog += step;
                ((IProgress<int>)pb).Report((int)prog);
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
                sortedTrees.Remove(tree);
                tree.counter++;
                sortedTrees.Add(tree);
            }
            else
            {
                byte pos = GetOctalPosFromPixel(c, level);
                if (tree.next[pos] == null)
                {
                    AddLeaf(tree, c, level, pos);
                    if (octreeHead.childrenCounter > nrOfColors)
                    {
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
           while(octreeHead.childrenCounter > nrOfColors)
           {
                var leastPop = sortedTrees.Max;
                sortedTrees.Remove(leastPop);
                leastPop.parent.next[leastPop.ind] = null;
                octreeHead.childrenCounter--;
           }
        }

        private void Traverse(Queue<Tree> q, Stack<Tree> s, Tree tree)
        {
            q.Enqueue(tree);
            while(q.Count != 0)
            {
                Tree it = q.Dequeue();
                if (it.nonEpmtyBranchesCount == 0) continue;
                s.Push(it);
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
                sortedTrees.Remove(tree);
                tree.counter++;
                sortedTrees.Add(tree);
            }
            else
            {
                byte pos = GetOctalPosFromPixel(c, level);
                if (tree.next[pos] == null)
                {
                    AddLeaf(tree, c, level, pos);
                }
                InsertTree(tree.next[pos], c, ++level);
            }
        }

        private void AddLeaf(Tree tree, Color c, byte level, byte pos)
        {
            tree.next[pos] = new Tree();
            tree.nonEpmtyBranchesCount++;
            tree.next[pos].c = c;
            tree.next[pos].parent = tree;
            tree.next[pos].ind = pos;
            tree.next[pos].level = level + 1;
            tree.next[pos].counter++;
            octreeHead.childrenCounter++;
            sortedTrees.Add(tree.next[pos]);
        }

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
