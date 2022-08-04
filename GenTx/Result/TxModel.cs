using NBitcoin;

namespace GenTx
{
    public class TxModel
    {
        public Network Network { get; set; }
        public BitcoinAddress Address { get; set; }
        public Key Key { get; set; }
        public IUTXO[] Utxos { get; set; }
        public BitcoinAddress DestinationAddr { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }

        public TxModel(Network network, Key key, string address, IUTXO[] utxos, string DestinationAddr, decimal amount, decimal fee)
        {
            this.Network = network;
            this.Key = key;
            this.Address = BitcoinAddress.Create(address, Network);
            this.Utxos = utxos;
            this.DestinationAddr = BitcoinAddress.Create(DestinationAddr, Network);
            this.Amount = amount;
            this.Fee = fee;
        }
        public TxModel(Network network, string address, IUTXO[] utxos, string DestinationAddr, decimal amount, decimal fee)
        {
            this.Network = network;
            this.Key = null;
            this.Address = BitcoinAddress.Create(address, Network);
            this.Utxos = utxos;
            this.DestinationAddr = BitcoinAddress.Create(DestinationAddr, Network);
            this.Amount = amount;
            this.Fee = fee;
        }

        public decimal GetTotalAmount()
        {
            return Amount + Fee;
        }

    }
}
