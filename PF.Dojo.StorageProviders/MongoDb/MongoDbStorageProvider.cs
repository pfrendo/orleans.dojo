using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Storage;

namespace PF.Dojo.StorageProviders.MongoDb
{
    public class MongoDbStorageProvider : IStorageProvider
    {
        private const string DATA_CONNECTION_STRING = "ConnectionString";
        private const string DATABASE_NAME_PROPERTY = "Database";
        private const string USE_GUID_AS_STORAGE_KEY = "UseGuidAsStorageKey";

        public bool UseGuidAsStorageKey { get; set; }
        public string Database { get; set; }
        public string ConnectionString { get; set; }
        public GrainStateMongoDataManager DataManager { get; set; }
        public string Name { get; set; }
        public Logger Log { get; set; }

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Log = providerRuntime.GetLogger(GetType().FullName);
            Name = name;

            if (!config.Properties.ContainsKey(DATA_CONNECTION_STRING) ||
                !config.Properties.ContainsKey(DATABASE_NAME_PROPERTY))
            {
                throw new ArgumentException("ConnectionString Or Database property not set");
            }

            ConnectionString = config.Properties[DATA_CONNECTION_STRING];
            Database = config.Properties[DATABASE_NAME_PROPERTY];

            UseGuidAsStorageKey = !config.Properties.ContainsKey(USE_GUID_AS_STORAGE_KEY) ||
                                       "true".Equals(config.Properties[USE_GUID_AS_STORAGE_KEY],
                                           StringComparison.OrdinalIgnoreCase);

            DataManager = new GrainStateMongoDataManager(Database, ConnectionString);

            return TaskDone.Done;
        }

        public Task Close()
        {
            DataManager = null;
            return TaskDone.Done;
        }

        public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string extendKey;

            var key = UseGuidAsStorageKey ? grainReference.GetPrimaryKey(out extendKey).ToString() : grainReference.ToKeyString();

            var entityData = await DataManager.ReadAsync(grainType, key);

            if (!string.IsNullOrEmpty(entityData))
            {
                ConvertFromStorageFormat(grainState, entityData);
            }
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string extendKey;

            var key = UseGuidAsStorageKey ? grainReference.GetPrimaryKey(out extendKey).ToString() : grainReference.ToKeyString();

            return DataManager.WriteAsync(grainType, key, grainState);
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (DataManager == null) throw new ArgumentException("DataManager property not initialized");

            string extendKey;

            var key = UseGuidAsStorageKey ? grainReference.GetPrimaryKey(out extendKey).ToString() : grainReference.ToKeyString();

            return DataManager.DeleteAsync(grainType, key);
        }

        protected static void ConvertFromStorageFormat(IGrainState grainState, string entityData)
        {
            //review
            var data = JsonConvert.DeserializeObject(entityData, grainState.GetType(), GrainStateMongoDataManager.JsonSetting);
            var dict = (IGrainState) data;
            grainState.State = dict;
        }
    }
}