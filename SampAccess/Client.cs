using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Runtime.Versioning;

namespace SampAccess
{
    /// <summary>
    /// This class can get or set settings of your SAMP client.
    /// 
    /// <para>It needs Microsoft.Win32.Registry package to access <b>Windows Registry</b></para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class Client
    {
        /// <summary>
        /// "Player name" value in the SAMP client
        /// </summary>
        /// <returns></returns>
        public static string PlayerName
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", false);
                return key.GetValue("PlayerName") as string;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("PlayerName", value);
            }
        }

        /// <summary>
        /// "Path to game executable" value in the SAMP client
        /// </summary>
        public static string GameExecutable
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP");
                return key.GetValue("gta_sa_exe") as string;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("gta_sa_exe", value);
            }
        }

        /// <summary>
        /// "Save RCON passwords" checkbox in the SAMP client 
        /// </summary>
        public static int? SaveRconPasswords
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP");
                return key.GetValue("SaveRconPasses") as int?;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("SaveRconPasses", value);
            }
        }

        /// <summary>
        /// "Save server passwords" checkbox in the SAMP client 
        /// </summary>
        public static int? SaveServerPasswords
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP");
                return key.GetValue("SaveServPasses") as int?;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("SaveServPasses", value);
            }
        }
    }
}
