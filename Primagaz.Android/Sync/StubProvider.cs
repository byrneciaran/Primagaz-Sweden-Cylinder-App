using System;
using Android.Content;
using Android.Database;

namespace Primagaz.Android.Sync
{
    [ContentProvider(new[] { "se.primagaz.cylinder.provider" }, Exported = true, Syncable = true)]
    public class StubProvider : ContentProvider
    {
        public override int Delete(global::Android.Net.Uri uri, string selection, string[] selectionArgs)
        {
            return 0;
        }

        public override string GetType(global::Android.Net.Uri uri)
        {
            return null;
        }

        public override global::Android.Net.Uri Insert(global::Android.Net.Uri uri, ContentValues values)
        {
            return null;
        }

        public override bool OnCreate()
        {
            return true;
        }

        public override ICursor Query(global::Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {
            return null;
        }

        public override int Update(global::Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
        {
            throw new NotImplementedException();
        }
    }
}
