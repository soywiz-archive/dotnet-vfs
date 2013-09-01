using uint64_t = System.UInt64;
using uint32_t = System.UInt32;
//using off_t = System.IntPtr;
using __off64_t = System.UInt64;
using size_t = System.IntPtr;
using dev_t = System.UInt64;
using time_t = System.IntPtr;
using mode_t = System.UInt32;
using nlink_t = System.IntPtr;
using ino_t = System.IntPtr;
using __ino64_t = System.UInt64;
using uid_t = System.UInt32;
using gid_t = System.UInt32;
using blksize_t = System.IntPtr;
using blkcnt_t = System.IntPtr;
using __blkcnt64_t = System.UInt64;


/*

 * 64-BITS:
	off_t:8
	unsigned:4
	unsigned long:8
	dev_t:8
	time_t:8
	mode_t:4
	nlink_t:8
	ino_t:8
	uid_t:4
	gid_t:4
	blksize_t:8
	blkcnt_t:8
	stat:144
 * 32-BITS:
	off_t:4
	unsigned:4
	unsigned long:4
	dev_t:8
	time_t:4
	mode_t:4
	nlink_t:4
	ino_t:4
	uid_t:4
	gid_t:4
	blksize_t:4
	blkcnt_t:4
	stat:88

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace DotNetVfs
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	unsafe public delegate int fuse_fill_dir_t(IntPtr buf, string name, stat* stbuf, __off64_t off);
	//unsafe public delegate int fuse_fill_dir_t(IntPtr buf, IntPtr name, stat* stbuf, off_t off);
	//public delegate int fuse_dirfil_t(fuse_dirh_t h, string name, int type, ino_t ino);

	public enum OperationsFlags : uint
	{
		flag_nullpath_ok = (1 << 0),
	}

	unsafe public struct fuse_conn_info
	{
		public uint proto_major;
		public uint proto_minor;
		public uint async_read;
		public uint max_write;
		public uint max_readahead;
		public uint capable;
		public uint want;
		public fixed uint  reserved[25];
	}

	public enum FileInfoFlags : uint
	{
		direct_io = (1 << 0),
		keep_cache = (1 << 1),
		flush = (1 << 2),
		nonseekable = (1 << 3),
	}

	public struct fuse_file_info
	{
		public int flags;
		public IntPtr fh_old;
		public int writepage;
		public FileInfoFlags Flags;
		public uint64_t fh;
		public uint64_t lock_owner;
	}

	[StructLayout(LayoutKind.Sequential, Size = 4)]
	public struct time_t
	{
		public uint tv_sec;
		public uint tv_nsec;
	}

	[StructLayout(LayoutKind.Sequential, Size = 104)]
	public struct stat
	{
		public dev_t st_dev;     /* ID of device containing file */
		private uint __pad1;
		public ino_t __st_ino;     /* inode number */
		public mode_t st_mode;    /* protection */
		public nlink_t st_nlink;   /* number of hard links */
		public uid_t st_uid;     /* user ID of owner */
		public gid_t st_gid;     /* group ID of owner */
		public dev_t st_rdev;    /* device ID (if special file) */
		private uint __pad2;
		public __off64_t st_size;    /* total size, in bytes */
		public blksize_t st_blksize; /* blocksize for file system I/O */
		public __blkcnt64_t st_blocks;  /* number of 512B blocks allocated */
		public time_t st_atime;   /* time of last access */
		public time_t st_mtime;   /* time of last modification */
		public time_t st_ctime;   /* time of last status change */
		public __ino64_t st_ino;

		static public void print_offset(string Name)
		{
			if (typeof(stat).GetField(Name) != null)
			{
				Console.WriteLine("{0}:{1} : {2}", Name, Marshal.OffsetOf(typeof(stat), Name), Marshal.SizeOf(typeof(stat).GetField(Name).FieldType));
			}
		}

		public override string ToString()
		{
			return String.Format(
				"stat(dev={0})",
				st_dev
			);
		}
	}
	
	public enum Mode : uint
	{
		S_IFDIR = 0x4000,  /* directory */
		S_IFIFO = 0x1000,  /* FIFO special */
		S_IFCHR = 0x2000,  /* character special */
		S_IFBLK = 0x3000,  /* block special */
		S_IFREG = 0x8000,  /* or just 0x0000, regular */
	}

	public enum Result : int
	{
		OK              = - 0,
		EPERM           = - 1,      /* Operation not permitted */
		ENOENT          = - 2,      /* No such file or directory */
		ESRCH           = - 3,      /* No such process */
		EINTR           = - 4,      /* Interrupted system call */
		EIO             = - 5,      /* I/O error */
		ENXIO           = - 6,      /* No such device or address */
		E2BIG           = - 7,      /* Arg list too long */
		ENOEXEC         = - 8,      /* Exec format error */
		EBADF           = - 9,      /* Bad file number */
		ECHILD          = -10,      /* No child processes */
		EAGAIN          = -11,      /* Try again */
		ENOMEM          = -12,      /* Out of memory */
		EACCES          = -13,      /* Permission denied */
		EFAULT          = -14,      /* Bad address */
		ENOTBLK         = -15,      /* Block device required */
		EBUSY           = -16,      /* Device or resource busy */
		EEXIST          = -17,      /* File exists */
		EXDEV           = -18,      /* Cross-device link */
		ENODEV          = -19,      /* No such device */
		ENOTDIR         = -20,      /* Not a directory */
		EISDIR          = -21,      /* Is a directory */
		EINVAL          = -22,      /* Invalid argument */
		ENFILE          = -23,      /* File table overflow */
		EMFILE          = -24,      /* Too many open files */
		ENOTTY          = -25,      /* Not a typewriter */
		ETXTBSY         = -26,      /* Text file busy */
		EFBIG           = -27,      /* File too large */
		ENOSPC          = -28,      /* No space left on device */
		ESPIPE          = -29,      /* Illegal seek */
		EROFS           = -30,      /* Read-only file system */
		EMLINK          = -31,      /* Too many links */
		EPIPE           = -32,      /* Broken pipe */
		EDOM            = -33,      /* Math argument out of domain of func */
		ERANGE          = -34,      /* Math result not representable */
		EDEADLK         = -35,      /* Resource deadlock would occur */
		ENAMETOOLONG    = -36,      /* File name too long */
		ENOLCK          = -37,      /* No record locks available */
		ENOSYS          = -38,      /* Function not implemented */
		ENOTEMPTY       = -39,      /* Directory not empty */
		ELOOP           = -40,      /* Too many symbolic links encountered */
		EWOULDBLOCK     = EAGAIN,  /* Operation would block */
		ENOMSG          = -42,      /* No message of desired type */
		EIDRM           = -43,      /* Identifier removed */
		ECHRNG          = -44,      /* Channel number out of range */
		EL2NSYNC        = -45,      /* Level 2 not synchronized */
		EL3HLT          = -46,      /* Level 3 halted */
		EL3RST          = -47,      /* Level 3 reset */
		ELNRNG          = -48,      /* Link number out of range */
		EUNATCH         = -49,      /* Protocol driver not attached */
		ENOCSI          = -50,      /* No CSI structure available */
		EL2HLT          = -51,      /* Level 2 halted */
		EBADE           = -52,      /* Invalid exchange */
		EBADR           = -53,      /* Invalid request descriptor */
		EXFULL          = -54,      /* Exchange full */
		ENOANO          = -55,      /* No anode */
		EBADRQC         = -56,      /* Invalid request code */
		EBADSLT         = -57,      /* Invalid slot */
						
		EDEADLOCK       = EDEADLK,
						
		EBFONT          = -59,      /* Bad font file format */
		ENOSTR          = -60,      /* Device not a stream */
		ENODATA         = -61,      /* No data available */
		ETIME           = -62,      /* Timer expired */
		ENOSR           = -63,      /* Out of streams resources */
		ENONET          = -64,      /* Machine is not on the network */
		ENOPKG          = -65,      /* Package not installed */
		EREMOTE         = -66,      /* Object is remote */
		ENOLINK         = -67,      /* Link has been severed */
		EADV            = -68,      /* Advertise error */
		ESRMNT          = -69,      /* Srmount error */
		ECOMM           = -70,      /* Communication error on send */
		EPROTO          = -71,      /* Protocol error */
		EMULTIHOP       = -72,      /* Multihop attempted */
		EDOTDOT         = -73,      /* RFS specific error */
		EBADMSG         = -74,      /* Not a data message */
		EOVERFLOW       = -75,      /* Value too large for defined data type */
		ENOTUNIQ        = -76,      /* Name not unique on network */
		EBADFD          = -77,      /* File descriptor in bad state */
		EREMCHG         = -78,      /* Remote address changed */
		ELIBACC         = -79,      /* Can not access a needed shared library */
		ELIBBAD         = -80,      /* Accessing a corrupted shared library */
		ELIBSCN         = -81,      /* .lib section in a.out corrupted */
		ELIBMAX         = -82,      /* Attempting to link in too many shared libraries */
		ELIBEXEC        = -83,      /* Cannot exec a shared library directly */
		EILSEQ          = -84,      /* Illegal byte sequence */
		ERESTART        = -85,      /* Interrupted system call should be restarted */
		ESTRPIPE        = -86,      /* Streams pipe error */
		EUSERS          = -87,      /* Too many users */
		ENOTSOCK        = -88,      /* Socket operation on non-socket */
		EDESTADDRREQ    = -89,      /* Destination address required */
		EMSGSIZE        = -90,      /* Message too long */
		EPROTOTYPE      = -91,      /* Protocol wrong type for socket */
		ENOPROTOOPT     = -92,      /* Protocol not available */
		EPROTONOSUPPORT = -93,      /* Protocol not supported */
		ESOCKTNOSUPPORT = -94,      /* Socket type not supported */
		EOPNOTSUPP      = -95,      /* Operation not supported on transport endpoint */
		EPFNOSUPPORT    = -96,      /* Protocol family not supported */
		EAFNOSUPPORT    = -97,      /* Address family not supported by protocol */
		EADDRINUSE      = -98,      /* Address already in use */
		EADDRNOTAVAIL   = -99,      /* Cannot assign requested address */
		ENETDOWN        = -100,     /* Network is down */
		ENETUNREACH     = -101,     /* Network is unreachable */
		ENETRESET       = -102,     /* Network dropped connection because of reset */
		ECONNABORTED    = -103,     /* Software caused connection abort */
		ECONNRESET      = -104,     /* Connection reset by peer */
		ENOBUFS         = -105,     /* No buffer space available */
		EISCONN         = -106,     /* Transport endpoint is already connected */
		ENOTCONN        = -107,     /* Transport endpoint is not connected */
		ESHUTDOWN       = -108,     /* Cannot send after transport endpoint shutdown */
		ETOOMANYREFS    = -109,     /* Too many references: cannot splice */
		ETIMEDOUT       = -110,     /* Connection timed out */
		ECONNREFUSED    = -111,     /* Connection refused */
		EHOSTDOWN       = -112,     /* Host is down */
		EHOSTUNREACH    = -113,     /* No route to host */
		EALREADY        = -114,     /* Operation already in progress */
		EINPROGRESS     = -115,     /* Operation now in progress */
		ESTALE          = -116,     /* Stale NFS file handle */
		EUCLEAN         = -117,     /* Structure needs cleaning */
		ENOTNAM         = -118,     /* Not a XENIX named type file */
		ENAVAIL         = -119,     /* No XENIX semaphores available */
		EISNAM          = -120,     /* Is a named type file */
		EREMOTEIO       = -121,     /* Remote I/O error */
		EDQUOT          = -122,     /* Quota exceeded */
						
		ENOMEDIUM       = -123,     /* No medium found */
		EMEDIUMTYPE     = -124,     /* Wrong medium type */
	}

	unsafe public class Delegates
	{
		public delegate Result getattr(string path, out stat stbuf);
		//public delegate int readlink(string path, string link, size_t);
		//public delegate int getdir(string path, fuse_dirh_t, fuse_dirfil_t);
		//public delegate int mknod(string path, mode_t, dev_t);
		//public delegate int mkdir(string path, mode_t);
		//public delegate int unlink(string path);
		//public delegate int rmdir(string path);
		//public delegate int symlink(string path, string Name);
		//public delegate int rename(string path, string);
		//public delegate int link(string path, string);
		//public delegate int chmod(string path, mode_t);
		//public delegate int chown(string path, uid_t, gid_t);
		//public delegate int truncate(string path, off_t);
		//public delegate int utime(string path,  utimbuf *);
		public delegate Result open(string path, ref fuse_file_info fi);
		public delegate int read(string path, byte* buf, size_t size, __off64_t offset, fuse_file_info* fi);
		//public delegate int write(string path, string, size_t, off_t,  fuse_file_info *);
		//public delegate int statfs(string path,  statvfs *);
		//public delegate int flush(string path,  fuse_file_info * info );
		//public delegate int release(string path,  fuse_file_info *);
		//public delegate int fsync(string path, int,  fuse_file_info *);
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

	unsafe public class Fuse
	{
		const string LIB = "libfuse.so";
		
		[DllImport(LIB, CallingConvention = CallingConvention.Cdecl)]
		static private extern int fuse_main_real(int argc, IntPtr argv, ref fuse_operations op, size_t op_size, IntPtr user_data);

		static public void main(ref fuse_operations op)
		{
			var args = Environment.GetCommandLineArgs();
			//Console.WriteLine(args.Length);
			//Console.WriteLine(String.Join("\n", args));
			var argv = AllocStringArray(args);
			try
			{
				fuse_main_real(args.Length, argv, ref op, (IntPtr)sizeof(fuse_operations), IntPtr.Zero);
			}
			finally
			{
				FreeStringArray(args.Length, argv);
			}
		}

		private static IntPtr AllocStringArray(string[] args)
		{
			IntPtr argv = Marshal.AllocHGlobal((args.Length + 1) * IntPtr.Size);

			for (int i = 0; i < args.Length; ++i)
			{
				Marshal.WriteIntPtr(argv, i * IntPtr.Size,
						Marshal.StringToHGlobalAuto(args[i]));
			}
			Marshal.WriteIntPtr(argv, args.Length * IntPtr.Size, IntPtr.Zero);
			return argv;
		}

		private static void FreeStringArray(int argc, IntPtr argv)
		{
			if (argv == IntPtr.Zero) return;

			for (int i = 0; i < argc; ++i)
			{
				IntPtr p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
				Marshal.FreeHGlobal(p);
			}
			Marshal.FreeHGlobal(argv);
		}

	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	public struct fuse_operations
	{
		public IntPtr getattr;
		public IntPtr readlink;
		public IntPtr getdir;
		public IntPtr mknod;
		public IntPtr mkdir;
		public IntPtr unlink;
		public IntPtr rmdir;
		public IntPtr symlink;
		public IntPtr rename;
		public IntPtr link;
		public IntPtr chmod;
		public IntPtr chown;
		public IntPtr truncate;
		public IntPtr utime;
		public IntPtr open;
		public IntPtr read;
		public IntPtr write;
		public IntPtr statfs;
		public IntPtr flush;
		public IntPtr release;
		public IntPtr fsync;
		public IntPtr setxattr;
		public IntPtr getxattr;
		public IntPtr listxattr;
		public IntPtr removexattr;
		public IntPtr opendir;
		public IntPtr readdir;
		public IntPtr releasedir;
		public IntPtr fsyncdir;
		public IntPtr init;
		public IntPtr destroy;
		public IntPtr access;
		public IntPtr create;
		public IntPtr ftruncate;
		public IntPtr fgetattr;
		public IntPtr @lock;
		public IntPtr utimens;
		public IntPtr bmap;
		public OperationsFlags flag;
		public IntPtr ioctl;
		public IntPtr poll;
	}

}
