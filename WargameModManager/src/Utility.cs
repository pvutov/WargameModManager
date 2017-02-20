using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModManager {
    static class Utility {
        public static string getStringJsonField(string field, string json) {
            string formattedFieldName = "\"" + field + "\":";

            string result = "";

            try {
                result = json.Substring(json.IndexOf(formattedFieldName) + formattedFieldName.Length);
                // Convert escaped double quotes into single quotes
                // Still error prone, should move to a json parsing library
                result = result.Replace("\\\"", "'");

                result = result.Split('"')[1];
            }
            catch (Exception e) when (e is ArgumentOutOfRangeException || e is IndexOutOfRangeException) {
                Program.warning("JSON field not found.");
            }

            return result;
        }
    }
}
