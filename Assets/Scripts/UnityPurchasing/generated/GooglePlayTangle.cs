// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("wFWxZQrDyaOHJZtgBaCUCGof4HRfQO9jKPEeVC5PInavy7ooWo46tq63eWkK7jiGJN/R8bUfwiMyZcmNH7hu8koJ3wS4Yeh14t40lLDGhY6pG5i7qZSfkLMf0R9ulJiYmJyZmgLwO6UbQEn5EwGdMin4jibf7GyJG5iWmakbmJObG5iYmRarQMYMBvcfviiB5B74797Wu+ha7KSt6VQYFXWlatu1WVTwBdTXke+os2FGL2jMy49rlaLopoRijImuIspPL03QoKBIjYbiAD0HEvrEet8haNUB/iINTZ+qF2pdTFwsAUnyL3kI0QVmlDzZaMlE/kH2Cbd9K4Zc4ssfbYUVgNgeqX4QhO6DwRfQPaN67gI9nuGjyeokrbltOBXB3JuamJmY");
        private static int[] order = new int[] { 13,7,9,6,13,10,7,7,12,10,13,11,12,13,14 };
        private static int key = 153;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
