// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("yXv428n0//DTf7F/DvT4+Pj8+fqr7wv1wojG5ALs6c5Cqi9PLbDAwP/Kdwo9LDxMYSmSTxlosWUG9Fy5FcUKu9U5NJBltLfxj8jTASZPCKw/II8DSJF+NE4vQhbPq9pIOu5a1ijt5oJgXWdymqQav0EItWGeQm0tCKkkniGWadcdS+Y8gqt/DeV14Lh7+Pb5yXv48/t7+Pj5dssgpmxml87XGQlqjljmRL+xkdV/okNSBantfskecOSO46F3sF3DGo5iXf6Bw6l/3kjhhH6Yj76224g6jMTNiTR4dWKQW8V7ICmZc2H9UkmY7ka/jAzpoDXRBWqjqcPnRfsAZcD0aAp/gBR/2A6SKmm/ZNgBiBWCvlT00Kbl7opEzdkNWHWhvPv6+Pn4");
        private static int[] order = new int[] { 0,5,11,12,7,7,8,7,9,11,12,12,13,13,14 };
        private static int key = 249;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
