// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("yJDPk7zNLyed9XI/7RZALWTlSUOHfmzekOMIdpHuRJMymqZUhtfuo/UlF9q81my9nmAxQs4ptb7ckBjD6GJzCmiX4DYQolpDDRqWCEOvTHYLjctyORT2PrrKikYbWBpHp/1osB9v35fZDNVPDLEjnMF/3AU8AqFOoXeMFq9goXKqAYJ//5hg57LcGsbQaA90SG19kRlSYHwfspGeB8YDo3TGRWZ0SUJNbsIMwrNJRUVFQURHQkhX1Aeh+TKPO7YOg6/lGYrrFLPGRUtEdMZFTkbGRUVE5fyDjfKzlmdR/Gl5vBe/zLBg+BwkdHuSv2lO+N13yPtQ3JM3Ads2+bVYRk96a7tAWfRMwQBT5zhx7SGjNlpsy8fN3eeA66VUqcD23UZHRURF");
        private static int[] order = new int[] { 13,11,5,6,12,7,6,10,13,9,11,13,12,13,14 };
        private static int key = 68;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
