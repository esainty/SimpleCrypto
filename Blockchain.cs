using System;
using System.Collections.Generic;
public class Blockchain {
    public List<Block> blockchain;
    public Block latestBlock {get {
        Console.WriteLine("latestBlock called");
        return blockchain[blockchain.Count-1];
    }}
    
    private int _timestamp {get {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }}
    
    public Blockchain() {
        string genesisHash = _generateBlockHash(0, "", 0, "Genesis");
        blockchain.Add(new Block(0, "0", genesisHash, 0, "Genesis"));
    }

    public bool replaceChain(List<Block> newChain) {
        if (_isValidChain(newChain) && newChain.Count > blockchain.Count) {
            blockchain = newChain;
            return true; 
        } else {
            Console.WriteLine("New chain received is invalid or of insufficient length");
            return false;
        }
    }

     private Block _generateBlock(string blockData) {
        Block previousBlock = latestBlock;
        int nextIndex = previousBlock.index + 1;
        int nextTimestamp = _timestamp;
        string nextHash = _generateBlockHash(nextIndex, previousBlock.hash, nextTimestamp, blockData);
        return new Block(nextIndex, previousBlock.hash, nextHash, nextTimestamp, blockData);
    }

    private string _generateBlockHash(int index, string previousHash, int timestamp, string data) {
        string toHash = index + previousHash + timestamp + data;
        return Cryptography.hashStringSHA256(toHash);
    }

    private bool _isValidBlock(Block newBlock, Block previousBlock) {
        if (newBlock.index != previousBlock.index - 1) {
            Console.WriteLine("New block has invalid index");
            return false;
        }
        if (newBlock.previousHash != previousBlock.hash) {
            Console.WriteLine("New block has invalid previous hash");
            return false;
        }
        if (_generateBlockHash(newBlock.index, newBlock.previousHash, newBlock.timestamp, newBlock.data) != newBlock.hash) {
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