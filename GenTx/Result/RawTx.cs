using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.RPC;
using System.Net.Http;

namespace GenTx
{
    internal class RawTx
    {
        private static async Task<string?> GetTxData(string tx)
        {
            var httpClient = new HttpClient();
            var apiCall = await httpClient.GetAsync("https://blockchain.info/rawtx/" + tx + "?format=hex");
            var asStr = await apiCall.Content.ReadAsStringAsync();
            return asStr;
        }
        public static byte[] CreateTx(TxModel txModel)
        {
            var bestUtxosForTx = PickBestUTXOs(txModel.Utxos, txModel.GetTotalAmount());

            var tx = Transaction.Create(txModel.Network);

            var par = GenerateOutpoints(tx, bestUtxosForTx);
            GenerateMoney(tx, bestUtxosForTx, txModel);

            PSBT psbt = PSBT.FromTransaction(tx, Network.Main);
            psbt.AddTransactions(par);
            Console.WriteLine(psbt.IsReadyToSign());
            return psbt.ToBytes();
        }
        private static IUTXO[] PickBestUTXOs(IUTXO[] utxos, decimal totalAmount)
        {
            decimal total = 0;
            var bestUtxos = new List<IUTXO>();
            foreach (var utxo in utxos)
            {
                if (total >= totalAmount) break;
                total += utxo.GetValue();
                bestUtxos.Add(utxo);
            }
            return bestUtxos.ToArray();
        }

        private static Transaction[] GenerateOutpoints(Transaction tx, IUTXO[] utxos)
        {
            var parTxs = new Transaction[utxos.Length];

            for (int i = 0; i < utxos.Length; i++)
            {
                var utxo = utxos[i];

                var t = GetTxData(utxo.GetTxId());
                t.Wait();

                parTxs[i] = Transaction.Parse(t.Result, Network.Main);
                tx.Inputs.Add(parTxs[i], (int)utxo.GetOutputNo());
            }
            return parTxs;
        }

        private static void GenerateMoney(Transaction tx, IUTXO[] bestUtxosForTx, TxModel txModel)
        {
            decimal rest = TotalBalanceInBestUtxos(bestUtxosForTx) - txModel.GetTotalAmount();
            var moneyToSend = new Money(txModel.Amount, MoneyUnit.BTC);

            if (rest > 0)
            {
                var restMoney = new Money(rest, MoneyUnit.BTC);
                tx.Outputs.Add(moneyToSend, txModel.DestinationAddr.ScriptPubKey);
                tx.Outputs.Add(restMoney, txModel.Address.ScriptPubKey);
                SignInputs(tx, bestUtxosForTx, txModel);
                return;
            }

            tx.Outputs.Add(moneyToSend, txModel.DestinationAddr.ScriptPubKey);
            SignInputs(tx, bestUtxosForTx, txModel);
        }

        private static decimal TotalBalanceInBestUtxos(IUTXO[] utxos)
        {
            decimal total = 0;
            foreach (var utxo in utxos)
            {
                total += utxo.GetValue();
            }
            return total;
        }

        private static void SignInputs(Transaction tx, IUTXO[] utxos, TxModel txModel)
        {
            for (int i = 0; i < utxos.Length; i++)
            {
                tx.Inputs[i].ScriptSig = txModel.Address.ScriptPubKey;
            }
        }

        private static void SignTx(Transaction tx, IUTXO[] utxos, TxModel txModel)
        {
            var coins = new List<ICoin>();
            foreach (var utxo in utxos)
            {
                var txInString = uint256.Parse(utxo.GetTxId());
                var coin = new Coin(txInString, utxo.GetOutputNo(), new Money(utxo.GetValue(), MoneyUnit.BTC),
                                    txModel.Address.ScriptPubKey);
                coins.Add(coin);
            }
            tx.Sign(txModel.Key.GetWif(txModel.Network), coins);
        }

    }
}
