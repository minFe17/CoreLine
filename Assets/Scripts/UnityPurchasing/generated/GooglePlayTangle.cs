// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("AoGPgLACgYqCAoGBgCtpweOuY5I6Z1SEPsU5Z3Lh+EAo9QFqJ1xAuloMKO2jFBnvwQ8iUuR3Hc7S9dozcXW4z5LbQH9FIRgmUi6/2t/SKM+NTCZlAsgwfvCV51p9vMCiwpk9gvWrKsNHm7AlkLR5TgfUtgROPKsWzGgFJJ0uDETGcnXXrl5HNRhFJY2wAoGisI2GiaoGyAZ3jYGBgYWAgzUnsCNKPAKz/vQvrAjt2fXOXVLLEDNaNkUOuG1rWNGwp7/yEMZpzrWwPRvDRLEjpWIiTqOA82GxFcsTsBuRzF34fwohHfDAuxuIJCLr455RZkVWdQNBzjv/FfzIqrYJUsEbH4+wMoc6f6pMPCkrG5unU9RqVrayffGYTzCOEOBfxYKDgYCB");
        private static int[] order = new int[] { 1,5,8,12,13,10,9,10,8,10,12,11,12,13,14 };
        private static int key = 128;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
