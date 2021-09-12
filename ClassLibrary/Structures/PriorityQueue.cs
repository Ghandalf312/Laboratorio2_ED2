using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CustomGenerics.Structures
{
    public class PriorityQueue<T>
    {
        PQNode<T> Root;
        public int DataNumber;

        public PriorityQueue()
        {
            DataNumber = 0;
        }

        private bool IsEmpty() { return Root == null; }

        public void AddValue(T value, double priority)
        {
            if (IsEmpty())
            {
                Root = new PQNode<T>(value, priority);
                DataNumber = 1;
            }
            else
            {
                DataNumber++;
                Add(new PQNode<T>(value, priority), priority);
            }
        }

        private void Add(PQNode<T> newNode, double priority)
        {
            var NewNodeFather = SearchLastNode(Root, 1);
            if (NewNodeFather.LeftSon != null)
            {
                NewNodeFather.RightSon = newNode;
                newNode.Father = NewNodeFather;
                OrderDownToUp(newNode);
            }
            else
            {
                NewNodeFather.LeftSon = newNode;
                newNode.Father = NewNodeFather;
                OrderDownToUp(newNode);
            }
        }

        private PQNode<T> SearchLastNode(PQNode<T> currentNode, int number)
        {
            try
            {
                int previousn = DataNumber;
                if (previousn == number)
                {
                    return currentNode;
                }
                else
                {
                    while (previousn / 2 != number)
                    {
                        previousn = previousn / 2;
                    }
                    if (previousn % 2 == 0)
                    {
                        if (currentNode.LeftSon != null)
                        {
                            return SearchLastNode(currentNode.LeftSon, previousn);
                        }
                        else
                        {
                            return currentNode;
                        }
                    }
                    else
                    {
                        if (currentNode.RightSon != null)
                        {
                            return SearchLastNode(currentNode.RightSon, previousn);
                        }
                        else
                        {
                            return currentNode;
                        }
                    }
                }
            }
            catch
            {
                return currentNode;
            }
        }


        public T GetFirst()
        {
            if (Root == null)
            {
                return default;
            }
            PQNode<T> LastNode = new PQNode<T>();
            LastNode = SearchLastNode(Root, 1);
            PQNode<T> FirstNode = (PQNode<T>)Root.Clone();
            var LastNodeCopy = (PQNode<T>)LastNode.Clone();
            Root.Value = LastNodeCopy.Value;
            Root.Priority = LastNodeCopy.Priority;
            if (LastNode.Father == null)
            {
                Root = null;
                DataNumber--;
                return LastNodeCopy.Value;
            }
            else
            {
                if (LastNode.Father.LeftSon == LastNode)
                {
                    LastNode.Father.LeftSon = null;
                }
                else
                {
                    LastNode.Father.RightSon = null;
                }
            }
            OrderUpToDown(Root);
            DataNumber--;
            return FirstNode.Value;
        }

        private void OrderDownToUp(PQNode<T> currentNode)
        {
            if (currentNode.Father != null)
            {
                if (currentNode.Priority < currentNode.Father.Priority)
                {
                    ChangeValues(currentNode);
                }
                OrderDownToUp(currentNode.Father);
            }
        }

        private void OrderUpToDown(PQNode<T> currentNode)
        {
            if (currentNode.LeftSon != null && currentNode.RightSon != null)
            {
                if (currentNode.LeftSon.Priority > currentNode.RightSon.Priority)
                {
                    if (currentNode.Priority > currentNode.RightSon.Priority)
                    {
                        ChangeValues(currentNode.RightSon);
                        OrderUpToDown(currentNode.RightSon);
                    }
                }
                else
                {
                    if (currentNode.Priority > currentNode.LeftSon.Priority)
                    {
                        ChangeValues(currentNode.LeftSon);
                        OrderUpToDown(currentNode.LeftSon);
                    }
                }
            }
            else if (currentNode.LeftSon != null)
            {
                if (currentNode.Priority > currentNode.LeftSon.Priority)
                {
                    ChangeValues(currentNode.LeftSon);
                    OrderUpToDown(currentNode.LeftSon);
                }
            }
        }

        private void ChangeValues(PQNode<T> currentNode)
        {
            var Priority1 = currentNode.Priority;
            var Value1 = currentNode.Value;
            currentNode.Priority = currentNode.Father.Priority;
            currentNode.Value = currentNode.Father.Value;
            currentNode.Father.Priority = Priority1;
            currentNode.Father.Value = Value1;
        }
    }
}
