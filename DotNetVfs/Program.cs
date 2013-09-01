using DotNetVfs.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DotNetVfs
{
	unsafe class HelloWorld
	{
		Stream Stream;
		TextWriter Logger;

		public HelloWorld()
		{
			Stream = File.Open("/home/dotnetvfs/log.txt", FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			Logger = new StreamWriter(Stream);
			Logger.WriteLine("HelloWorld()"); Logger.Flush(); Stream.Flush();
			Logger.WriteLine("sizeof.stat({0})", sizeof(stat)); Logger.Flush(); Stream.Flush();


			stat.print_offset("st_dev");
			stat.print_offset("__st_ino");
			stat.print_offset("st_mode");
			stat.print_offset("st_nlink");
			stat.print_offset("st_uid");
			stat.print_offset("st_gid");
			stat.print_offset("st_rdev");
			stat.print_offset("st_size");
			stat.print_offset("st_blksize");
			stat.print_offset("st_blocks");
			stat.print_offset("st_atime");
			stat.print_offset("st_mtime");
			stat.print_offset("st_ctime");
			stat.print_offset("st_ino");
		}

		public Result getattr(string path, out stat stbuf)
		{
			//return Result.ENOENT;

			Logger.WriteLine("hello_getattr: '{0}'", path); Logger.Flush(); Stream.Flush();

			stbuf = default(stat);

			if (path == "/")
			{
				stbuf.st_mode = (uint)Mode.S_IFDIR | 0755;
				stbuf.st_nlink = (IntPtr)2;
			}
			else if (path == "/hello")
			{
				stbuf.st_mode = (uint)Mode.S_IFREG | 0444;
				stbuf.st_nlink = (IntPtr)1;
				stbuf.st_size = 100;
			}
			else
			{
				return Result.ENOENT;
			}

			return Result.OK;
		}

		public Result opendir(string path, fuse_file_info* fi)
		{
			Logger.WriteLine("hello_opendir: '{0}'", path);
			return Result.OK;
		}

		public Result readdir(string path, IntPtr buf, fuse_fill_dir_t filler, ulong offset, ref fuse_file_info fi)
		{
			Logger.WriteLine("hello_readdir: '{0}', {1}", path, offset);
			Logger.Flush(); Stream.Flush();

			if (path != "/") return Result.ENOENT;

			//return Result.ENOENT;

			foreach (var String in new[] { ".", "..", "hello" })
			{
				//var Ptr = Marshal.StringToHGlobalAuto(String); filler(buf, Ptr, null, (IntPtr)0);
				var FillResult = filler(buf, String, null, 0);
				//Logger.WriteLine("  : '{0}' : {1}", String, FillResult);
			}
			
			return Result.OK;
		}

		public Result open(string path, ref fuse_file_info fi)
		{
			Logger.WriteLine("hello_open: '{0}'", path); Logger.Flush(); Stream.Flush();

			return Result.OK;
		}

		public int read(string path, byte* buf, IntPtr size, ulong offset, fuse_file_info* fi)
		{
			for (int n = 0; n < size.ToInt32(); n++) buf[n] = (byte)'a';

			return (int)size;
		}
	}

	

	unsafe class Program
	{
		public enum FileEntryType : int
		{
			Folder = 0,
			File = 1,
		}

		[SqliteUnique("md5")]
		public class FileReference
		{
			public string md5;
			public long size;
		}

		[SqliteUnique("directory", "name")]
		public class FileEntry
		{
			public string directory;
			public string name;
			public FileEntryType type;
			public long reference;
			public long ctime;
		}

		class EntryPath
		{
			public readonly string Directory;
			public readonly string BaseName;

			public EntryPath(string Directory, string BaseName)
			{
				this.Directory = Directory;
				this.BaseName = BaseName;
			}

			public EntryPath(string FullPath)
			{
				var Index = FullPath.LastIndexOf("/");
				if (Index < 0)
				{
					Directory = "";
					BaseName = FullPath;
				}
				else
				{
					Directory = FullPath.Substring(0, Index);
					BaseName = FullPath.Substring(Index + 1);
				}
			}

			public string FullPath { get {
				if (String.IsNullOrEmpty(Directory)) return BaseName;
				return Directory + "/" + BaseName;
			} }
		}

		class Tree
		{
			SqliteClient Client;
			SqliteTable<FileEntry> FileEntryTable;
			SqliteTable<FileReference> FileReferenceTable;

			public Tree(SqliteClient Client)
			{
				this.Client = Client;
				this.FileEntryTable = Client.Table<FileEntry>("FileEntry");
				this.FileReferenceTable = Client.Table<FileReference>("FileReference");
			}

			public IEnumerable<FileEntry> GetFilesInFolder(EntryPath Path)
			{
				Console.WriteLine("GetFilesInFolder: '{0}'", Path.FullPath);
				foreach (var File in FileEntryTable.Select("directory=?", Path.FullPath))
				{
					Console.WriteLine("  : '{0}'", File.name);
					yield return File;
				}
			}

			public FileEntry GetFileInFolder(EntryPath Path)
			{
				foreach (var File in FileEntryTable.Select("directory=? AND name=?", Path.Directory, Path.BaseName))
				{
					return File;
				}
				throw (new Exception("Can't find file '" + Path.Directory + '/' + Path.BaseName + "'"));
			}

			private void _CreateFolder(string directory, string name)
			{
				Console.WriteLine("_CreateFolder: '{0}', '{1}'", directory, name);
				FileEntryTable.Insert(new FileEntry()
				{
					directory = directory,
					name = name,
					type = FileEntryType.Folder,
					reference = 0,
				});

			}

			public void CreateFolder(EntryPath Path)
			{
				_CreateFolder(Path.Directory, Path.BaseName);
				_CreateFolder(Path.FullPath, null);
			}

			public void AddFile(EntryPath Path)
			{
				AddFile(new FileEntry()
				{
					directory = Path.Directory,
					name = Path.BaseName,
					type = FileEntryType.File,
					reference = 0,
				});
			}

			public void AddFile(FileEntry FileEntry)
			{
				FileEntryTable.Insert(FileEntry);
			}
		}

		class TreeFileSystem
		{
			Tree Tree;

			public TreeFileSystem(Tree Tree)
			{
				this.Tree = Tree;
			}

			public Result readdir(string path, IntPtr buf, fuse_fill_dir_t filler, ulong offset, ref fuse_file_info fi)
			{
				int Count = 0;

				foreach (var Entry in Tree.GetFilesInFolder(new EntryPath(path)))
				{
					if (Entry.name == null)
					{
						filler(buf, ".", null, 0);
						filler(buf, "..", null, 0);
					}
					else
					{
						filler(buf, Entry.name, null, 0);
					}

					Count++;
				}

				return (Count == 0) ? Result.ENOENT : Result.OK;
			}

			public Result getattr(string path, out stat stbuf)
			{
				stbuf = default(stat);

				try
				{
					var Entry = Tree.GetFileInFolder(new EntryPath(path));

					switch (Entry.type)
					{
						case FileEntryType.Folder:
							stbuf.st_mode = (uint)Mode.S_IFDIR | 0755;
							stbuf.st_nlink = (IntPtr)2;
							break;
						case FileEntryType.File:
							stbuf.st_mode = (uint)Mode.S_IFREG | 0444;
							stbuf.st_nlink = (IntPtr)1;
							stbuf.st_size = 100;
							break;
					}

					return Result.OK;
				}
				catch (Exception)
				{
					return Result.ENOENT;
				}
			}

			public int mkdir(string path, uint mode)
			{
				Tree.CreateFolder(new EntryPath(path));
				return 0;
			}
		}

		//static void Test()
		//{
		//	using (var Client = new SqliteClient("test.db"))
		//	{
		//		var Tree = new Tree(Client);
		//		Tree.CreateFolder(new EntryPath("test"));
		//		Tree.AddFile(new EntryPath("test", "demo"));
		//		foreach (var file in Tree.GetFilesInFolder("test"))
		//		{
		//			Console.WriteLine(file.directory + "/" + file.name + " : " + file.type);
		//		}
		//	}
		//
		//	Console.ReadKey();
		//	Environment.Exit(0);
		//}

		static void Main(string[] args)
		{
			//var FileSystem = new TreeFileSystem(new Tree(new SqliteClient("test.db")));
			var FileSystem = new TreeFileSystem(new Tree(new SqliteClient(":memory:")));

			FileSystem.mkdir("/1", 0777);
			var stat = default(stat);
			var fuse_file_info = default(fuse_file_info);
			FileSystem.getattr("/", out stat);
			FileSystem.readdir("/", IntPtr.Zero, (buf, name, stbuf, off) =>
			{
				Console.WriteLine("{0}", name);
				return 0;
			}, 0, ref fuse_file_info);

			Fuse.main(new FuseOperations()
			{
				mkdir = FileSystem.mkdir,
				getattr = FileSystem.getattr,
				readdir = FileSystem.readdir,
				//open = FileSystem.open,
				//read = FileSystem.read,
			});
		}
	}
}
