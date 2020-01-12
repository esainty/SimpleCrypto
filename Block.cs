using System;

[Serializable]
public class Block {
    public int index;
    public string hash;
    public string previousHash;
    public int timestamp;
    public string data;
    public int difficulty; 
    public int nonce;

    public bool isComplete {get {
        if (hash != null && data != null) {
            return true;
        } else {
            return false;
        }
    }}

    public Block(int index, string previousHash, string hash, int timestamp, string data, int difficulty, int nonce) {
        this.index = index;
        this.previousHash = previousHash;
        this.hash = hash;
        this.timestamp = timestamp;
        this.data = data;
        this.difficulty = difficulty;
        this.nonce = nonce;
    }
}