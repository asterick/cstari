using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

using System.IO;
using Microsoft.Win32;

namespace cstari.utility
{
    static public class ProfileManager
    {
        private class ProfileSettings
        {
            public Dictionary<string,GameProfile> profiles;

            public ProfileSettings()
            {
                profiles = new Dictionary<string, GameProfile>();
                StreamReader f;

                try
                {
                    f = new StreamReader(@"profiles.cfg");
                }
                catch
                {
                    return;
                }

                try
                {
                    string s;

                    do
                    {
                        s = f.ReadLine();

                        if (s == null)
                            break;

                        string[] config = s.Split(new Char[] { '\t' });

                        GameProfile profile = new GameProfile();

                        profile.name = config[1];
                        profile.mapper = config[2];
                        profile.controller_a = config[3];
                        profile.controller_b = config[4];
                        profile.signal = (SignalType)Convert.ToInt32(config[5]);
                        profiles[config[0]] = profile;
                    }
                    while (s != null);
                }
                catch
                {
                    Console.WriteLine("Failed to parse configuration");
                }
                finally
                {
                    f.Close();
                }
            }

            ~ProfileSettings()
            {
                try
                {
                    StreamWriter f = new StreamWriter(@"profiles.cfg");

                    foreach (string md5 in profiles.Keys)
                    {
                        GameProfile profile = profiles[md5];

                        f.WriteLine(
                            "{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
                            md5, 
                            profile.name,
                            profile.mapper,
                            profile.controller_a,
                            profile.controller_b,(
                            int)profile.signal);
                    }

                    f.Close();
                }
                catch
                {
                    Console.WriteLine("Failed to save configuration file");
                }
            }
        };

        public struct GameProfile
        {
            public string name;

            public string controller_a;
            public string controller_b;
            public string mapper;
            public SignalType signal;
        }
                
        static private MD5CryptoServiceProvider md5;
        static private ProfileSettings profiles;


        static ProfileManager()
        {
            md5 = new MD5CryptoServiceProvider();
            profiles = new ProfileSettings();
        }

        static public string GetHash(byte[] data)
        {
            return Convert.ToBase64String(md5.ComputeHash(data));
        }

        static public bool Profiled( string md5 )
        {
            return profiles.profiles.ContainsKey(md5);
        }

        static public void AddProfile(string md5, GameProfile p)
        {
            profiles.profiles[md5] = p;
        }

        static public void RemoveProfile(string md5)
        {
            profiles.profiles.Remove(md5);
        }

        static public GameProfile GetProfile(string md5)
        {
            return profiles.profiles[md5];
        }
    }
}
