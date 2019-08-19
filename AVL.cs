using System;
using System.Collections;
using System.Collections.Generic;

namespace ASD.Graphs
{

    /// <summary>
    /// Drzewa AVL
    /// </summary>
    /// <typeparam name="TKey">Typ kluczy elementów przechowywanych w drzewie</typeparam>
    /// <typeparam name="TValue">Typ wartości elementów przechowywanych w drzewie</typeparam>
    /// <remarks>
    /// Wartości kluczy muszą być unikalne.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class AVL<TKey, TValue> : IAbstractDictionary<TKey, TValue>
    {
        private readonly IComparer<TKey> _comparer;

        public AVL(IComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        public AVL() : this(Comparer<TKey>.Default)
        {

        }

        private AVLNode Root { get; set; }

        public int Count { get; private set; }

        public bool Search(TKey key, out TValue value)
        {
            var node = Root;

            while (node != null)
            {
                if (_comparer.Compare(key, node.Key) < 0)
                {
                    node = node.Left;
                }
                else if (_comparer.Compare(key, node.Key) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    value = node.Value;

                    return true;
                }
            }

            value = default;

            return false;
        }

        public bool Insert(TKey key, TValue value)
        {
            var node = Root;

            while (node != null)
            {
                var compare = _comparer.Compare(key, node.Key);

                if (compare < 0)
                {
                    var left = node.Left;

                    if (left == null)
                    {
                        node.Left = new AVLNode { Key = key, Value = value, Parent = node };

                        InsertBalance(node, 1);
                        Count++;
                        return true;
                    }

                    node = left;
                }
                else if (compare > 0)
                {
                    var right = node.Right;

                    if (right == null)
                    {
                        node.Right = new AVLNode { Key = key, Value = value, Parent = node };

                        InsertBalance(node, -1);
                        Count++;
                        return true;
                    }

                    node = right;
                }
                else
                {
                    return false;
                }
            }

            Root = new AVLNode { Key = key, Value = value };
            Count++;
            return true;
        }

        private void InsertBalance(AVLNode node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                switch (balance)
                {
                    case 0:
                        return;
                    case 2:
                    {
                        if (node.Left.Balance == 1)
                        {
                            RotateRight(node);
                        }
                        else
                        {
                            RotateLeftRight(node);
                        }

                        return;
                    }

                    case -2:
                    {
                        if (node.Right.Balance == -1)
                        {
                            RotateLeft(node);
                        }
                        else
                        {
                            RotateRightLeft(node);
                        }

                        return;
                    }
                }

                var parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? 1 : -1;
                }

                node = parent;
            }
        }

        private AVLNode RotateLeft(AVLNode node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;

            right.Parent = parent;
            right.Left = node;
            node.Right = rightLeft;
            node.Parent = right;

            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }

            if (node == Root)
            {
                Root = right;
            }
            else if (parent.Right == node)
            {
                parent.Right = right;
            }
            else
            {
                parent.Left = right;
            }

            right.Balance++;
            node.Balance = -right.Balance;

