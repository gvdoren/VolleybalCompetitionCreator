using System;
using System.Management;
using System.Security.Cryptography;
using System.Security;
using System.Collections;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Security
{
    /// <summary>
    /// Generates a 16 byte Unique Identification code of a computer
    /// Example: 4876-8DB5-EE85-69D3-FE52-8CF7-395D-2EA9
    /// </summary>
    public class FingerPrint
    {
        private static string fingerPrint = string.Empty;
        public static string Value()
        {
            if (string.IsNullOrEmpty(fingerPrint))
            {
                fingerPrint = GetHash("CPU >> " + cpuId() + 
                                      "\nBIOS >> " + biosId() + 
                                      "\nBASE >> " + baseId() +
                                      "\nDISK >> "+ diskId() + 
                                      "\nVIDEO >> " + videoId() +
                                      "\nMAC >> "+ macId()
                                     );
            }
            return fingerPrint;
        }
        private static string GetHash(string s)
        {
            MD5 sec = new MD5CryptoServiceProvider();
            ASCIIEncoding enc = new ASCIIEncoding();
            byte[] bt = enc.GetBytes(s);
            return GetHexString(sec.ComputeHash(bt));
        }
        private static string GetHexString(byte[] bt)
        {
            string s = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte b = bt[i];
                int n, n1, n2;
                n = (int)b;
                n1 = n & 15;
                n2 = (n >> 4) & 15;
                if (n2 > 9)
                    s += ((char)(n2 - 10 + (int)'A')).ToString();
                else
                    s += n2.ToString();
                if (n1 > 9)
                    s += ((char)(n1 - 10 + (int)'A')).ToString();
                else
                    s += n1.ToString();
                if ((i + 1) != bt.Length && (i + 1) % 2 == 0) s += "-";
            }
            return s;
        }
        #region Original Device ID Getting Code
        //Return a hardware identifier
        private static string identifier
        (string wmiClass, string wmiProperty, string wmiMustBeTrue)
        {
            string result = "";
            System.Management.ManagementClass mc = new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                if (mo[wmiMustBeTrue].ToString() == "True")
                {
                    //Only get the first one
                    if (result == "")
                    {
                        try
                        {
                            result = mo[wmiProperty].ToString();
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return result;
        }
        //Return a hardware identifier
        private static string identifier(string wmiClass, string wmiProperty)
        {
            string result = "";
            System.Management.ManagementClass mc =
        new System.Management.ManagementClass(wmiClass);
            System.Management.ManagementObjectCollection moc = mc.GetInstances();
            foreach (System.Management.ManagementObject mo in moc)
            {
                //Only get the first one
                if (result == "")
                {
                    try
                    {
                        result = mo[wmiProperty].ToString();
                        break;
                    }
                    catch
                    {
                    }
                }
            }
            return result;
        }
        private static string cpuId()
        {
            //Uses first CPU identifier available in order of preference
            //Don't get all identifiers, as it is very time consuming
            string retVal = identifier("Win32_Processor", "UniqueId");
            if (retVal == "") //If no UniqueID, use ProcessorID
            {
                retVal = identifier("Win32_Processor", "ProcessorId");
                if (retVal == "") //If no ProcessorId, use Name
                {
                    retVal = identifier("Win32_Processor", "Name");
                    if (retVal == "") //If no Name, use Manufacturer
                    {
                        retVal = identifier("Win32_Processor", "Manufacturer");
                    }
                    //Add clock speed for extra security
                    retVal += identifier("Win32_Processor", "MaxClockSpeed");
                }
            }
            return retVal;
        }
        //BIOS Identifier
        private static string biosId()
        {
            return identifier("Win32_BIOS", "Manufacturer")
            + identifier("Win32_BIOS", "SMBIOSBIOSVersion")
            + identifier("Win32_BIOS", "IdentificationCode")
            + identifier("Win32_BIOS", "SerialNumber")
            + identifier("Win32_BIOS", "ReleaseDate")
            + identifier("Win32_BIOS", "Version");
        }
        //Main physical hard drive ID
        private static string diskId()
        {
            return identifier("Win32_DiskDrive", "Model")
            + identifier("Win32_DiskDrive", "Manufacturer")
            + identifier("Win32_DiskDrive", "Signature")
            + identifier("Win32_DiskDrive", "TotalHeads");
        }
        //Motherboard ID
        private static string baseId()
        {
            return identifier("Win32_BaseBoard", "Model")
            + identifier("Win32_BaseBoard", "Manufacturer")
            + identifier("Win32_BaseBoard", "Name")
            + identifier("Win32_BaseBoard", "SerialNumber");
        }
        //Primary video controller ID
        private static string videoId()
        {
            return identifier("Win32_VideoController", "DriverVersion")
            + identifier("Win32_VideoController", "Name");
        }
        //First enabled network card ID
        private static string macId()
        {
            return identifier("Win32_NetworkAdapterConfiguration",
                "MACAddress", "IPEnabled");
        }
        #endregion
    }

    public static class StringCipher
    {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private const string initVector = "tu89geji340t89u2";

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];
            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            //cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }
    }

    public class LicenseKey
    {
        DateTime date;// = new DateTime(2000,0,0);
        string user;
        string key;
        public LicenseKey(string Key)
        {
            key = Key;
            Valid();
        }
        public bool Valid(bool msg = false)
        {
            bool returnvalue = false;
            try
            {
                string result = StringCipher.Decrypt(key, FingerPrint.Value());
                string[] delimiters = new string[] { "|||" };
                string[] results = result.Split(delimiters, StringSplitOptions.None);
                if (results.Length != 3)
                {
                    if (msg) MessageBox.Show("License Key is invalid");
                    return false;
                }
                if (results[2] != FingerPrint.Value())
                {
                    if (msg) MessageBox.Show("License Key is not for this computer");
                    return false;
                }
                date = DateTime.Parse(results[0]);
                user = results[1];
                returnvalue = true;
            }
            catch {}
            return returnvalue;
        }
        static public string Create(string user, string fingerprint, DateTime date)
        {
            string total = date.ToShortDateString() + "|||" + user + "|||" + fingerprint;
            return StringCipher.Encrypt(total, fingerprint);
        }
        public DateTime ValidUntil()
        {
            return date;
        }
        public string ValidUser()
        {
            return user;
        }
        public string Key()
        {
            return key;
        }
    }

}