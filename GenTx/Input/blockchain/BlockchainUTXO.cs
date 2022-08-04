using System;

namespace GenTx
{
    internal class BlockchainUTXO : IUTXO
    {
        public string? tx_hash_big_endian { get; set; }
        public uint tx_output_n { get; set; }
        public ulong value { get; set; }

        public uint GetOutputNo()
        {
            return tx_output_n;
        }

        public string? GetTxId()
        {
            return tx_hash_big_endian;
        }

        public decimal GetValue()
        {
            return Convert.ToDecimal(value / 100000000.0);
        }
    }
}
