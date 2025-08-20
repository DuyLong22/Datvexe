// File: Helpers/VnPayHelper.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BusTicketBooking.Helpers
{
    public static class VnPayHelper
    {
        public static string GeneratePaymentUrl(string vnp_TmnCode, string vnp_HashSecret, Dictionary<string, string> vnp_Params)
        {
            // Tạo chuỗi query
            var sorted = vnp_Params.OrderBy(k => k.Key);
            var queryString = string.Join("&", sorted.Select(k => $"{k.Key}={Uri.EscapeDataString(k.Value)}"));
            var hashData = string.Join("&", sorted.Select(k => $"{k.Key}={k.Value}"));
            string secureHash;
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret)))
            {
                secureHash = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(hashData))).Replace("-", "").ToUpper();
            }
            return $"https://sandbox.vnpayment.vn/paymentv2/vpcpay.html?{queryString}&vnp_SecureHash={secureHash}";
        }
    }
}
