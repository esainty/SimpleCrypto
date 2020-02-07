using System.Collections.Generic;

namespace SimpleCrypto {
    public class TransactionManager {
        SortedSet<Transaction> txPool;

        public TransactionManager() {
            // txPool = new SortedSet<Transaction>(transactions, Comparer<Transaction>.Create((x, y) => {
            //     x.
            // }));
        }

        public Transaction[] getBestTransactions(int count) {
            Transaction[] txs = {txPool.GetEnumerator().Current};
            return txs;
        }
    }
}