using MssqlToolBox.Models;

namespace MssqlToolBox.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;

    internal class CredentialManager
    {
        private static readonly object Lock = new();
        private static CredentialManager? _instance;
        private readonly Dictionary<string, DatabaseConnectionInfo> _connections;
        private DatabaseConnectionInfo? _activeConnection;
        private string? _masterKey;
        private const string ConnectionsFileName = "connections";

        private CredentialManager()
        {
            _connections = new Dictionary<string, DatabaseConnectionInfo>();
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
                if (IsSetMasterKey())
                    SaveConnectionsToFile();
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
        private void SaveConnectionsToFile()
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(_connections);

                var encryptedData = EncryptionHelper.EncryptString(jsonData, _masterKey);

                File.WriteAllText(ConnectionsFileName, encryptedData);
            }
            catch
            {
                lock (Lock)
                {
                    _connections.Clear();
                }
                throw;
            }
        }

        public void LoadConnectionsFromFile()
        {
            if (!IsConnectionFileExists())
                return;
            var encryptedData = File.ReadAllText(ConnectionsFileName);
            var decryptedData = EncryptionHelper.DecryptString(encryptedData, _masterKey);
            var connections = JsonSerializer.Deserialize<Dictionary<string, DatabaseConnectionInfo>>(decryptedData);

            foreach (var connection in connections)
            {
                _connections[connection.Key] = connection.Value;
            }
        }

        public void SetMasterKey(string? masterKey)
        {
            lock (Lock)
            {
                _masterKey = masterKey;
            }
        }
        public bool IsSetMasterKey()
        {
            lock (Lock)
            {
                return _masterKey != null;
            }
        }
        public bool IsConnectionFileExists()
        {
            return File.Exists(ConnectionsFileName);
        }
    }

}
