// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("56NHuY7EiqhOoKWCDuZjA2H8jIwyhVI8qMKv7Tv8EY9Wwi4Rss2P5YU3tJeFuLO8nzP9M0K4tLS0sLW2ROVo0m3aJZtRB6pwzuczQak5rPRzbMNPBN0yeAJjDlqD55YEdqIWmux5nUkm7+WPqwm3TCmMuCRGM8xYWYlG95l1eNwp+Pu9w4SfTWoDROAzkgStyDLUw/L6l8R2wIiBxXg0OWShqs4sESs+1uhW8w1E+S3SDiFhM5RC3mYl8yiUTcRZzvIYuJzqqaI3tLq1hTe0v7c3tLS1Oods6iAq27OGO0ZxYHAALWXeA1Uk/SlKuBD1LtwXiTdsZdU/LbEeBdSiCvPAQKWCm1VFJsIUqgjz/d2ZM+4PHknlocYIgZVBFDnt8Le2tLW0");
        private static int[] order = new int[] { 5,2,5,8,7,13,12,8,8,12,13,11,13,13,14 };
        private static int key = 181;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
