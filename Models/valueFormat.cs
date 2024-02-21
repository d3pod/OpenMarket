using System.Net;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace openmarket.Models
{
    public class valueFormat
    {
        private string key = "d28cl2013c1a2022ddca4dl1988d1991";
        public string urlFormat(string text)
        {
            string textFormatted = stringFormat(text);
            return encodeText(textFormatted);
        }
        public string stringFormat(string text)
        {
            string Acentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçñ";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCcn";
            string textFormatted = text;
            for (int i = 0; i < Acentos.Length; i++)
            {
                textFormatted = textFormatted.Replace(Acentos[i].ToString(), semAcentos[i].ToString());
            }
            return textFormatted;
        }
        public string encodeText(string text)
        {
            return WebUtility.UrlEncode(text);
        }
        public string removeEmojis(string text)
        {
            string values = "(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])";
            var textFormatted = Regex.Replace(text, values, "");
            return textFormatted;
        }
        public string encrypter(string text)
        {
            byte[] iv = new byte[16];
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(text);
                        }
                        array = memoryStream.ToArray();
                    }
                }
                return Convert.ToBase64String(array);
            }
        }
        public string decrypter(string text)
        {
            byte[] iv = new byte[16];  
            byte[] buffer = Convert.FromBase64String(text);  
  
            using (Aes aes = Aes.Create())  
            {  
                aes.Key = Encoding.UTF8.GetBytes(key);  
                aes.IV = iv;  
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);  
  
                using (MemoryStream memoryStream = new MemoryStream(buffer))  
                {  
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))  
                    {  
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))  
                        {  
                            return streamReader.ReadToEnd();  
                        }  
                    }  
                }  
            }  
        }
    }
}