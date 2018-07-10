using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EstimatingProducts.DiffAlgorithm
{
    public static class ListDifferenceFinder
    {
        // This algorithm is described in "An O(ND) Difference Algorithm and its Variations",
        // Eugene W. Meyers, 'Algorithmica' Vol. 1 No 2, 1986, pp. 251-266
        // More information about diff algorithms exist on stack overflow at
        // https://stackoverflow.com/questions/805626/diff-algorithm?rq=1


        public static Tuple<int, int, int, int, int> FindLongestMiddleSnake<T>(IList<T> oldKeys, IList<T> newKeys)
        {
            var listLengthDifference = oldKeys.Count - newKeys.Count;
            // The max edits corresponds to deleting all old keys, and inserting
            // all the new keys
            var maxEdits = oldKeys.Count + newKeys.Count;
            var forwardPathEndPoints = new int[maxEdits * 2 + 1];
            var backwardPathEndPoints = new int[maxEdits * 2 + 1];
            backwardPathEndPoints[maxEdits + listLengthDifference + 1] = oldKeys.Count + 1;
            for (var editLength = 0; editLength < ((oldKeys.Count + newKeys.Count) / 2) + 1; editLength++)
            {
                for (var k = -editLength; k <= editLength; k += 2)
                {
                    #region Furthest reaching forward D-path in diagonal k

                    // I need to shift the index that I use to 
                    // read and write to the arrays at this point, 
                    // since I'm not using negativately indexed arrays.
                    var zeroBasedDiagonal = k + maxEdits; 
                    int x;
                    // This if statement checks which edit path on the
                    // adjacent diagonals is longer, if both are the same,
                    // it defaults to the one on the left (corresponding
                    // to a deletion)
                    if (k == -editLength || k != editLength && forwardPathEndPoints[zeroBasedDiagonal - 1] < forwardPathEndPoints[zeroBasedDiagonal + 1])
                    {
                        x = forwardPathEndPoints[zeroBasedDiagonal + 1];
                    }
                    else
                    {
                        x = forwardPathEndPoints[zeroBasedDiagonal - 1] + 1;
                    }
                    var y = x - k;
                    while (x < oldKeys.Count && y < newKeys.Count && oldKeys[x].Equals(newKeys[y]))
                    {
                        x++;
                        y++;
                    }                    
                    forwardPathEndPoints[zeroBasedDiagonal] = x;

                    #endregion

                    if ((listLengthDifference % 2 == 1 || listLengthDifference % 2 == -1)  && k >= listLengthDifference - editLength + 1 && k <= listLengthDifference + editLength - 1)
                    {
                        if (forwardPathEndPoints[zeroBasedDiagonal] >= backwardPathEndPoints[zeroBasedDiagonal])
                        {
                            var shortestEditScriptLength = 2 * editLength - 1;
                            var middleSnakeStartRow = backwardPathEndPoints[zeroBasedDiagonal];
                            var middleSnakeStartColumn = middleSnakeStartRow - k;
                            var middleSnakeEndRow = forwardPathEndPoints[zeroBasedDiagonal];
                            var middleSnakeEndColumn = middleSnakeEndRow - k;
                            return new Tuple<int, int, int, int, int>(
                                shortestEditScriptLength, 
                                middleSnakeStartRow, 
                                middleSnakeStartColumn, 
                                middleSnakeEndRow, 
                                middleSnakeEndColumn
                            );
                        }
                    }
                }
                for (var k = -editLength; k <= editLength; k += 2)
                {
                    #region Furthest reaching backwards D-path in diagonal k + listLengthDifference

                    var diagonal = k + listLengthDifference;
                    var zeroBasedDiagonal = diagonal + maxEdits;
                    int x;
                    if (k == -editLength || k != editLength && backwardPathEndPoints[zeroBasedDiagonal - 1] <
                        backwardPathEndPoints[zeroBasedDiagonal] + 1)
                    {
                        x = backwardPathEndPoints[zeroBasedDiagonal + 1] - 1;
                    }
                    else
                    {
                        x = backwardPathEndPoints[zeroBasedDiagonal - 1];
                    }
                    var y = x - diagonal;
                    while (x > 0 && y > 0 && oldKeys[x - 1].Equals(newKeys[y - 1]))
                    {
                        x--;
                        y--;
                    }
                    backwardPathEndPoints[zeroBasedDiagonal] = x;

                    #endregion

                    if (listLengthDifference % 2 == 0 && diagonal >= -editLength && diagonal <= editLength)
                    {
                        if (forwardPathEndPoints[zeroBasedDiagonal] >= backwardPathEndPoints[zeroBasedDiagonal])
                        {
                            var shortestEditScriptLength = 2 * editLength;
                            var middleSnakeStartRow = backwardPathEndPoints[zeroBasedDiagonal];
                            var middleSnakeStartColumn = middleSnakeStartRow - diagonal;
                            var middleSnakeEndRow = forwardPathEndPoints[zeroBasedDiagonal];
                            var middleSnakeEndColumn = middleSnakeEndRow - diagonal;
                            return new Tuple<int, int, int, int, int>(
                                shortestEditScriptLength,
                                middleSnakeStartRow,
                                middleSnakeStartColumn,
                                middleSnakeEndRow,
                                middleSnakeEndColumn
                            );
                        }
                    }
                }
            }
            throw new Exception("This diff algorithm is not expected to reach this point, " +
                                "please check the data passed in and then " +
                                "either dig up resources on diff algorithms " +
                                "or bug Dan Shank about the algorithm's implementation if he's available. " +
                                "If you're not a programmer and you see this message something's gone horribly wrong.");
        }

        public static Queue<SnakeNode> CreateShortestEditQueue<T>(IList<T> oldKeys, IList<T> newKeys)
        {
            var shortestEditQueue = new Queue<SnakeNode>();
            var firstSnakeStart = new Tuple<int, int>(0, 0);
            var lastSnakeEnd = new Tuple<int, int>(oldKeys.Count, newKeys.Count);
            if (oldKeys.Count == 0 && newKeys.Count == 0)
            {
                // Do nothing to the shortest edit queue, no edits to be made.
            }
            else
            {
                // Create Shortest Edit Sequence Tree doesn't add the last and first edits to the tree,
                // since it needs to assume that those ends are parts of previous snakes during its
                // recursive calls. Therefore, I need to add them before I call the function, with some
                // special logic if either oldKeys or newKeys is empty.

                var shortestListLength = oldKeys.Count < newKeys.Count ? oldKeys.Count : newKeys.Count;
                var i = 0;
                while (i < shortestListLength && oldKeys[i].Equals(newKeys[i]))
                {
                    i++;
                }
                var firstEdit = new SnakeNode
                {
                    SnakeStart = firstSnakeStart,
                    SnakeEnd = new Tuple<int, int>(i, i)
                };
                if (i == oldKeys.Count && i == newKeys.Count)
                {
                    shortestEditQueue.Enqueue(firstEdit); // Both lists are equal, return a single edit that indicates that
                    return shortestEditQueue;
                }
                var j = 0;
                while (j < shortestListLength && oldKeys[oldKeys.Count - 1 - j].Equals(newKeys[newKeys.Count - 1 - j]))
                {
                    j++;
                }
                var lastEdit = new SnakeNode
                {
                    SnakeStart = new Tuple<int, int>(oldKeys.Count - j, newKeys.Count - j),
                    SnakeEnd = lastSnakeEnd
                };
                var middleEdits = CreateShortestEditSequenceTree(oldKeys, 0, newKeys, 0);
                shortestEditQueue.Enqueue(firstEdit);
                if (!(middleEdits is null))
                {
                    shortestEditQueue = middleEdits.GetEditQueue(shortestEditQueue);
                }
                shortestEditQueue.Enqueue(lastEdit);
            }
            return shortestEditQueue;
        }

        public static SnakeNode CreateShortestEditSequenceTree<T>(IList<T> oldKeys, int rowStart, IList<T> newKeys, int columnStart)
        {
            if (oldKeys.Count > 0 && newKeys.Count > 0)
            {
                var middleSnakeInfo = FindLongestMiddleSnake(oldKeys, newKeys);
                var editsNeeded = middleSnakeInfo.Item1;
                var x = middleSnakeInfo.Item2;
                var y = middleSnakeInfo.Item3;
                var u = middleSnakeInfo.Item4;
                var v = middleSnakeInfo.Item5;
                if (editsNeeded > 1)
                {
                    var leftTree = CreateShortestEditSequenceTree(oldKeys.Take(x).ToList(), rowStart,
                        newKeys.Take(y).ToList(), columnStart);
                    var rightTree =
                        CreateShortestEditSequenceTree(oldKeys.Skip(u).ToList(), u + rowStart, newKeys.Skip(v).ToList(), v + columnStart);
                    return new SnakeNode
                    {
                        Left = leftTree,
                        Right = rightTree,
                        SnakeStart = new Tuple<int, int>(x + rowStart, y + columnStart),
                        SnakeEnd = new Tuple<int, int>(u + rowStart, v + columnStart)
                    };
                }
                else
                {
                    return null;
                }
            }
            // Since we assume the endpoints of the edit graph are already included, there's no need to return anything here
            else if (oldKeys.Count <= 1 && newKeys.Count <= 1)
            {
                return null;
            }
            // This code executes when there's just insertions
            else if (oldKeys.Count == 0)
            {
                var middleKeyIndex = newKeys.Count / 2;
                var leftTree = CreateShortestEditSequenceTree(oldKeys, rowStart, newKeys.Take(middleKeyIndex).ToList(), columnStart);
                var rightTree = CreateShortestEditSequenceTree(oldKeys, rowStart, newKeys.Skip(middleKeyIndex).ToList(), columnStart + middleKeyIndex);
                return new SnakeNode
                {
                    Left = leftTree,
                    Right = rightTree,
                    SnakeStart = new Tuple<int, int>(rowStart, middleKeyIndex + columnStart),
                    SnakeEnd = new Tuple<int, int>(rowStart, middleKeyIndex + columnStart)
                };
            }
            // This code executes when there's just deletions
            else
            {
                var middleKeyIndex = oldKeys.Count / 2;
                var leftTree = CreateShortestEditSequenceTree(oldKeys.Take(middleKeyIndex).ToList(), rowStart, newKeys, columnStart);
                var rightTree = CreateShortestEditSequenceTree(oldKeys.Skip(middleKeyIndex).ToList(), rowStart + middleKeyIndex, newKeys, columnStart);
                return new SnakeNode
                {
                    Left = leftTree,
                    Right = rightTree,
                    SnakeStart = new Tuple<int, int>(middleKeyIndex + rowStart, columnStart),
                    SnakeEnd = new Tuple<int, int>(middleKeyIndex + rowStart, columnStart)
                };
            }
        }

        public static void ExecuteEditTree(Queue<SnakeNode> editQueue, Action<int, int> insert, Action<int> delete,
            Action<int, int> move)
        {
            while (editQueue.Count > 0)
            {
                var currentEdit = editQueue.Dequeue();
                if (currentEdit.SnakeStart.Item1 != currentEdit.SnakeEnd.Item1)
                {
                    for (var i = 0; i < currentEdit.SnakeEnd.Item1 - currentEdit.SnakeStart.Item1; i++)
                    {
                        move(currentEdit.SnakeStart.Item1 + i, currentEdit.SnakeStart.Item2 + i);
                    }
                }
                if (editQueue.Count == 0)
                {
                    return;
                }
                var nextEdit = editQueue.Peek();
                if (currentEdit.SnakeEnd.Item1 == nextEdit.SnakeStart.Item1)
                {
                    insert(nextEdit.SnakeStart.Item1, nextEdit.SnakeStart.Item2);
                }
                else
                {
                    delete(nextEdit.SnakeStart.Item1);
                }
            }
        }

        public static void PerformShortestEditSequence<T>(IList<T> oldKeys, IList<T> newKeys,
            Action<int, int> insert, Action<int> delete, Action<int, int> move)
        {
            var editQueue = CreateShortestEditQueue(oldKeys, newKeys);
            ExecuteEditTree(editQueue, insert, delete, move);
        }
    }
}