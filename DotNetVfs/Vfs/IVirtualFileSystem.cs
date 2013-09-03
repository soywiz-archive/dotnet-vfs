using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetVfs.Vfs
{
	public interface IVirtualFileSystem
	{
		IVirtualFileStat GetAttribute(string Path);
		string ReadSymbolicLink(string Path);
		void CreateDirectory(string Path, VirtualFileMode Mode);
		void DeleteFile(string Path);
		void DeleteDirectory(string Path);
		void CreateSymbolicLink(string Path, string Name);
		void Rename(string Source, string Destination);
		void CreateHardLink(string Path, string Name);
		void ChangeMode(string Path, VirtualFileMode Mode);
		void ChangeOwner(string Path, VirtualFileUser User, VirtualFileGroup Group);
		void Truncate(string Path, long Size);
		void SetFileModificationTime(string Path, DateTime AccessTime, DateTime ModificationTime);

		IVirtualFileSystemStats GetFileSystemStats();

		IVirtualFileHandle Open(string Path);
		void Read(string Path, byte* Buffer, int Size, long Offset, IVirtualFileHandle Handle);
		void Write(string Path, byte* Buffer, int Size, long Offset, IVirtualFileHandle Handle);
		void Flush(string Path, IVirtualFileHandle Handle);
		void FileSync(string Path, IVirtualFileHandle Handle);

		//public delegate int release(string path,  fuse_file_info *);
		//public delegate int setxattr(string path, string, string, size_t, int);
		//public delegate int getxattr(string path, string name, char *value, size_t vlen);
		//public delegate int listxattr(string path, char *, size_t);
		//public delegate int removexattr(string path, string);
		//[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate Result opendir(string path, fuse_file_info* fi);
		public delegate Result readdir(string path, IntPtr buf, fuse_fill_dir_t filler, __off64_t offset, ref fuse_file_info fi);
		//public delegate int releasedir(string path,  fuse_file_info *fi);
		//public delegate int fsyncdir(string path, int,  fuse_file_info *fi);
		//public delegate void* init(fuse_conn_info *conn);
		//public delegate void destroy(void *);
		//public delegate int access(string path, int);
		//public delegate int create(string path, mode_t,  fuse_file_info *);
		//public delegate int ftruncate(string path, off_t,  fuse_file_info *);
		//public delegate int fgetattr(string path,  stat *,  fuse_file_info *);
		//public delegate int @lock(string path,  fuse_file_info *, int cmd,  flock *);
		//public delegate int utimens(string path, const  timespec tv[2]);
		//public delegate int bmap(string path, size_t blocksize, uint64_t *idx);
		//public delegate int ioctl(string path, int cmd, void *arg,  fuse_file_info *, uint flags, void *data);
		//public delegate int poll(string path,  fuse_file_info *,  fuse_pollhandle *ph, unsigned *reventsp);
	}
}
