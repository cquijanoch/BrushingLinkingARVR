using System.Collections.Generic;
using System.IO;

namespace BrushingAndLinking
{
    public static class Logger
    {
        public static string FilePath;
        private static StreamWriter loogerWriter;

        private static void WriteToCsv(string nameCsv, IEnumerable<string[]> headers, IEnumerable<string[]> rows)
        {
            if (!FilePath.EndsWith('/'))
                FilePath += '/';

            if (!nameCsv.EndsWith(".csv"))
                nameCsv += ".csv";

            loogerWriter = new StreamWriter(FilePath + nameCsv, true);

            foreach (string[] head in headers)
                loogerWriter.WriteLine(string.Join(',', head));

            foreach (string[] line in rows)
                loogerWriter.WriteLine(string.Join(',', line));

            loogerWriter.Close();
        }

        public static void ReadCsvFromString(string text, ref List<string[]> data, ref string[] headers)
        {
            // Split entire string
            string[] textSplit = text.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

            // Read header
            headers = textSplit[0].Split(',');

            // Create data structure
            data = new List<string[]>();

            // Read data line by line. Add each row to our list of lists
            for (int i = 1; i < textSplit.Length; i++)
            {
                string[] lineSplit = textSplit[i].Split(',');
                data.Add(lineSplit);
            }
        }
    }
}