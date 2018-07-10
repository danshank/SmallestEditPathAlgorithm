using System;
using System.Collections;
using System.Collections.Generic;

namespace EstimatingProducts.DiffAlgorithm
{
    public class SnakeNode
    {
        public Tuple<int, int> SnakeStart;
        public Tuple<int, int> SnakeEnd;
        public SnakeNode Left;
        public SnakeNode Right;

        public Queue<SnakeNode> GetEditQueue(Queue<SnakeNode> currentQueue)
        {
            if (!(Left is null))
            {
                currentQueue = Left.GetEditQueue(currentQueue);
            }
            currentQueue.Enqueue(this);
            if (!(Right is null))
            {
                currentQueue = Right.GetEditQueue(currentQueue);
            }
            return currentQueue;
        }
    }
}