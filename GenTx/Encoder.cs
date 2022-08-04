using System;
using System.Collections.Generic;
using System.Numerics;

namespace GenTx
{
    class Encoder
    {
        private static readonly String BASE43_ALPHABET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ$*+-./:";
        private static readonly String HEX_ALPHABET = "0123456789ABCDEF";

        public static string encodeBase43(byte[] rawData)
        {
            string s = "";
            BigInteger intVal = BigInteger.Zero;
            BigInteger unit = BigInteger.One;

            for (int i = rawData.Length - 1; i >= 0; i--)
            {
                intVal = intVal + unit * rawData[i];
                unit = unit * 256;
            }

            while (intVal > 0)
            {
                int digit = (int)(intVal % 43);
                s = BASE43_ALPHABET[digit] + s;
                intVal = intVal / 43;
            }

            return s;
        }

        public static byte[] decodeBase43(string s)
        {
            BigInteger intVal = BigInteger.Zero;
            BigInteger unit = BigInteger.One;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                int digit = BASE43_ALPHABET.IndexOf(s[i]);
                if (digit < 0)
                {
                    throw new Exception("Base43 decoding - encountered invalid character " + s[i]);
                }

                intVal = intVal + unit * digit;
                unit = unit * 43;
            }

            List<byte> rawData = new List<byte>();

            while (intVal > 0)
            {
                rawData.Add((byte)(intVal % 256));
                intVal /= 256;
            }

            rawData.Reverse();

            return rawData.ToArray();
        }

        public static string encodeHex(byte[] rawData)
        {
            string s = "";
            for (int i = 0; i < rawData.Length; i++)
            {
                s += HEX_ALPHABET[rawData[i] >> 4];
                s += HEX_ALPHABET[rawData[i] & 0xF];
            }
            return s;
        }
    }
}
