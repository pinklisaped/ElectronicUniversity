using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PNetwork
{
    /// <summary>
    /// Класс-преобразователь для отправки и приема пакетов
    /// </summary>
    public static class Serializator
    {
        public static byte[] Serialize<T>(T data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, data);
            return ms.ToArray();
        }
        /// <summary>
        /// Преобразует 
        /// </summary>
        /// <param name="array">Массив байтов</param>
        /// <returns>Возвращает Packet</returns>
        /// <exception cref="DeserializeException - Возникает при несовпадении отправленных/принятых данных"></exception>
        public static T Deserialize<T>(byte[] array)
        {
            MemoryStream ms = new MemoryStream(array);
            BinaryFormatter formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(ms);
        }
    }
    public class Cryptography_RSA
    {
        System.Security.Cryptography.RSACryptoServiceProvider RSA = new System.Security.Cryptography.RSACryptoServiceProvider(2048);
        public byte[] Encrypt(byte[] data)
        {
            int massiveslenght = (int)Math.Ceiling(data.Length / 214D);
            byte[][] newdata = new byte[massiveslenght][];
            byte[][] crypto = new byte[massiveslenght][];
            for (int i = 0, k = 0; i < massiveslenght; i++)
            {
                newdata[i] = data.Length > k + 214 ? new byte[214] : new byte[data.Length - k];
                for (int j = 0; j < newdata[i].Length && k < data.Length; j++, k++)
                    newdata[i][j] = data[k];
                newdata[i] = RSA.Encrypt(newdata[i], true);
            }
            data = new byte[massiveslenght * 256];
            for (int i = 0, k = 0; i < massiveslenght; i++)
            {
                for (int j = 0; j < newdata[i].Length && k < data.Length; j++, k++)
                    data[k] = newdata[i][j];
            }
            return data;
        }
        public byte[] Decrypt(byte[] data)
        {
            int massiveslenght = (int)Math.Ceiling(data.Length / 256D);
            byte[][] newdata = new byte[massiveslenght][];

            for (int i = 0, k = 0; i < massiveslenght; i++)
            {
                newdata[i] = new byte[256];
                for (int j = 0; j < newdata[i].Length && k < data.Length; j++, k++)
                    newdata[i][j] = data[k];
                newdata[i] = RSA.Decrypt(newdata[i], true);
            }
            int length = 0;
            foreach (byte[] temp in newdata)
            {
                length += temp.Length;
            }
            data = new byte[length];
            for (int i = 0, k = 0; i < massiveslenght; i++)
            {
                for (int j = 0; j < newdata[i].Length && k < data.Length; j++, k++)
                    data[k] = newdata[i][j];
            }
            return data;
        }

        public void Load_Key(string xmlkey)
        {
            RSA.FromXmlString(xmlkey);
        }
        public string Save_Key(bool privatekey)
        {
            return RSA.ToXmlString(privatekey);
        }
    }
}
