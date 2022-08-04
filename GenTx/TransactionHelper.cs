using System;
using System.Threading.Tasks;
using RestSharp;
using System.Net.Http;
using NBitcoin;
using System.Collections.Generic;

namespace GenTx
{
    class TransactionHelper
    {
        public static readonly string UTXO_ENDPOINT_SOCHAIN = "https://sochain.com/api/v2/get_tx_unspent/BTC/";
        public static readonly string UTXO_ENDPOINT_BLOCKCHAIN = "https://blockchain.info/unspent?active=";

        public static readonly string SEND_TX_BLOCKCHAIN = "https://blockchain.info/pushtx";
        public static readonly string SEND_TX_SOCHAIN = "https://sochain.com/api/v2/send_tx/BTC";

        public static async Task<decimal> GetTotalBalance(string address, bool useBlockchainDotCom = true)
        {
            var utxos = await TransactionHelper.FetchUTXOsAsync(address, useBlockchainDotCom);

            //Console.WriteLine(utxos);

            decimal total = 0m;
            if (utxos != null)
            {
                foreach (var utxo in utxos)
                {
                    total += utxo.GetValue();
                }
            }
            return total;
        }

        public static async Task<IUTXO[]?> FetchUTXOsAsync(string address, bool useBlockchainDotCom = true)
        {
            var _httpClient = new HttpClient();

            if (useBlockchainDotCom)
            {
                // try to receive data from blockchain.com
                var apiCall = await _httpClient.GetAsync(UTXO_ENDPOINT_BLOCKCHAIN + address);
                var asStr = await apiCall.Content.ReadAsStringAsync();
                var reply = System.Text.Json.JsonSerializer.Deserialize<BlockchainReply>(asStr);
                return reply?.unspent_outputs;
            }
            else
            {
                // failure: try sochain.com
                var apiCall = await _httpClient.GetAsync(UTXO_ENDPOINT_SOCHAIN + address);
                var asStr = await apiCall.Content.ReadAsStringAsync();
                var reply = System.Text.Json.JsonSerializer.Deserialize<SochainReply>(asStr);
                return reply?.data?.txs;
            }
        }

        private static string? TxInputValidation(TxModel txModel)
        {
            if (txModel.Amount < 0.00000001m) return "The smallest transfer amount must be greater or equal to 0.00000001";
            if (txModel.Fee < 0.00000001m) return "The smallest fee must be greater or equal to 0.00000001";
            if (txModel.DestinationAddr == txModel.Address) return "You cannot send a transaction to your own address";

            return null;
        }
        private static bool HaveEnoughFunds(TxModel txModel)
        {
            decimal total = 0m;
            foreach (var utxo in txModel.Utxos)
            {
                total += utxo.GetValue();
            }
            return total >= txModel.GetTotalAmount();
        }

        private static int CompareUTXO(IUTXO a, IUTXO b)
        {
            if (a.GetValue() > b.GetValue()) return -1;
            else if (a.GetValue() < b.GetValue()) return 1;
            else return 0;
        }

        public static async Task<byte[]?> GenerateTxAsync(
            string sender, string recipient, decimal amount, decimal fee, bool useBlockchainDotCom = true
        )
        {

            var utxos = await TransactionHelper.FetchUTXOsAsync(sender, useBlockchainDotCom);

            Array.Sort(utxos, CompareUTXO);

            if (utxos is null)
            {
                Console.WriteLine("Cannot Fetch UTXOs.");
                return null;
            }

            var txModel = new TxModel(Network.Main, sender, utxos, recipient, amount, fee);

            if (TxInputValidation(txModel) != null)
            {
                Console.WriteLine(TxInputValidation(txModel));
                return null;
            }
            if (!HaveEnoughFunds(txModel))
            {
                Console.WriteLine("Insufficient Funds.");
                return null;
            }
            var tx = RawTx.CreateTx(txModel);
            return tx;
        }

        public static async Task<string?> BroadcastRawTxAsync(string txHex, bool useBlockchainDotCom = true)
        {
            if (string.IsNullOrEmpty(txHex)) return null;
            if (useBlockchainDotCom)
            {
                HttpClient client = new HttpClient();

                HttpContent content = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("tx", txHex)
                });

                var result = await client.PostAsync(SEND_TX_BLOCKCHAIN, content);
                var resultStr = await result.Content.ReadAsStringAsync();

                return resultStr;
            } else
            {
                var client = new RestClient(SEND_TX_SOCHAIN);

                var request = new RestRequest("", Method.Post);
                request.AddJsonBody(new { tx_hex = txHex });

                var response = await client.ExecuteAsync(request);

                var txHash = System.Text.Json.JsonSerializer.Deserialize<TxData>(response.Content);

                return txHash?.data?.txid;
            }
        }
    }
}
