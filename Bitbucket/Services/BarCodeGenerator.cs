using System.Text;

namespace Bitbucket.Services
{
    public class BarCodeGenerator
    {
        public static char[] AvailableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        public static int MaxLengthBarCode = 25;
        public string GenerateBarCode(int charInStart, int charInEnd)
        {
            StringBuilder builder = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < charInStart; i++)
            {
                builder.Append(AvailableChars[random.Next(0, AvailableChars.Length)]);
            }

            for (int i = 0; i < MaxLengthBarCode - (charInStart + charInEnd); i++)
            {
                builder.Append(random.Next(0, 9));
            }

            for (int i = 0; i < charInEnd; i++)
            {
                builder.Append(AvailableChars[random.Next(0, AvailableChars.Length)]);
            }

            return builder.ToString();
        }
    }
}