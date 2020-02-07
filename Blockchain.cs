using System;
using System.Collections.Generic;

namespace SimpleCrypto {
    [Serializable]
    public class Blockchain {
        public List<Block> blockchain = new List<Block>();

        public const int BLOCK_GENERATION_INTERVAL = 10;
        public const int DIFFICULTY_ADJUSTMENT_INTERVAL = 10;

        public Block latestBlock {get {
            Console.WriteLine("latestBlock called");
            return blockchain[blockchain.Count-1];
        }}
        
        private static int _timestamp {get {
            return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }}
        
        public Blockchain(Block genesisBlock) {
            blockchain.Add(genesisBlock);
        }

        public Block generateBlock(BlockData blockData) {
            Block previousBlock = latestBlock;
            int nextIndex = previousBlock.height + 1;
            int nextDifficulty = _calculateDifficulty();
            Block nextBlock = new Block(nextIndex, previousBlock.hash, _timestamp, blockData, nextDifficulty);
            return nextBlock;
        }

        private int _calculateDifficulty() {
            if (latestBlock.height % DIFFICULTY_ADJUSTMENT_INTERVAL == 0 && latestBlock.height != 0) {
                Block previousAdjustmentBlock = blockchain[blockchain.Count - DIFFICULTY_ADJUSTMENT_INTERVAL];
                int expectedTime = BLOCK_GENERATION_INTERVAL * DIFFICULTY_ADJUSTMENT_INTERVAL;
                // !!** Check this, might be one index off **!!
                int actualTime = latestBlock.timestamp - previousAdjustmentBlock.timestamp;
                if (actualTime < expectedTime / 2) {
                    return previousAdjustmentBlock.difficulty + 1;
                } else if (actualTime > expectedTime * 2) {
                    return previousAdjustmentBlock.difficulty - 1;
                } else {
                    return previousAdjustmentBlock.difficulty;
                }
            } else {
                return latestBlock.difficulty;
            }
        }

        private static double _calculateCumulativeWork(List<Block> chain) {
            double cumulativeWork = 0;
            foreach (Block block in chain) {
                cumulativeWork += Math.Pow(2, block.difficulty);
            }
            return cumulativeWork;
        }

        private bool _newChainShouldReplaceExisting(List<Block> newChain, UTxOut[] uTxOuts) {
            if (_isValidChain(newChain, uTxOuts)) {
                double cumulativeWorkOld = _calculateCumulativeWork(blockchain);
                double cumulativeWorkNew = _calculateCumulativeWork(newChain);
                if (cumulativeWorkNew > cumulativeWorkOld) {
                    return true;
                } else {
                    return false;
                }
            } else {
                return false;
            }
        }

        private static bool _isValidTimestamp(int previousTimestamp, int newTimestamp) {
            // Returns true if 
            // new timestamp is at most one minute prior to previous timestamp
            // new timestamp is at most one minute ahead of current time
            return (previousTimestamp - 60 < newTimestamp && newTimestamp - 60 < _timestamp) 
                ? true : false;
        }

        private bool _isValidChain(List<Block> chain, UTxOut[] uTxOuts) {
            if (!Equals(chain[0], blockchain[0])) {
                return false;
            }
            for (int i = 1; i < chain.Count; i++) {
                if (!chain[i].isValidBlock(chain[i-1], uTxOuts)) {
                    return false;
                }
            }
            return true; 
        }
    }
}