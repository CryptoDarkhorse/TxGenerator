namespace GenTx
{
    public interface IUTXO
    {
        string? GetTxId();
        decimal GetValue();
        uint GetOutputNo();
    }
}
