﻿using Realms;

namespace Primagaz.Standard
{
    public sealed class Db
    {
        static readonly Db _instance = new Db();

        public Realm CurrentRealm { get; }

        Db()
        {
            var config = new RealmConfiguration("primagaz.realm")
            {
                // key MUST be exactly this size
                EncryptionKey = new byte[64]
                {
                    0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
                    0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18,
                    0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 0x28,
                    0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                    0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                    0x71, 0x72, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
                    0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
                    0x51, 0x52, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58
                },
                SchemaVersion = 0
            };

            CurrentRealm = Realm.GetInstance(config);


            CurrentRealm.Error += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine(e.Exception.Message);
            };
        }

        public static Db Instance
        {
            get
            {
                return _instance;
            }
        }

    }
}
