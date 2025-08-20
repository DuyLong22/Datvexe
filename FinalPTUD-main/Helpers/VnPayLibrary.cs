using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

public static class VnPayLibrary
{
    // Tạo chữ ký HMAC SHA512
    public static string CreateSignature(Dictionary<string, string> data, string secretKey)
    {
        // Sắp xếp key alphabet
        var sorted = data.OrderBy(k => k.Key);
        var sb = new StringBuilder();
        foreach (var kv in sorted)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                if (sb.Length > 0) sb.Append("&");
                sb.Append(kv.Key).Append("=").Append(kv.Value);
            }
        }
        string dataString = sb.ToString();

        using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            return BitConverter.ToString(hashValue).Replace("-", "").ToUpper();
        }
    }

    // Validate chữ ký trả về VNPay
    public static bool ValidateSignature(IQueryCollection query, string secretKey, string secureHash)
    {
        // Lấy tất cả key bắt đầu bằng vnp_ trừ vnp_SecureHash
        var data = new Dictionary<string, string>();
        foreach (var key in query.Keys)
        {
            if (key.StartsWith("vnp_") && key != "vnp_SecureHash")
            {
                data[key] = query[key];
            }
        }

        string calculatedHash = CreateSignature(data, secretKey);
        return string.Equals(calculatedHash, secureHash, StringComparison.OrdinalIgnoreCase);
    }
}
