using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace Lab10.utils
{
    public class Parser
    {
        public static async Task<string[]> Parse(string CSVString)
        {
            using (StringReader stringReader = new StringReader(CSVString))
            {

                using (TextFieldParser textFieldParser = new TextFieldParser(stringReader))
                {
                    textFieldParser.TextFieldType = FieldType.Delimited;
                    textFieldParser.SetDelimiters(",");

                    textFieldParser.ReadFields();

                    string[] rows = textFieldParser.ReadFields();

                    return rows;
                }
            }
        }
    }
}
