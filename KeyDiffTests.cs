using System;
using System.Collections.Generic;
using EstimatingProducts.DiffAlgorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ProductUnitTests
{
    [TestClass]
    public class KeyDiffTests
    {
        [TestMethod]
        public void SimpleDiff()
        {
            var current = new List<string> {"Hello"};
            var next = new List<string>{"Goodbye"};
            var points = new List<int[]>
            {
                new [] {0, 0},
                new [] {0, 1},
                new [] {1, 1}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void CreateOne()
        {
            var current = new List<string>();
            var next = new List<string>{"a"};
            var points = new List<int[]>
            {
                new [] {0, 0},
                new [] {0, 1}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void OneInsert()
        {
            var current = new List<string>{"a"};
            var next = new List<string>{"a", "b"};
            var points = new List<int[]>
            {
                new [] {0, 0, 1, 1},
                new [] {1, 2}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void OneDeletion()
        {
            var current = new List<string>{"a", "b"};
            var next = new List<string>{"a"};
            var points = new List<int[]>
            {
                new [] {0, 0, 1, 1},
                new[] {2, 1}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void OnlyInsertions()
        {
            var current = new List<string>();
            var next = new List<string>{"a", "b", "c", "d"};
            var points = new List<int[]>
            {
                new [] {0, 0},
                new [] {0, 1},
                new [] {0, 2},
                new [] {0, 3},
                new [] {0, 4}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void OnlyDeletions()
        {
            var current = new List<string>{"a", "b", "c"};
            var next = new List<string>();
            var points = new List<int[]>
            {
                new [] {0, 0},
                new [] {1, 0},
                new [] {2, 0},
                new [] {3, 0}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        [TestMethod]
        public void ComplexChangeTest()
        {
            var current = new List<string> { "a", "b", "c", "c", "d", "e", "f", "f", "g" };
            var next = new List<string>{"c", "d", "f", "a", "b", "d", "g", "f", "g", "g"};
            var points = new List<int[]>
            {
                new [] {0, 0},
                new [] {0, 1},
                new [] {0, 2},
                new [] {0, 3, 2, 5},
                new [] {3, 5},
                new [] {4, 5, 5, 6},
                new [] {6, 6},
                new [] {7, 6},
                new [] {7, 7, 8, 8},
                new [] {8, 9, 9, 10}
            };
            var expectedDiffs = CreateQueueFromCoordList(points);
            var actualDiffs = ListDifferenceFinder.CreateShortestEditQueue(current, next);
            Assert.IsTrue(EditQueuesAreEqual(expectedDiffs, actualDiffs));
        }

        public bool EditQueuesAreEqual(Queue<EditTree> expected, Queue<EditTree> actual)
        {
            if (expected.Count != actual.Count)
            {
                return false;
            }
            for (var i = 0; i < expected.Count; i++)
            {
                var expectedEdit = expected.Dequeue();
                var actualEdit = actual.Dequeue();
                if (
                    expectedEdit.SnakeStart.Item1 != actualEdit.SnakeStart.Item1
                    || expectedEdit.SnakeStart.Item2 != actualEdit.SnakeStart.Item2
                    || expectedEdit.SnakeEnd.Item1 != actualEdit.SnakeEnd.Item1
                    || expectedEdit.SnakeEnd.Item2 != actualEdit.SnakeEnd.Item2
                )
                {
                    return false;
                }
            }
            return true;
        }

        public Queue<EditTree> CreateQueueFromCoordList(List<int[]> coords)
        {
            var queue = new Queue<EditTree>();
            foreach (var coordArray in coords)
            {
                var snakeStart = new Tuple<int, int>(coordArray[0], coordArray[1]);
                Tuple<int, int> snakeEnd;
                if (coordArray.Length == 4)
                {
                    snakeEnd = new Tuple<int, int>(coordArray[2], coordArray[3]);
                }
                else
                {
                    snakeEnd = snakeStart;
                }
                var edit = new EditTree{SnakeStart = snakeStart, SnakeEnd = snakeEnd};
                queue.Enqueue(edit);
            }
            return queue;
        }
    }
}
