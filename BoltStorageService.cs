using System.Collections.Generic;
using System.Threading.Tasks;
using Axlebolt.Bolt.Api;
using Axlebolt.RpcSupport;

namespace Axlebolt.Bolt.Storage
{
	public class BoltStorageService : BoltService<BoltStorageService>
	{
		private readonly StorageRemoteService _storageRemoteService;

		private readonly Dictionary<string, byte[]> _storageFiles;

		public BoltStorageService()
		{
			_storageFiles = new Dictionary<string, byte[]>();
			_storageRemoteService = new StorageRemoteService(BoltPlayerApi.Instance.ClientService);
		}

		public Task WriteFile(string filename, byte[] file)
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_storageRemoteService.WriteFile(filename, file);
				_storageFiles[filename] = file;
			});
		}

		public Task<byte[]> ReadFileFromServer(string filename)
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				byte[] array = _storageRemoteService.ReadFile(filename);
				if (!_storageFiles.ContainsKey(filename))
				{
					_storageFiles.Add(filename, array);
				}
				return array;
			});
		}

		public byte[] ReadFile(string filename)
		{
			return _storageFiles.ContainsKey(filename) ? _storageFiles[filename] : null;
		}

		public Task DeleteFile(string filename)
		{
			return BoltPlayerApi.Instance.Async(delegate
			{
				_storageRemoteService.DeleteFile(filename);
				_storageFiles.Remove(filename);
			});
		}

		public bool IsFileExist(string filename)
		{
			return _storageFiles.ContainsKey(filename);
		}

		internal override void Load()
		{
			List<BoltStorageFile> list = BoltStorageFileMapper.Instance.ToOriginalList(_storageRemoteService.ReadAllFiles());
			_storageFiles.Clear();
			foreach (BoltStorageFile item in list)
			{
				if (Logger.LogDebug)
				{
					Logger.Debug($"Loaded {item.Filename} ({item.File.Length})");
				}
				_storageFiles.Add(item.Filename, item.File);
			}
		}

		internal override void Unload()
		{
			_storageFiles.Clear();
		}
	}
}
