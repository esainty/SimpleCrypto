using System;
using SimpleWebsocket;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleCrypto {
    class BlockchainManager {
        Blockchain blockchain;
        TransactionManager transactionManager;
        List<UTxOut> unspentTransactions;
        CancellationTokenSource cts = new CancellationTokenSource();

        public BlockchainManager() {
            KeyPair genesisKeyPair = new KeyPair();
            BlockData genesisBlockData = new BlockData(
                new CoinbaseTransaction(genesisKeyPair.publicKey, 0),
                new Transaction[] {},
                0
            );
            blockchain = new Blockchain(new Block(0, new byte[]{}, 0, genesisBlockData, 1));
            transactionManager = new TransactionManager();
            unspentTransactions = new List<UTxOut>();
        }

        public async Task initialise() {
            WebsocketServer server = new WebsocketServer();
            server.addRoutes(
                HttpHandler.createRoute("/blocks", async (HttpListenerRequest req, HttpListenerResponse res) => {
                    // Returns blockchain C# OBJECT as serialized datastream.
                    if (req.HttpMethod == "GET") {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(res.OutputStream, blockchain);
                        await res.OutputStream.DisposeAsync();
                        return 200;
                    } else if (req.HttpMethod == "POST") {
                        BinaryFormatter formatter = new BinaryFormatter();
                        blockchain = (Blockchain)formatter.Deserialize(req.InputStream);
                        res.StatusCode = 200;
                        await res.OutputStream.DisposeAsync();
                        return 200;
                    } else {
                        // Wrong HTTP Method
                        await HttpHandler.sendErrorResponseAsync(res, 405);
                        return 405;
                    }
                }),
                HttpHandler.createRoute("/mineblock", async (HttpListenerRequest req, HttpListenerResponse res) => {
                    if (req.HttpMethod == "POST") {
                        // Read in request
                        BinaryFormatter formatter = new BinaryFormatter();
                        byte[] byteArray = new byte[req.ContentLength64];
                        await req.InputStream.ReadAsync(byteArray, 0, (int)req.ContentLength64);

                        // Generate new block data
                        KeyPair keys = new KeyPair();
                        CoinbaseTransaction ctx = new CoinbaseTransaction(keys.publicKey, blockchain.blockchain.Count);
                        Transaction[] transactions = transactionManager.getBestTransactions(10);
                        BlockData data = new BlockData(ctx, transactions, blockchain.blockchain.Count);

                        // Attempt to find valid hash for block.
                        blockchain.blockchain.Add(blockchain.generateBlock(data));

                        // Serialise block and return to requester. 
                        formatter.Serialize(res.OutputStream, blockchain.blockchain[blockchain.blockchain.Count - 1]);
                        await res.OutputStream.DisposeAsync();
                        return 200;
                    } else {
                        return 405;
                    }
                })
            );

            Task runningServer = server.startServerAsync();
            await runningServer;
        }
    }
}