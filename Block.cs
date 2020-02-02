using System;
using System.IO;
using System.Linq;
using System.Collections;

namespace SimpleCrypto {

    [Serializable]
    public class Block {
        public int index;
        public byte[] hash;
        public byte[] previousHash;
        public int timestamp;
        public BlockData data;
        public int difficulty; 
        public int nonce;

        public bool isComplete {get {
            if (hash != null && data != null) {
                return true;
            } else {
                return false;
            }
        }}

        public Block(int index, byte[] previousHash, int timestamp, BlockData data, int difficulty) {
            this.index = index;
            this.previousHash = previousHash;
            this.timestamp = timestamp;
            this.data = data;
            this.difficulty = difficulty;
        }

        public byte[] generateHash() {
            byte[] toHash = BitConverter.GetBytes(index)
                .Concat(previousHash)
                .Concat(BitConverter.GetBytes(timestamp))
                .Concat(data.getBytes())
                .Concat(BitConverter.GetBytes(difficulty))
                .Concat(BitConverter.GetBytes(nonce)).ToArray();
            return Cryptography.SHA256HashBytes(toHash);
        }

        public void mineBlockHash() {
            bool hashValid = false;
            int nonce = 0;
            byte[] hash = generateHash();
            while (!hashValid) {
                if (_isValidHash(hash, difficulty)) {
                    hashValid = true;
                } else {
                    nonce++;
                    hash = generateHash();
                }
            }
            this.hash = hash;
            this.nonce = nonce;
        }

        private static bool _isValidHash(byte[] hash, int difficulty) {
            BitArray hashBits = new BitArray(hash);
            if (difficulty > hashBits.Length) {
                throw new IndexOutOfRangeException("Difficulty exceeds total length of hash");
            }

            // Return false if any bits in first [difficulty] bits are 1
            for (int i = 0; i < difficulty; i++) {
                if (hashBits[i]) {
                    return false;
                }
            }
            return true;
        }

        public bool isValidBlock(Block previousBlock) {
            if (index != previousBlock.index - 1) {
                Console.WriteLine("New block has invalid index");
                return false;
            }
            if (previousHash != previousBlock.hash) {
                Console.WriteLine("New block has invalid previous hash");
                return false;
            }
            if (generateHash() != hash) {
                Console.WriteLine("New block's hash does not match content hash");
                return false;
            }
            if (!_isValidHash(hash, difficulty)) {
                Console.WriteLine("New block hash does not meet difficulty requirements");
                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class BlockData {
        public CoinbaseTransaction coinbaseTransaction;
        public Transaction[] transactions;

        public BlockData(CoinbaseTransaction coinbaseTransaction, Transaction[] transactions, int blockHeight) {
            this.coinbaseTransaction = coinbaseTransaction;
            this.transactions = transactions;
            if (!isValidData(blockHeight)) {
                throw new InvalidDataException("Data for block is invalid.");
            }
        }

        public bool isValidData(int blockHeight) {
            // Check that coinbase and all regular transactions are valid. 
            if (!coinbaseTransaction.isValidCoinbaseTransaction(blockHeight)) {
                return false;
            } 
            foreach (Transaction tx in transactions) {
                if(!tx.isValidTransaction()) {
                    return false;
                }
            }
            return true;
        }

        public byte[] getBytes() {
            byte[] bytes = coinbaseTransaction.getBytes();
            foreach (Transaction tx in transactions) {
                bytes.Concat(tx.getBytes());
            }
            return bytes;
        }
    }
}
