using System;
using System.Text;
using OtpNet;

namespace Primagaz.Standard.Service
{
    public static class OtpService
    {
        const int TimeStepInSeconds = 86400; // 24 hours
        const string SecretKey = "PrimagazApp";

        public static bool ValidateOTP(string otp)
        {
            var secretKeyAsByteArray = Encoding.Unicode.GetBytes(SecretKey);
            var totp = new Totp(secretKeyAsByteArray, TimeStepInSeconds);
            var result = totp.VerifyTotp(otp, out long timeStepMatched);
            return result;
        }
    }
}
