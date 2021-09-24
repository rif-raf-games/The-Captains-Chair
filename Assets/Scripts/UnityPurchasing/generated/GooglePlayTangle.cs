// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("DZh8qMcOBG5K6FatyG1ZxafSLbnTZLPdSSNODNod8G63I8/wUyxuBFJn2qeQgZHhzIQ/4rTFHMirWfEUY3q0pMcj9UvpEhw8eNIP7v+oBECSjSKu5TzTmeOC77tiBnfll0P3ewZCplhvJWtJr0FEY+8HguKAHW1tpQSJM4w7xHqw5kuRLwbSoEjYTRXSdaM/h8QSyXWsJbgvE/lZfQtIQ7hopxZ4lJk9yBkaXCJlfqyL4qUB0nPlTCnTNSITG3YllyFpYCSZ1dhk1lV2ZFlSXX7SHNKjWVVVVVFUV9ZVW1Rk1lVeVtZVVVTbZo0Lwcs6hUBLL83wyt83CbcS7KUYzDPvwIDPPfZo1o2ENN7MUP/kNUPrEiGhRCfpYHSg9dgMEVZXVVRV");
        private static int[] order = new int[] { 13,2,11,9,7,5,8,8,12,9,13,11,12,13,14 };
        private static int key = 84;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
