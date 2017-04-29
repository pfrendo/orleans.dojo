using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Orleans;

namespace PF.Dojo.StorageProviders.MongoDb
{
	/// <summary>
	///     Interfaces with a MongoDB database driver.
	/// </summary>
	public class GrainStateMongoDataManager
	{
		private static readonly ConcurrentDictionary<string, bool> RegisterIndexMap =
			new ConcurrentDictionary<string, bool>();

		public static JsonSerializerSettings JsonSetting = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Converters = new List<JsonConverter>()
		};

		private readonly IMongoDatabase _database;

		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="connectionString">A database name.</param>
		/// <param name="databaseName">A MongoDB database connection string.</param>
		public GrainStateMongoDataManager(string databaseName, string connectionString)
		{
			var client = new MongoClient(connectionString);
			_database = client.GetDatabase(databaseName);
		}

		/// <summary>
		///     Deletes a file representing a grain state object.
		/// </summary>
		/// <param name="collectionName">The type of the grain state object.</param>
		/// <param name="key">The grain id string.</param>
		/// <returns>Completion promise for this operation.</returns>
		public Task DeleteAsync(string collectionName, string key)
		{
			var collection = _database.GetCollection<BsonDocument>(collectionName);
			if (collection == null)
				return TaskDone.Done;

			var query = BsonDocument.Parse("{key:\"" + key + "\"}");
			collection.FindOneAndDeleteAsync(query);

			return TaskDone.Done;
		}

		/// <summary>
		///     Reads a file representing a grain state object.
		/// </summary>
		/// <param name="collectionName">The type of the grain state object.</param>
		/// <param name="key">The grain id string.</param>
		/// <returns>Completion promise for this operation.</returns>
		public async Task<string> ReadAsync(string collectionName, string key)
		{
			var collection = await GetCollection(collectionName);

			if (collection == null)
				return null;

			var query = BsonDocument.Parse("{__key:\"" + key + "\"}");
			using (var cursor = await collection.FindAsync(query))
			{
				var existing = (await cursor.ToListAsync()).FirstOrDefault();

				if (existing == null)
					return null;

				existing.Remove("_id");
				existing.Remove("__key");

				return existing.ToJson();
			}
		}

		/// <summary>
		///     Writes a file representing a grain state object.
		/// </summary>
		/// <param name="collectionName">The type of the grain state object.</param>
		/// <param name="key">The grain id string.</param>
		/// <param name="entityData">The grain state data to be stored./</param>
		/// <returns>Completion promise for this operation.</returns>
		public async Task WriteAsync(string collectionName, string key, IGrainState entityData)
		{
			var collection = await GetCollection(collectionName);

			var query = BsonDocument.Parse("{__key:\"" + key + "\"}");

			using (var cursor = await collection.FindAsync(query))
			{
				var existing = (await cursor.ToListAsync()).FirstOrDefault();

				var json = JsonConvert.SerializeObject(entityData, JsonSetting);

				var doc = BsonSerializer.Deserialize<BsonDocument>(json);
				doc["__key"] = key;

				if (existing != null)
				{
					doc["_id"] = existing["_id"];
					await collection.ReplaceOneAsync(query, doc);
				}
				else
				{
					await collection.InsertOneAsync(doc);
				}
			}
		}

		private async Task<IMongoCollection<BsonDocument>> GetCollection(string name)
		{
			var collection = _database.GetCollection<BsonDocument>(name);

			if (RegisterIndexMap.ContainsKey(name)) return collection;

			using (var cursor = await collection.Indexes.ListAsync())
			{
				var indexes = await cursor.ToListAsync();
				if (indexes.Count(index => index["name"] == "__key_1") == 0)
				{
					var keys = Builders<BsonDocument>.IndexKeys.Ascending("__key");
					await collection.Indexes.CreateOneAsync(keys,
						new CreateIndexOptions {Unique = true, Version = 1});
				}
				RegisterIndexMap.TryAdd(name, true);
			}
			return collection;
		}
	}
}