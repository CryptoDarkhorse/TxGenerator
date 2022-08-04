namespace GenTx
{
    class Txs
    {
        public SochanUTXO[]? txs { get; set; }
    }

    class SochainReply
    {
        public Txs? data { get; set; }
    }
}
