using QRCoder;

namespace BookingAPI.Helpers
{
    public static class GenerateQr
    {
        public static byte[] GenerateQrBytes(string text)
        {
            using (var generator = new QRCodeGenerator())
            {
                var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
                var qr = new PngByteQRCode(data);

                return qr.GetGraphic(20);
            }
        }
    }
}
