using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenTx
{
    class Program
    {

        static void Main(string[] args)
        {
            string sender = "1EJPm1dTtVoG7uPQuKV3ciHFUMVunprgpr"; 
            string recipient = "bc1qj83mgchmssl63fer7rdd4h9zft5q2yuhup8m3r";
            decimal amount = 0.0001m;
            decimal fee = 0.00001m;

            Console.WriteLine("Getting balance of address {0}", sender);
            var bal = TransactionHelper.GetTotalBalance(sender);
            bal.Wait();
            Console.WriteLine("Balance of address {0} is {1}", sender, bal.Result);
            Console.WriteLine("");

            Console.WriteLine("Making transaction sending {0}BTC from {1} to {2} (fee {3})...",
                amount, sender, recipient, fee);

            var task = TransactionHelper.GenerateTxAsync(sender, recipient, amount, fee);
            task.Wait();
            byte[] txData = task.Result;

            if (txData == null) return;

            string txStr = Encoder.encodeBase43(txData);

            Console.WriteLine("Transaction PSBT(Base43 encoding):\n{0}", txStr);

            Console.WriteLine("Transaction PSBT(Hex):\n{0}", Encoder.encodeHex(txData));

            Console.WriteLine("");

            string signedTxStr = "7YRL0F6PEIUO22Z+TSVYO1+6MWZ22Y0.Q5XK-W*.HA7QZ556PD18TVNZY-K.X/NINJ/S$LY-87V-L5ZQH6CB3X*H5I/:/VKJMP2GX2VE*+6IPYJ$ETEZNJFJW.G9*+L14W+JTWW07RWCI+E-:ECVE3YCSL0-VHZF:*K87J4PWC:A$LOB42-XI21I5ECHP/E42X9GO972+QIO0L3UJPPRFC.2U.1S5QR:A:U831-DAO6.2TPHC+A1G9W./D0Z1B6+R6:2SMZP.5O.XUT3BXV7M91-TP9FUCQ3.MJKQ7*98YZ5RFRFLSPM.LQAM/89O-0J.T6X2/ZOH9VB288RC6ZS$0RVXZ67U3KT+D1Y$USUO0BLMHE1YWU6RP36HH:F8BT-3-I6BLEHIIYADR3RXVKBCUKM9A5T0BEX*LZQ2OMDQ4H9-8RCS:.KE1LFJ1IXI8-PZMD4WEQZW6UQP--*L9:$A3B6+LT60JMOWNS*:SL:LS-MUIQ-*DJ1NXCAKE70434H*N967DDAH48IT0L5T3H55XE1K-QGVFE";

            byte[] decodedTxData = Encoder.decodeBase43(signedTxStr);
            string hexEncoded = Encoder.encodeHex(decodedTxData);
            Console.WriteLine("Hex encoded data:\n{0}", hexEncoded);

            Console.WriteLine("Broadcasting data...");
            var broadcastTask = TransactionHelper.BroadcastRawTxAsync(hexEncoded);
            broadcastTask.Wait();
            if (broadcastTask.Result == null) Console.WriteLine("Failed");
            else Console.WriteLine("Broadcast result: {0}", broadcastTask.Result);
        }
    }
}
