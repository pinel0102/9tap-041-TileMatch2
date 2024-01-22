// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("efPim/kGcaeBM8vSnIsHmdI+3eeO/k4GSJ1E3p0gsg1Q7k2UrZMw32S0hkstR/0sD/Gg01+4JC9NAYlSaUzmWWrBTQKmkEqnaCTJ197r+ioW7/1PAXKZ5wB/1QKjCzfFF0Z/MlfU2tXlV9Tf11fU1NV0bRIcYyIHmhxa46iFZ68rWxvXismL1jZs+SHlV9T35djT3P9TnVMi2NTU1NDV1lkBXgItXL62DGTjrnyH0bz1dNjS9sBt+Ogthi5dIfFpjbXl6gMu+N/T2cZFljBoox6qJ58SPnSIG3qFItHIZd1QkcJ2qeB8sDKny/1aVlxMQfme5dn87ACIw/HtjiMAD5ZXkjIw5h2HPvEw4zuQE+5uCfF2I02LV3YRejTFOFFnTNfW1NXU");
        private static int[] order = new int[] { 6,7,5,4,11,7,12,12,13,13,13,12,13,13,14 };
        private static int key = 213;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
