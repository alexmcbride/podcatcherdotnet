using System.Web;

namespace PodcatcherDotNet.Helpers {
   public static class HtmlHelper {
       public static string RemoveHtmlTags(string source, int maxLength) {
           char[] array = new char[source.Length];
           int arrayIndex = 0;
           bool inside = false;

           for (int i = 0; i < source.Length; i++) {
               char let = source[i];

               if (let == '<') {
                   inside = true;
                   continue;
               }

               if (let == '>') {
                   inside = false;
                   continue;
               }

               if (!inside) {
                   array[arrayIndex] = let;
                   arrayIndex++;
               }

               if (arrayIndex > maxLength) {
                   break;
               }
           }

           return new string(array, 0, arrayIndex);
       }

       public static string DecodeHtml(string source) {
           return HttpUtility.HtmlDecode(source);
       }
    }
}
