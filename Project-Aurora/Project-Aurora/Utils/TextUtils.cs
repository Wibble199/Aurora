using System.Text.RegularExpressions;

namespace Aurora.Utils {

    public static class TextUtils {

        /// <summary>
        /// Converts a CamelCaseString into a Space Case String.
        /// E.G. "OMGThisIsSoCool" -> "OMG This Is So Cool"
        /// </summary>
        // https://stackoverflow.com/a/5796427
        public static string CamelToSpaceCase(this string str) => Regex.Replace(str, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1");
    }
}