            return right;
        }

        private AVLNode RotateRight(AVLNode node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;

            left.Parent = parent;
            left.Right = node;
            node.Left = leftRight;
            node.Parent = left;

            if (leftRight != null)
            {
                leftRight.Parent = node;
            }

            if (node == Root)
            {
                Root = left;
            }
            else if (parent.Left == node)
            {
                parent.Left = left;
            }
            else
            {
                parent.Right = left;
            }

            left.Balance--;
            node.Balance = -left.Balance;

            return left;
        }

        private AVLNode RotateLeftRight(AVLNode node)
        {
            var left = node.Left;
            var leftRight = left.Right;
            var parent = node.Parent;
            var leftRightRight = leftRight.Right;
            var leftRightLeft = leftRight.Left;

            leftRight.Parent = parent;
            node.Left = leftRightRight;
            left.Right = leftRightLeft;
            leftRight.Left = left;
            leftRight.Right = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }

            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }

            if (node == Root)
            {
                Root = leftRight;
            }
            else if (parent.Left == node)
            {
                parent.Left = leftRight;
            }
            else
            {
                parent.Right = leftRight;
            }

            switch (leftRight.Balance)
            {
                case -1:
                    node.Balance = 0;
                    left.Balance = 1;
                    break;
                case 0:
                    node.Balance = 0;
                    left.Balance = 0;
                    break;
                default:
                    node.Balance = -1;
                    left.Balance = 0;
                    break;
            }

            leftRight.Balance = 0;

            return leftRight;
        }

        private AVLNode RotateRightLeft(AVLNode node)
        {
            var right = node.Right;
            var rightLeft = right.Left;
            var parent = node.Parent;
            var rightLeftLeft = rightLeft.Left;
            var rightLeftRight = rightLeft.Right;

            rightLeft.Parent = parent;
            node.Right = rightLeftLeft;
            right.Left = rightLeftRight;
            rightLeft.Right = right;
            rightLeft.Left = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }

            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }

            if (node == Root)
            {
                Root = rightLeft;
            }
            else if (parent.Right == node)
            {
                parent.Right = rightLeft;
            }
            else
            {
                parent.Left = rightLeft;
            }

            switch (rightLeft.Balance)
            {
                case 1:
                    node.Balance = 0;
                    right.Balance = -1;
                    break;
                case 0:
                    node.Balance = 0;
                    right.Balance = 0;
                    break;
                default:
                    node.Balance = 1;
                    right.Balance = 0;
                    break;
            }

            rightLeft.Balance = 0;

            return rightLeft;
        }

        public bool Remove(TKey key)
        {
            var node = Root;

            while (node != null)
            {
                if (_comparer.Compare(key, node.Key) < 0)
                {
                    node = node.Left;
                }
                else if (_comparer.Compare(key, node.Key) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    var left = node.Left;
                    var right = node.Right;

                    if (left == null)
                    {
                        if (right == null)
                        {
                            if (node == Root)
                            {
                                Root = null;
                            }
                            else
                            {
                                var parent = node.Parent;

                                if (parent.Left == node)
                                {
                                    parent.Left = null;

                                    DeleteBalance(parent, -1);
                                }
                                else
                                {
                                    parent.Right = null;

                                    DeleteBalance(parent, 1);
                                }
                            }
                        }
                        else
                        {
                            Replace(node, right);

                            DeleteBalance(node, 0);
                        }
                    }
                    else if (right == null)
                    {
                        Replace(node, left);

                        DeleteBalance(node, 0);
                    }
                    else
                    {
                        var successor = right;

                        if (successor.Left == null)
                        {
                            var parent = node.Parent;

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;
                            left.Parent = successor;

                            if (node == Root)
                            {
                                Root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            DeleteBalance(successor, 1);
                        }
                        else
                        {
                            while (successor.Left != null)
                            {
                                successor = successor.Left;
                            }

                            var parent = node.Parent;
                            var successorParent = successor.Parent;
                            var successorRight = successor.Right;

                            if (successorParent.Left == successor)
                            {
                                successorParent.Left = successorRight;
                            }
                            else
                            {
                                successorParent.Right = successorRight;
                            }

                            if (successorRight != null)
                            {
                                successorRight.Parent = successorParent;
                            }

                            successor.Parent = parent;
                            successor.Left = left;
                            successor.Balance = node.Balance;
                            successor.Right = right;
                            right.Parent = successor;
                            left.Parent = successor;

                            if (node == Root)
                            {
                                Root = successor;
                            }
                            else
                            {
                                if (parent.Left == node)
                                {
                                    parent.Left = successor;
                                }
                                else
                                {
                                    parent.Right = successor;
                                }
                            }

                            DeleteBalance(successorParent, -1);
                        }
                    }
                    Count--;
                    return true;
                }
            }

            return false;
        }

        private void DeleteBalance(AVLNode node, int balance)
        {
            while (node != null)
            {
                balance = (node.Balance += balance);

                switch (balance)
                {
                    case 2 when node.Left.Balance >= 0:
                    {
                        node = RotateRight(node);

                        if (node.Balance == -1)
                        {
                            return;
                        }

                        break;
                    }

                    case 2:
                        node = RotateLeftRight(node);
                        break;
                    case -2 when node.Right.Balance <= 0:
                    {
                        node = RotateLeft(node);

                        if (node.Balance == 1)
                        {
                            return;
                        }

                        break;
                    }

                    case -2:
                        node = RotateRightLeft(node);
                        break;
                    default:
                    {
                        if (balance != 0)
                        {
                            return;
                        }

                        break;
                    }
                }

                var parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? -1 : 1;
                }

                node = parent;
            }
        }

        private static void Replace(AVLNode target, AVLNode source)
        {
            var left = source.Left;
            var right = source.Right;

            target.Balance = source.Balance;
            target.Key = source.Key;
            target.Value = source.Value;
            target.Left = left;
            target.Right = right;

            if (left != null)
            {
                left.Parent = target;
            }

            if (right != null)
            {
                right.Parent = target;
            }
        }

        public void SetAccess(Action access)
        {

        }

        public bool Modify(TKey key, TValue value)
        {
            var node = Root;

            while (node != null)
            {
                if (_comparer.Compare(key, node.Key) < 0)
                {
                    node = node.Left;
                }
                else if (_comparer.Compare(key, node.Key) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    node.Value = value;

                    return true;
                }
            }

            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new AVLEnumerator(Root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
                
        [Serializable]
        private sealed class AVLEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly AVLNode _root;
            private Action _action;
            private AVLNode _current;
            private AVLNode _right;

            public AVLEnumerator(AVLNode root)
            {
                _right = _root = root;
                _action = _root == null ? Action.End : Action.Right;
            }

            public bool MoveNext()
            {
                switch (_action)
                {
                    case Action.Right:
                        _current = _right;

                        while (_current.Left != null)
                        {
                            _current = _current.Left;
                        }

                        _right = _current.Right;
                        _action = _right != null ? Action.Right : Action.Parent;

                        return true;

                    case Action.Parent:
                        while (_current.Parent != null)
                        {
                            var previous = _current;

                            _current = _current.Parent;

                            if (_current.Left != previous) continue;
                            _right = _current.Right;
                            _action = _right != null ? Action.Right : Action.Parent;

                            return true;
                        }

                        _action = Action.End;

                        return false;

                    default:
                        return false;
                }
            }

            public void Reset()
            {
                _right = _root;
                _action = _root == null ? Action.End : Action.Right;
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_current.Key, _current.Value);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            private enum Action
            {
                Parent,
                Right,
                End
            }
        }

        [Serializable]
        private sealed class AVLNode
        {
            public AVLNode Parent;
            public AVLNode Left;
            public AVLNode Right;
            public TKey Key;
            public TValue Value;
            public int Balance;
        }
    }

}
