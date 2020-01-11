using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleCrypto {
    class Program {
        static async Task Main(string[] args) {
            BlockchainManager bcManager = new BlockchainManager();
            await bcManager.initialise();
        }
    }
}
