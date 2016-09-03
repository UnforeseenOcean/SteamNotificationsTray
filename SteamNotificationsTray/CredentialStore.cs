﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using SteamNotificationsTray.WebLogin.Models;
using Newtonsoft.Json;

namespace SteamNotificationsTray
{
    static class CredentialStore
    {
        static byte[] GetStrongNameKey()
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            return assembly.GetName().GetPublicKey();
        }

        internal static TransferParameters GetTransferParameters()
        {
            var encryptedParams = Properties.Settings.Default.Credentials;
            if (string.IsNullOrEmpty(encryptedParams)) return null;
            try
            {
                byte[] encryptedBlob = Convert.FromBase64String(encryptedParams);
                byte[] decryptedBlob = ProtectedData.Unprotect(encryptedBlob, GetStrongNameKey(), DataProtectionScope.CurrentUser);
                string decryptedParams = Encoding.UTF8.GetString(decryptedBlob);
                return JsonConvert.DeserializeObject<TransferParameters>(decryptedParams);
            }
            catch
            {
                return null;
            }
        }

        internal static void SaveTransferParameters(TransferParameters transferParams)
        {
            string serialized = JsonConvert.SerializeObject(transferParams);
            byte[] blob = Encoding.UTF8.GetBytes(serialized);
            byte[] cryptedBlob = ProtectedData.Protect(blob, GetStrongNameKey(), DataProtectionScope.CurrentUser);
            string cryptedParams = Convert.ToBase64String(cryptedBlob);
            Properties.Settings.Default.Credentials = cryptedParams;
            Properties.Settings.Default.Save();
        }
    }
}