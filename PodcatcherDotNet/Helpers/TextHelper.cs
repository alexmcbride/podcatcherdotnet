using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PodcatcherDotNet.Helpers {
    public static class TextHelper {
        private static Typeface _typeface;
        private static double _fontSize;

        public static void Initialize(FontFamily fontFamily, double fontSize) {
            _typeface = new Typeface(fontFamily.ToString());
            _fontSize = fontSize;
        }

        public static double GetTextWidth(string text) {
            var formattedText = GetFormattedText(text);

            return formattedText.Width;
        }

        private static FormattedText GetFormattedText(string text) {
            return new FormattedText(
               text,
               CultureInfo.CurrentCulture,
               FlowDirection.LeftToRight,
               _typeface,
               _fontSize,
               Brushes.Black);
        }
    }
}
