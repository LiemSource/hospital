using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VaeHelper
{
    public class SHA256WithRSAHelper
    {
        /// <summary>
        /// RSA分段解密,密文来自java
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="m_strDecryptString"></param>
        /// <returns></returns>
        public static string RSADecryptJava(string privateKey, string m_strDecryptString)
        {
            string xmlPrivateKey = RSAPrivateKeyJava2DotNet(privateKey);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(xmlPrivateKey);
            var sourceBytes = Convert.FromBase64String(m_strDecryptString);
            int page = rsa.KeySize / 8;
            using (var plaiStream = new MemoryStream(sourceBytes))
            {
                using (var decrypStream = new MemoryStream())
                {
                    var offSet = 0;
                    var inputLen = sourceBytes.Length;
                    for (var i = 0; inputLen - offSet > 0; offSet = i * page)
                    {

                        if (inputLen - offSet > page)
                        {
                            var buffer = new Byte[page];
                            plaiStream.Read(buffer, 0, page);
                            var decrypData = rsa.Decrypt(buffer, false);
                            decrypStream.Write(decrypData, 0, decrypData.Length);
                        }
                        else
                        {
                            var buffer = new Byte[inputLen - offSet];
                            plaiStream.Read(buffer, 0, inputLen - offSet);
                            var decrypData = rsa.Decrypt(buffer, false);
                            decrypStream.Write(decrypData, 0, decrypData.Length);
                        }
                        ++i;
                    }
                    decrypStream.Position = 0;
                    cipherbytes = decrypStream.ToArray();
                }
            }
            return Regex.Unescape(Encoding.UTF8.GetString(cipherbytes));
        }
        /// <summary>
        /// RSA分段解密,密文来自DotNet
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="m_strDecryptString"></param>
        /// <returns></returns>
        public static string RSADecryptDotNet(string privateKey, string m_strDecryptString)
        {
            RSACryptoServiceProvider rsa = DecodeRSAPrivateKey(Convert.FromBase64String(privateKey));
            byte[] cipherbytes;
            var sourceBytes = Convert.FromBase64String(m_strDecryptString);
            int page = rsa.KeySize / 8;
            using (var plaiStream = new MemoryStream(sourceBytes))
            {
                using (var decrypStream = new MemoryStream())
                {
                    var offSet = 0;
                    var inputLen = sourceBytes.Length;
                    for (var i = 0; inputLen - offSet > 0; offSet = i * page)
                    {

                        if (inputLen - offSet > page)
                        {
                            var buffer = new Byte[page];
                            plaiStream.Read(buffer, 0, page);
                            var decrypData = rsa.Decrypt(buffer, false);
                            decrypStream.Write(decrypData, 0, decrypData.Length);
                        }
                        else
                        {
                            var buffer = new Byte[inputLen - offSet];
                            plaiStream.Read(buffer, 0, inputLen - offSet);
                            var decrypData = rsa.Decrypt(buffer, false);
                            decrypStream.Write(decrypData, 0, decrypData.Length);
                        }
                        ++i;
                    }
                    decrypStream.Position = 0;
                    cipherbytes = decrypStream.ToArray();
                }
            }
            return Regex.Unescape(Encoding.UTF8.GetString(cipherbytes));
        }

        /// <summary>
        /// RSA私钥格式转换，java->.net
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static string RSAPrivateKeyJava2DotNet(string privateKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

            return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        }

        private static RSACryptoServiceProvider DecodePemPrivateKey(String pemstr)
        {
            byte[] pkcs8privatekey;
            pkcs8privatekey = Convert.FromBase64String(pemstr);
            if (pkcs8privatekey != null)
            {
                RSACryptoServiceProvider rsa = DecodePrivateKeyInfo(pkcs8privatekey);
                return rsa;
            }
            else
                return null;
        }

        private static RSACryptoServiceProvider DecodePrivateKeyInfo(byte[] pkcs8)
        {
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];

            MemoryStream mem = new MemoryStream(pkcs8);
            int lenstream = (int)mem.Length;
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading  
            byte bt = 0;
            ushort twobytes = 0;

            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)  
                    binr.ReadByte();    //advance 1 byte  
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes  
                else
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x02)
                    return null;

                twobytes = binr.ReadUInt16();

                if (twobytes != 0x0001)
                    return null;

                seq = binr.ReadBytes(15);        //read the Sequence OID  
                if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct  
                    return null;

                bt = binr.ReadByte();
                if (bt != 0x04)    //expect an Octet string  
                    return null;

                bt = binr.ReadByte();        //read next byte, or next 2 bytes is  0x81 or 0x82; otherwise bt is the byte count  
                if (bt == 0x81)
                    binr.ReadByte();
                else
                    if (bt == 0x82)
                    binr.ReadUInt16();
                //------ at this stage, the remaining sequence should be the RSA private key  

                byte[] rsaprivkey = binr.ReadBytes((int)(lenstream - mem.Position));
                RSACryptoServiceProvider rsacsp = DecodeRSAPrivateKey(rsaprivkey);
                return rsacsp;
            }

            catch (Exception)
            {
                return null;
            }

            finally { binr.Close(); }

        }

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

        private static RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // ---------  Set up stream to decode the asn.1 encoded RSA private key  ------  
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading  
            byte bt = 0;
            ushort twobytes = 0;
            int elems = 0;
            try
            {
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)    //data read as little endian order (actual data order for Sequence is 30 81)  
                    binr.ReadByte();    //advance 1 byte  
                else if (twobytes == 0x8230)
                    binr.ReadInt16();    //advance 2 bytes  
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)    //version number  
                    return null;
                bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;


                //------  all private key components are Integer sequences ----  
                elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // ------- create RSACryptoServiceProvider instance and initialize with public key -----  
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters();
                RSAparams.Modulus = MODULUS;
                RSAparams.Exponent = E;
                RSAparams.D = D;
                RSAparams.P = P;
                RSAparams.Q = Q;
                RSAparams.DP = DP;
                RSAparams.DQ = DQ;
                RSAparams.InverseQ = IQ;
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)        //expect integer  
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();    // data size in next byte  
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte();    // data size in next 2 bytes  
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;        // we already have the data size  
            }



            while (binr.ReadByte() == 0x00)
            {    //remove high order zeros in data  
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);        //last ReadByte wasn't a removed zero, so back up a byte  
            return count;
        }

        public static string SignerSymbol = "SHA1WithRSA";
        private static AsymmetricKeyParameter CreateKEY(bool isPrivate, string key)
        {
            byte[] keyInfoByte = Convert.FromBase64String(key);
            if (isPrivate)
                return PrivateKeyFactory.CreateKey(keyInfoByte);
            else
                return PublicKeyFactory.CreateKey(keyInfoByte);
        }

        /// <summary> 
        /// 数据加密 
        /// </summary> 
        /// <param name="content">待加密字符串</param>
        /// /// <param name="privatekey">私钥</param> 
        /// <returns>加密后字符串</returns> 
        public static string Sign(string content, string privatekey)
        {
            ISigner sig = SignerUtilities.GetSigner(SignerSymbol);
            sig.Init(true, CreateKEY(true, privatekey));

            byte[] bytes = Encoding.UTF8.GetBytes(content); //待加密字符串
            sig.BlockUpdate(bytes, 0, bytes.Length);
            byte[] signature = sig.GenerateSignature(); // Base 64 encode the sig so its 8-bit clean 
            var signedString = Convert.ToBase64String(signature);
            return signedString;
        }

        public static string M1050B(string inputString)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(inputString);

                // 使用 SHA-256 算法计算散列
                byte[] hash;
                using (SHA256 sha256 = SHA256.Create())
                {
                    hash = sha256.ComputeHash(data);
                }

                // 将字节数组转换为十六进制字符串
                string result = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
    }
}
