using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System;

namespace TRDev.SafetyClient
{
    public class AES : MonoBehaviour
    {
        public string key;
        //public string iv;

        public string jiaMiText;
        public string jieMiText;

        private void Start()
        {
            Debug.Log("加密：" + Encryptor_Base64(jiaMiText, key));
            Debug.Log("解密：" + Decryptor_Base64(jieMiText, key));
        }

        private static RijndaelManaged Create(string key)
        {
            RijndaelManaged rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.Key = Encoding.UTF8.GetBytes(key);
            rijndaelManaged.BlockSize = 128;
            rijndaelManaged.Mode = CipherMode.ECB;
            rijndaelManaged.Padding = PaddingMode.PKCS7;
            return rijndaelManaged;
        }

        public static string Encryptor_Base64(string _text, string key)
        {
            try
            {
                ICryptoTransform transform = Create(key).CreateEncryptor();

                byte[] textBytes = Encoding.UTF8.GetBytes(_text);

                byte[] resultBytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                return Convert.ToBase64String(resultBytes);
            }
            catch (Exception _ex)
            {
                Debug.Log(_ex.ToString());
                return null;
            }
        }

        public static string Decryptor_Base64(string _text, string _key)
        {
            try
            {
                byte[] textBytes = Convert.FromBase64String(_text);

                ICryptoTransform cryptoTransform = Create(_key).CreateDecryptor();

                byte[] resultBytes = cryptoTransform.TransformFinalBlock(textBytes, 0, textBytes.Length);

                return Encoding.UTF8.GetString(resultBytes);

            }
            catch (Exception _ex)
            {
                Debug.Log(_ex.ToString());
                return null;
            }
        }



    }
}