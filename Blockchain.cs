using System;
using System.Collections.Generic;

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
    
    public Blockchain() {
        string genesisHash = _generateBlockHash(0, "", 0, "Genesis", 0, 0);
        blockchain.Add(new Block(0, "0", genesisHash, 0, "Genesis", 0, 0));
    }

    

     public Block generateBlock(string blockData) {
        Block previousBlock = latestBlock;
        int nextIndex = previousBlock.index + 1;
        int nextDifficulty = _calculateDifficulty();
        Block nextBlock = _findValidBlock(nextIndex, previousBlock.hash, _timestamp, blockData, nextDifficulty);
        return nextBlock;
    }

    private static string _generateBlockHash(int index, string previousHash, int timestamp, string data, int difficulty, int nonce) {
        string toHash = index + previousHash + timestamp + data + difficulty + nonce;
        return Cryptography.SHA256HashString(toHash);
    }

    private static Block _findValidBlock(int index, string previousHash, int timestamp, string data, int difficulty) {
        bool hashValid = false;
        int nonce = 0;
        string hash = "";
        while (!hashValid) {
            hash = _generateBlockHash(index, previousHash, timestamp, data, difficulty, nonce);
            if (_isValidHash(hash, difficulty)) {
                hashValid = true;
            } else {
                nonce++;
            }
        }
        return new Block(index, previousHash, hash, timestamp, data, difficulty, nonce);
    }

    private int _calculateDifficulty() {
        if (latestBlock.index % DIFFICULTY_ADJUSTMENT_INTERVAL == 0 && latestBlock.index != 0) {
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

    private bool _newChainShouldReplaceExisting(List<Block> newChain) {
        if (_isValidChain(newChain)) {
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

    private static bool _isValidHash(string hash, int difficulty) {
        string comparator = "";
        for (int i = 0; i < difficulty; i++) {
            comparator += '0';
        }
        return hash.StartsWith(comparator) ? true : false;
    }

    private static bool _isValidBlock(Block newBlock, Block previousBlock) {
        if (newBlock.index != previousBlock.index - 1) {
            Console.WriteLine("New block has invalid index");
            return false;
        }
        if (newBlock.previousHash != previousBlock.hash) {
            Console.WriteLine("New block has invalid previous hash");
            return false;
        }
        if (_generateBlockHash(newBlock.index, newBlock.previousHash, newBlock.timestamp, newBlock.data, newBlock.difficulty, newBlock.nonce) != newBlock.hash) {
            Console.WriteLine("New block has invalid hash");
        }
        return true;
    }

    private bool _isValidChain(List<Block> chain) {
        if (!Equals(chain[0], blockchain[0])) {
            return false;
        }
        for (int i = 1; i < chain.Count; i++) {
            if (!_isValidBlock(chain[i], chain[i-1])) {
                return false;
            }
        }
        return true; 
    }
}