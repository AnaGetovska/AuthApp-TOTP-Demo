using OtpNet;
using QRCoder;
using System.Drawing;

namespace TotpDemo
{
    public class TOTPService
    {
        public string GenerateTOTPSecret()
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(key);
        }
        public string GenerateQRCodeUri(string username, string secret)
        {
            return $"otpauth://totp/{username}?secret={secret}&issuer=TotpDemo";
        }

        public byte[] GenerateQRCode(string uri)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new BitmapByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }
        public bool ValidateTOTP(string secret, string code)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret));
            return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
        }
    }
}
