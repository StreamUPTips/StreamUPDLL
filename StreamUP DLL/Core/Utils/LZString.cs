using System;
using System.Collections.Generic;
using System.Text;

namespace StreamUP
{
    /// <summary>
    /// LZ-String compression/decompression for C#
    /// Port of the JavaScript lz-string library (https://github.com/pieroxy/lz-string)
    /// Compatible with compressToBase64/decompressFromBase64
    /// </summary>
    public static class LZString
    {
        private const string COMPRESSION_PREFIX = "LZSC:";
        private const string KeyStrBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        private static readonly Dictionary<char, int> BaseReverseDic;

        static LZString()
        {
            BaseReverseDic = new Dictionary<char, int>();
            for (int i = 0; i < KeyStrBase64.Length; i++)
            {
                BaseReverseDic[KeyStrBase64[i]] = i;
            }
        }

        /// <summary>
        /// Check if a string is LZ-String compressed (has LZSC: prefix)
        /// </summary>
        public static bool IsCompressed(string data)
        {
            return !string.IsNullOrEmpty(data) && data.StartsWith(COMPRESSION_PREFIX);
        }

        /// <summary>
        /// Decompress a string, auto-detecting if it's compressed
        /// Returns original string if not compressed or if decompression fails
        /// </summary>
        public static string Decompress(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            if (!IsCompressed(data))
                return data;

            // Remove prefix and decompress
            string compressed = data.Substring(COMPRESSION_PREFIX.Length);
            string result = DecompressFromBase64(compressed);

            // If decompression fails, return original
            return result ?? data;
        }

        /// <summary>
        /// Decompress from Base64 encoded LZ-String
        /// </summary>
        public static string DecompressFromBase64(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            try
            {
                return _decompress(input.Length, 32, index =>
                {
                    if (index >= input.Length)
                        return 0; // Match JavaScript behavior (undefined -> 0)
                    return GetBaseValue(input[index]);
                });
            }
            catch
            {
                return null;
            }
        }

        private static int GetBaseValue(char c)
        {
            if (BaseReverseDic.TryGetValue(c, out int val))
                return val;
            // Return 0 for unknown characters (matches JavaScript undefined -> 0 behavior in bitwise ops)
            return 0;
        }

        private static string _decompress(int length, int resetValue, Func<int, int> getNextValue)
        {
            var dictionary = new Dictionary<int, string>();
            int next;
            int enlargeIn = 4;
            int dictSize = 4;
            int numBits = 3;
            string entry = "";
            var result = new List<string>();
            string w;
            int bits, resb, maxpower, power;
            string c = null;

            var data = new DecompressData
            {
                val = getNextValue(0),
                position = resetValue,
                index = 1
            };

            for (int i = 0; i < 3; i += 1)
            {
                dictionary[i] = ((char)i).ToString();
            }

            bits = 0;
            maxpower = (int)Math.Pow(2, 2);
            power = 1;
            while (power != maxpower)
            {
                resb = data.val & data.position;
                data.position >>= 1;
                if (data.position == 0)
                {
                    data.position = resetValue;
                    data.val = getNextValue(data.index++);
                }
                bits |= (resb > 0 ? 1 : 0) * power;
                power <<= 1;
            }

            next = bits;
            switch (next)
            {
                case 0:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 8);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data.val & data.position;
                        data.position >>= 1;
                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            data.val = getNextValue(data.index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = ((char)bits).ToString();
                    break;
                case 1:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 16);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data.val & data.position;
                        data.position >>= 1;
                        if (data.position == 0)
                        {
                            data.position = resetValue;
                            data.val = getNextValue(data.index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = ((char)bits).ToString();
                    break;
                case 2:
                    return "";
            }
            dictionary[3] = c;
            w = c;
            result.Add(c);
            while (true)
            {
                if (data.index > length)
                {
                    return "";
                }

                bits = 0;
                maxpower = (int)Math.Pow(2, numBits);
                power = 1;
                while (power != maxpower)
                {
                    resb = data.val & data.position;
                    data.position >>= 1;
                    if (data.position == 0)
                    {
                        data.position = resetValue;
                        data.val = getNextValue(data.index++);
                    }
                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }

                int cc = bits;
                switch (cc)
                {
                    case 0:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 8);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data.val & data.position;
                            data.position >>= 1;
                            if (data.position == 0)
                            {
                                data.position = resetValue;
                                data.val = getNextValue(data.index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }

                        dictionary[dictSize++] = ((char)bits).ToString();
                        cc = dictSize - 1;
                        enlargeIn--;
                        break;
                    case 1:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 16);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data.val & data.position;
                            data.position >>= 1;
                            if (data.position == 0)
                            {
                                data.position = resetValue;
                                data.val = getNextValue(data.index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }
                        dictionary[dictSize++] = ((char)bits).ToString();
                        cc = dictSize - 1;
                        enlargeIn--;
                        break;
                    case 2:
                        return string.Join("", result);
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }

                if (dictionary.ContainsKey(cc))
                {
                    entry = dictionary[cc];
                }
                else
                {
                    if (cc == dictSize)
                    {
                        entry = w + w[0];
                    }
                    else
                    {
                        return null;
                    }
                }

                result.Add(entry);

                // Add w+entry[0] to the dictionary.
                dictionary[dictSize++] = w + entry[0];
                enlargeIn--;

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }

                w = entry;
            }
        }

        private class DecompressData
        {
            public int val;
            public int position;
            public int index;
        }
    }
}
