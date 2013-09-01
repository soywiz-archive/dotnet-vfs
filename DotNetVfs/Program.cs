using Community.CsharpSqlite;
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
		static void Test()
		{
			using (var Client = new SqliteClient("test.db"))
			{
				//Client.Exec("DROP TABLE file_reference;");
				Client.Exec("CREATE TABLE IF NOT EXISTS file_reference (md5, size);");
				Client.Exec("CREATE TABLE IF NOT EXISTS file_history (path, reference, );");
				Client.Insert("file_reference", new Dictionary<string, object>()
				{
					{ "md5", "mymd5" },
					{ "size", 10000 },
				});

				foreach (var Row in Client.Query("SELECT md5, size FROM file_reference;"))
				{
					Console.WriteLine("Row: {0}, {1}", Row[0], Row[1]);
				}
			}

			Console.ReadKey();
			Environment.Exit(0);
		}

		static void Main(string[] args)
		{
			Test();

			//File.WriteAllText("log.txt", "LOL!");
			var HelloWorld = new HelloWorld();
			fuse_operations hello_oper = new fuse_operations()
			{
				getattr = Marshal.GetFunctionPointerForDelegate((Delegate)(Delegates.getattr)HelloWorld.getattr),
				readdir = Marshal.GetFunctionPointerForDelegate((Delegate)(Delegates.readdir)HelloWorld.readdir),
				open = Marshal.GetFunctionPointerForDelegate((Delegate)(Delegates.open)HelloWorld.open),
				read = Marshal.GetFunctionPointerForDelegate((Delegate)(Delegates.read)HelloWorld.read),
			};

			Fuse.main(ref hello_oper);
		}
	}
}
