using System.Text;

namespace SampAccess
{
    internal static class CodePages
    {
        public static void AddCodePages()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //We're registring more encodings from System.Text.Encoding.CodePages package
        }
    }
}
