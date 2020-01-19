using System;
using SimpleWebsocket;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

class BlockchainManager {

    Blockchain blockchain = new Blockchain();
    CancellationTokenSource cts = new CancellationTokenSource();


    public BlockchainManager() {
        
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
                    BinaryFormatter formatter = new BinaryFormatter();
                    byte[] byteArray = new byte[req.ContentLength64];
                    await req.InputStream.ReadAsync(byteArray, 0, (int)req.ContentLength64);
                    string data = Encoding.ASCII.GetString(byteArray);
                    // string data = (string)formatter.Deserialize(req.InputStream);
                    blockchain.blockchain.Add(blockchain.generateBlock(data));
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