using System;
using SimpleWebsocket;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

class BlockchainManager {

    Blockchain blockchain = new Blockchain();

    public BlockchainManager() {
        
    }

    public async Task initialise() {
        WebsocketServer server = new WebsocketServer();
        server.addRoutes(
            HttpHandler.createRoute("/blocks", async (HttpListenerRequest req, HttpListenerResponse res) => {
                // Returns blockchain C# OBJECT as serialized datastream.
                if (req.HttpMethod == "GET") {
                    BinaryFormatter formatter = new BinaryFormatter();
                    // byte[] data;
                    // using (MemoryStream stream = new MemoryStream()) {
                    //     formatter.Serialize(stream, blockchain);
                    //     data = stream.ToArray();
                    // }
                    // await HttpHandler.sendResponseAsync(res, data);
                    formatter.Serialize(res.OutputStream, blockchain);
                    await res.OutputStream.DisposeAsync();
                    return 200;
                } else if (req.HttpMethod == "POST") {
                    byte[] data = new byte[req.ContentLength64];
                    //await req.InputStream.ReadAsync(data, 0, 0);
                    BinaryFormatter formatter = new BinaryFormatter();
                    blockchain = (Blockchain)formatter.Deserialize(req.InputStream);
                    return 200;
                } else {
                    // Wrong HTTP Method
                    await HttpHandler.sendErrorResponseAsync(res, 405);
                    return 405;
                }
            })
        );
        
        Task runningServer = server.startServerAsync();
        await runningServer;
    }
}