using MssqlToolBox.Models;

namespace MssqlToolBox.Helpers
{
    using System;
    using System.Collections.Generic;

    internal class CredentialManager
    {
        private static readonly object Lock = new();
        private static CredentialManager? _instance;
        private readonly Dictionary<string, DatabaseConnectionInfo> _connections;
        private DatabaseConnectionInfo? _activeConnection;

        private CredentialManager()
        {
            _connections = new Dictionary<string, DatabaseConnectionInfo>();
            _activeConnection = null;
        }

        public static CredentialManager Instance
        {
            get
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CredentialManager();
                    }
                    return _instance;
                }
            }
        }

        public void AddConnection(string name, DatabaseConnectionInfo connection)
        {
            lock (Lock)
            {
                _connections[name] = connection;
            }
        }

        public int GetAvailableConnectionCount()
        {
            lock (Lock)
            {
                return _connections.Count;
            }
        }

        public void ListConnections()
        {
            lock (Lock)
            {
                ConsoleHelpers.WriteLineColoredMessage("Available Connections:", ConsoleColor.DarkYellow);
                for (var i = 0; i < _connections.Count; i++)
                {
                    ConsoleHelpers.WriteLineColoredMessage($"- {i + 1}. {_connections.ElementAt(i).Key}", ConsoleColor.Yellow);
                }
            }
        }

        public List<DatabaseConnectionInfo> GetConnections()
        {
            lock (Lock)
            {
                return _connections.Values.ToList();
            }
        }

        public DatabaseConnectionInfo GetActiveConnection()
        {
            lock (Lock)
            {
                if (_activeConnection != null)
                {
                    return _activeConnection;
                }
                throw new InvalidOperationException("No active connection is set.");
            }
        }

        public void SetActiveConnection(DatabaseConnectionInfo connection)
        {
            lock (Lock)
            {
                _activeConnection = connection;
            }
        }
    }

}
