using System.IO;
using System.Linq;
using System.Text;

namespace KiwiDb.Util
{
    public static class Magic
    {
        public const int SignatureLength = 64;

        public static byte[] MakeSignature(string signature)
        {
            using (var stream = new MemoryStream())
            {
                var bytes = Encoding.ASCII.GetBytes(signature ?? string.Empty);
                Verify.Argument(bytes.Length < SignatureLength, "Signature \"{0}\" is too long", signature);

                stream.Write(bytes, 0, bytes.Length);

                if (bytes.Length < SignatureLength)
                {
                    var padding = Enumerable.Range(0, SignatureLength - bytes.Length).Select(i => (byte) ' ').ToArray();
                    padding[padding.Length - 1] = (byte) '\n';
                    stream.Write(padding, 0, padding.Length);
                }

                return stream.ToArray();
            }
        }

        public static string Read(BinaryReader reader)
        {
            return Encoding.ASCII.GetString(reader.ReadBytes(SignatureLength)).TrimEnd();
        }
    }
}