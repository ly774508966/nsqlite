using NPlugins.Sqlite;
using System;
using UnityEngine;

namespace NSqliteDemo
{
    class OtherModel : Storable
    {
        public DateTime Date { get; set; }

        public bool CanWrite { get; set; }

        public Vector3 Position { get; set; }
    }
}
