using System;

namespace GenTx
{
    internal class SochanUTXO : IUTXO
    {
        public string? txid { get; set; }
        public uint output_no { get; set; }
        public string? value { get; set; }

        public uint GetOutputNo()
        {
            return output_no;
        }

        public string? GetTxId()
        {
            return txid;
        }

        public decimal GetValue()
        {
            return Convert.ToDecimal(value);
        }
    }
}
