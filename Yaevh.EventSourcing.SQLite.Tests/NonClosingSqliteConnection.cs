using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.SQLite.Tests
{
    public class NonClosingSqliteConnection : SqliteConnection
    {
        public NonClosingSqliteConnection(string connectionString) : base(connectionString) { }
        public override void Close() { }
    }
}
