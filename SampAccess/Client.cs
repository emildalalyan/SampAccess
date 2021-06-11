using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows;
using System.Runtime.Versioning;

namespace SampAccess
{
    /// <summary>
    /// Static class, which can set or get some <b>SAMP client parameters</b>
    /// <para>It is gathering and changing information through <b>Windows Registry</b></para>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class Client
    {
        /// <summary>
        /// <b>Player name</b> in the Windows version of SAMP client
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
        /// Path to <b>game executable</b> in the Windows version of SAMP client
        /// </summary>
        /// <param name="output"></param>
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
        /// <b>Save RCON passwords</b> checkbox in Windows version of SAMP client 
        /// </summary>
        /// <param name="output"></param>
        public static bool? SaveRconPasswords
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP");
                return key.GetValue("SaveRconPasses") as bool?;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("SaveRconPasses", value);
            }
        }

        /// <summary>
        /// <b>Save server passwords</b> checkbox in Windows version of SAMP client 
        /// </summary>
        /// <param name="output"></param>
        public static bool? SaveServerPasswords
        {
            get
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP");
                return key.GetValue("SaveServPasses") as bool?;
            }
            set
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\SAMP", true);
                key.SetValue("SaveServPasses", value);
            }
        }
    }
}
