using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetVfs.Vfs
{
	public interface IVirtualFileStat
	{
		long Device { get; }
		long NodeIdentifier { get; }
		uint Mode { get; }
		uint HardLinkCount { get; }
		VirtualFileUser UserId { get; }
		VirtualFileGroup GroupId { get; }
		uint DeviceId { get; }
		long Size { get; }
		long BlockSize { get; }
		long BlockCount { get; }
		DateTime AccessTime { get; }
		DateTime ModificationTime { get; }
		DateTime CreationTime { get; }
	}
}
