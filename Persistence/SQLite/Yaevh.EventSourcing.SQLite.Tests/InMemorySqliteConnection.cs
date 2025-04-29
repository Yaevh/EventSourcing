using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yaevh.EventSourcing.SQLite.Tests
{
    /// <summary>
    /// An in-memory <see cref="SqliteConnection"/> that keeps the DB alive
    /// between subsequent calls to the connection factory
    /// </summary>
    /// <remarks>
    /// When using :inmemory: SQLite DB, the DB is deleted when the connection is closed;
    /// however, we want it to be kept alive between subsequent calls in testing scenarios
    /// </remarks>
    public class InMemorySqliteConnection : SqliteConnection
    {
        public InMemorySqliteConnection() : base("DataSource=:memory:") { }
        
        public override void Close()
        {
            // don't close the connection
        }
    }
}
