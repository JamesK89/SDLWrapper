using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	public enum RWopEventOverrideMode
	{
		/// <summary>
		/// Continue with this request using the given arguments,
		/// including any changes made to the event arguments.
		/// </summary>
		Continue = 0,
		/// <summary>
		/// Override this request and return given Result
		/// from the event arguments.
		/// </summary>
		Ignore
	}

	// This class might be rather pointless in light of
	// System.IO.Stream but I added it for the sake of completeness
	// and for compatibility with native SDL functions that might
	// expect an SDL_RWops stream.
	public class ReadWriteOperation
		: Stream, IDisposable
	{
		private struct RWopFunction<T>
		{
			public IntPtr Pointer;
			public T Delegate;
			public GCHandle Handle;
		}
		
		private RWopFunction<SDLRWopsCloseCallback>
			_closeBaseHandler;
		private RWopFunction<SDLRWopsCloseCallback>
			_closeHandler;
		
		private RWopFunction<SDLRWopsSizeCallback>
			_sizeBaseHandler;
		private RWopFunction<SDLRWopsSizeCallback>
			_sizeHandler;
		
		private RWopFunction<SDLRWopsSeekCallback>
			_seekBaseHandler;
		private RWopFunction<SDLRWopsSeekCallback>
			_seekHandler;
		
		private RWopFunction<SDLRWopsWriteCallback>
			_writeBaseHandler;
		private RWopFunction<SDLRWopsWriteCallback>
			_writeHandler;
		
		private RWopFunction<SDLRWopsReadCallback>
			_readBaseHandler;
		private RWopFunction<SDLRWopsReadCallback>
			_readHandler;
		
		public delegate void OnCloseEventHandler(
			object sender,
			RWopCloseEventArgs e);

		public delegate void OnSeekEventHandler(
			object sender,
			RWopSeekEventArgs e);

		public delegate void OnSizeEventHandler(
			object sender,
			RWopSizeEventArgs e);

		public delegate void OnReadEventHandler(
			object sender,
			RWopReadEventArgs e);
		
		public delegate void OnWriteEventHandler(
			object sender,
			RWopWriteEventArgs e);

		public event OnCloseEventHandler Closing;
		public event OnSeekEventHandler Seeking;
		public event OnSizeEventHandler GettingSize;
		public event OnReadEventHandler Reading;
		public event OnWriteEventHandler Writing;

		protected ReadWriteOperation()
			: base()
		{
			Initialize();

			Handle = SDL_AllocRW();

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			IsOwner = true;

			SetupCallbacks();
		}

		public ReadWriteOperation(string fileName, string fileMode = "r")
			: base()
		{
			Initialize();

			Handle = SDL_RWFromFile(fileName, fileMode);

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			SetupCallbacks();
		}

		public ReadWriteOperation(Stream baseStream)
			: base()
		{
			Initialize();

			BaseStream = baseStream;

			Handle = SDL_AllocRW();

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			IsOwner = true;

			SetupCallbacks();
		}

		public ReadWriteOperation(byte[] data, bool @readonly = false)
			: base()
		{
			Initialize();

			if (data != null && data.Length > 0)
			{
				GarbageHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
				Memory = GarbageHandle.AddrOfPinnedObject();

				if (@readonly)
				{
					Handle = SDL_RWFromConstMem(Memory, data.Length);
				}
				else
				{
					Handle = SDL_RWFromMem(Memory, data.Length);
				}

				if (Handle == IntPtr.Zero)
				{
					throw new SDLException();
				}
				
				FreeMemory = true;

				SetupCallbacks();
			}
		}
		
		public ReadWriteOperation(
			IntPtr filePointer,
			bool autoClose = false)
			: base()
		{
			Initialize();

			Handle = SDL_RWFromFP(filePointer, autoClose.ToSDLBool());

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			SetupCallbacks();
		}

		public ReadWriteOperation(
			IntPtr data,
			int length,
			bool @readonly = false)
			: base()
		{
			Initialize();

			if (data != IntPtr.Zero)
			{
				Memory = data;

				if (@readonly)
				{
					Handle = SDL_RWFromConstMem(Memory, length);
				}
				else
				{
					Handle = SDL_RWFromMem(Memory, length);
				}

				if (Handle == IntPtr.Zero)
				{
					throw new SDLException();
				}

				SetupCallbacks();
			}
		}
		
#if !SAFE_AS_POSSIBLE
		public unsafe ReadWriteOperation(
			byte* data,
			int length,
			bool @readonly = false)
			: this(new IntPtr(data), length, @readonly)
		{
		}
#endif

		private void Initialize()
		{
			Handle = IntPtr.Zero;
			Memory = IntPtr.Zero;
			FreeMemory = false;
			IsOwner = false;

			_closeBaseHandler =
				new RWopFunction<SDLRWopsCloseCallback>();
			_closeHandler =
				new RWopFunction<SDLRWopsCloseCallback>();

			_sizeBaseHandler = 
				new RWopFunction<SDLRWopsSizeCallback>();
			_sizeHandler = 
				new RWopFunction<SDLRWopsSizeCallback>();

			_seekBaseHandler = 
				new RWopFunction<SDLRWopsSeekCallback>();
			_seekHandler =
				new RWopFunction<SDLRWopsSeekCallback>();

			_readBaseHandler =
				new RWopFunction<SDLRWopsReadCallback>();
			_readHandler =
				new RWopFunction<SDLRWopsReadCallback>();

			_writeBaseHandler =
				new RWopFunction<SDLRWopsWriteCallback>();
			_writeHandler =
				new RWopFunction<SDLRWopsWriteCallback>();
		}

		public Stream BaseStream
		{
			get;
			private set;
		}

		public uint Type
		{
			get
			{
				uint result = SDL_RWOPS_UNKNOWN;

#if !SAFE_AS_POSSIBLE
				unsafe
				{
					result = ((SDL_RWops*)Handle.ToPointer())->type;
				}
#else
				SDL_RWops ops = Marshal.PtrToStructure<SDL_RWops>(Handle);
				result = ops.type;
#endif

				return result;
			}
		}

		private bool IsOwner
		{
			get;
			set;
		}

		public override long Position
		{
			get
			{
				return SDL_RWtell(Handle);
			}
			set
			{
				if (SDL_RWseek(Handle, value, RW_SEEK_SET) == -1)
				{
					throw new SDLException();
				}
			}
		}

		public override bool CanRead => 
			BaseStream?.CanRead ?? throw new NotImplementedException();

		public override bool CanSeek => 
			BaseStream?.CanSeek ?? throw new NotImplementedException();

		public override bool CanWrite => 
			BaseStream?.CanWrite ?? throw new NotImplementedException();

		public override long Length => 
			_sizeHandler.Delegate.Invoke(Handle);

		private bool FreeMemory
		{
			get;
			set;
		}

		private IntPtr Memory
		{
			get;
			set;
		}

		private GCHandle GarbageHandle
		{
			get;
			set;
		}

		public IntPtr Handle
		{
			get;
			private set;
		}

		protected virtual void OnClose(RWopCloseEventArgs e)
		{
			Closing?.Invoke(this, e);
		}

		private int OnCloseCallback(IntPtr context)
		{
			int result = 0;

			RWopCloseEventArgs args = new RWopCloseEventArgs();

			args.Context = Handle;
			args.Override = RWopEventOverrideMode.Continue;
			args.Result = result;

			OnClose(args);

			if (args.Override == RWopEventOverrideMode.Ignore)
			{
				result = args.Result;
			}
			else
			{
				if (IsOwner)
				{
					result = 0;
					SDL_FreeRW(Handle);
				}
				else if (_closeBaseHandler.Pointer != IntPtr.Zero)
				{
					result = _closeBaseHandler.Delegate.Invoke(
						args.Context);
				}

				if (result == 0)
				{
					Handle = IntPtr.Zero;
					Close();
				}
			}

			return result;
		}

		protected virtual void OnSize(RWopSizeEventArgs e)
		{
			GettingSize?.Invoke(this, e);
		}

		private long OnSizeCallback(IntPtr context)
		{
			long result = 0;

			RWopSizeEventArgs args = new RWopSizeEventArgs();

			args.Context = context;
			args.Override = RWopEventOverrideMode.Continue;
			args.Result = result;

			OnSize(args);

			if (args.Override == RWopEventOverrideMode.Ignore)
			{
				result = args.Result;
			}
			else
			{
				if (_sizeBaseHandler.Pointer != IntPtr.Zero)
				{
					result = _sizeBaseHandler.Delegate.Invoke(
						args.Context);
				}
				else if (BaseStream != null)
				{
					return BaseStream.Length;
				}
			}

			return result;
		}

		protected virtual void OnSeek(RWopSeekEventArgs e)
		{
			Seeking?.Invoke(this, e);
		}

		private long OnSeekCallback(
			IntPtr context,
			long offset,
			int whence)
		{
			long result = 0;

			RWopSeekEventArgs args = new RWopSeekEventArgs();

			args.Context = context;
			args.Offset = offset;
			args.Override = RWopEventOverrideMode.Continue;

			switch (whence)
			{
				case RW_SEEK_CUR:
					args.Origin = SeekOrigin.Current;
					break;
				case RW_SEEK_END:
					args.Origin = SeekOrigin.End;
					break;
				default:
				case RW_SEEK_SET:
					args.Origin = SeekOrigin.Begin;
					break;
			}

			OnSeek(args);

			if (args.Override == RWopEventOverrideMode.Ignore)
			{
				result = args.Result;
			}
			else
			{
				if (_seekBaseHandler.Pointer != IntPtr.Zero)
				{
					switch (args.Origin)
					{
						case SeekOrigin.Current:
							whence = RW_SEEK_CUR;
							break;
						case SeekOrigin.End:
							whence = RW_SEEK_END;
							break;
						default:
						case SeekOrigin.Begin:
							whence = RW_SEEK_SET;
							break;
					}

					result = _seekBaseHandler.Delegate.Invoke(
						args.Context, args.Offset, whence);
				}
				else if (BaseStream != null)
				{
					BaseStream.Seek(args.Offset, args.Origin);
				}
			}

			return result;
		}

		protected virtual void OnRead(RWopReadEventArgs e)
		{
			Reading?.Invoke(this, e);
		}

		private IntPtr OnReadCallback(
			IntPtr context,
			IntPtr ptr,
			IntPtr size,
			IntPtr maxnum)
		{
			IntPtr result = IntPtr.Zero;

			RWopReadEventArgs args = new RWopReadEventArgs();

			args.Context = context;
			args.Override = RWopEventOverrideMode.Continue;
			args.Data = ptr;
			args.Size = (uint)size.ToInt64();
			args.Count = (uint)maxnum.ToInt64();

			OnRead(args);

			if (args.Override == RWopEventOverrideMode.Ignore)
			{
				result = new IntPtr(args.Result);
			}
			else
			{
				if (_readBaseHandler.Pointer != IntPtr.Zero)
				{
					result = _readBaseHandler.Delegate.Invoke(
						args.Context,
						args.Data,
						new IntPtr(args.Size),
						new IntPtr(args.Count));
				}
				else if (BaseStream != null)
				{
					result = IntPtr.Zero;

					byte[] buffer = new byte[args.Size * args.Count];
					int numRead = BaseStream.Read(buffer, 0, buffer.Length);

					if (numRead > 0)
					{
						Marshal.Copy(buffer, 0, ptr, numRead);
						result = new IntPtr(numRead);
					}
				}
			}

			return result;
		}

		protected virtual void OnWrite(RWopWriteEventArgs e)
		{
			Writing?.Invoke(this, e);
		}

		private IntPtr OnWriteCallback(
			IntPtr context,
			IntPtr ptr,
			IntPtr size,
			IntPtr maxnum)
		{
			IntPtr result = IntPtr.Zero;

			RWopWriteEventArgs args = new RWopWriteEventArgs();

			args.Context = context;
			args.Override = RWopEventOverrideMode.Continue;
			args.Data = ptr;
			args.Size = (uint)size.ToInt64();
			args.Count = (uint)maxnum.ToInt64();

			OnWrite(args);

			if (args.Override == RWopEventOverrideMode.Ignore)
			{
				result = new IntPtr(args.Result);
			}
			else
			{
				if (_writeBaseHandler.Pointer != IntPtr.Zero)
				{
					result = _writeBaseHandler.Delegate.Invoke(
						context,
						ptr,
						new IntPtr(args.Size),
						new IntPtr(args.Count));
				}
				else if (BaseStream != null)
				{
					byte[] buffer = new byte[args.Size * args.Count];
					Marshal.Copy(ptr, buffer, 0, buffer.Length);
					BaseStream.Write(buffer, 0, buffer.Length);
					result = new IntPtr(buffer.Length);
				}
			}

			return result;
		}

		private void SetupFunctionPointers()
		{
			IntPtr closePtr = IntPtr.Zero;
			IntPtr sizePtr = IntPtr.Zero;
			IntPtr seekPtr = IntPtr.Zero;
			IntPtr readPtr = IntPtr.Zero;
			IntPtr writePtr = IntPtr.Zero;

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_RWops* ops = (SDL_RWops*)Handle.ToPointer();

				closePtr = ops->close;
				sizePtr = ops->size;
				seekPtr = ops->seek;
				readPtr = ops->read;
				writePtr = ops->write;
			}
#else
			SDL_RWops ops =
				Marshal.PtrToStructure<SDL_RWops>(Handle);

			closePtr = ops.close;
			sizePtr = ops.size;
			seekPtr = ops.seek;
			readPtr = ops.read;
			writePtr = ops.write;
#endif

			if (closePtr != IntPtr.Zero)
			{
				_closeBaseHandler = new RWopFunction<SDLRWopsCloseCallback>();
				_closeBaseHandler.Delegate =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsCloseCallback>(
						closePtr);
				_closeBaseHandler.Pointer = closePtr;
			}

			if (sizePtr != IntPtr.Zero)
			{
				_sizeBaseHandler = new RWopFunction<SDLRWopsSizeCallback>();
				_sizeBaseHandler.Delegate =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsSizeCallback>(
						sizePtr);
				_sizeBaseHandler.Pointer = sizePtr;
			}

			if (seekPtr != IntPtr.Zero)
			{
				_seekBaseHandler = new RWopFunction<SDLRWopsSeekCallback>();
				_seekBaseHandler.Delegate =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsSeekCallback>(
						seekPtr);
				_seekBaseHandler.Pointer = seekPtr;
			}

			if (readPtr != IntPtr.Zero)
			{
				_readBaseHandler = new RWopFunction<SDLRWopsReadCallback>();
				_readBaseHandler.Delegate =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsReadCallback>(
						readPtr);
				_readBaseHandler.Pointer = readPtr;
			}

			if (writePtr != IntPtr.Zero)
			{
				_writeBaseHandler = new RWopFunction<SDLRWopsWriteCallback>();
				_writeBaseHandler.Delegate =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsWriteCallback>(
						writePtr);
				_writeBaseHandler.Pointer = writePtr;
			}
		}

		private RWopFunction<T> SetupCallback<T>(T method)
		{
			RWopFunction<T> result = default(RWopFunction<T>);

			result.Delegate = method;
			result.Handle = GCHandle.Alloc(result.Delegate);
			result.Pointer = Marshal.GetFunctionPointerForDelegate<T>(method);

			return result;
		}

		private void SetupCallbacks()
		{
			SetupFunctionPointers();

			_closeHandler = SetupCallback<SDLRWopsCloseCallback>(
				OnCloseCallback);
			_sizeHandler = SetupCallback<SDLRWopsSizeCallback>(
				OnSizeCallback);
			_seekHandler = SetupCallback<SDLRWopsSeekCallback>(
				OnSeekCallback);
			_readHandler = SetupCallback<SDLRWopsReadCallback>(
				OnReadCallback);
			_writeHandler = SetupCallback<SDLRWopsWriteCallback>(
				OnWriteCallback);

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_RWops* ops = (SDL_RWops*)Handle.ToPointer();

				ops->close = _closeHandler.Pointer;
				ops->size = _sizeHandler.Pointer;
				ops->seek = _seekHandler.Pointer;
				ops->read = _readHandler.Pointer;
				ops->write = _writeHandler.Pointer;
			}
#else
			SDL_RWops ops =
				Marshal.PtrToStructure<SDL_RWops>(Handle);

			ops.close = _closeHandler.Pointer;
			ops.size = _sizeHandler.Pointer;
			ops.seek = _seekHandler.Pointer;
			ops.read = _readHandler.Pointer;
			ops.write = _writeHandler.Pointer;

			Marshal.StructureToPtr<SDL_RWops>(ops, Handle, false);
#endif
		}

		private void CleanupCallback<T>(ref RWopFunction<T> func)
		{
			if (func.Delegate != null)
			{
				func.Handle.Free();
				func.Delegate = default(T);
				func.Pointer = IntPtr.Zero;
			}
		}

		private void CleanupCallbacks()
		{
			CleanupCallback(ref _closeHandler);
			CleanupCallback(ref _sizeHandler);
			CleanupCallback(ref _seekHandler);
			CleanupCallback(ref _readHandler);
			CleanupCallback(ref _writeHandler);
		}

		public override void Flush()
		{
			if (BaseStream == null)
			{
				throw new NotImplementedException();
			}

			BaseStream?.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			int whence = RW_SEEK_SET;

			switch (origin)
			{
				case SeekOrigin.Begin:
					whence = RW_SEEK_SET;
					break;
				case SeekOrigin.Current:
					whence = RW_SEEK_CUR;
					break;
				case SeekOrigin.End:
					whence = RW_SEEK_END;
					break;
			}

			long result = SDL_RWseek(Handle, offset, whence);

			if (result == -1)
			{
				throw new SDLException();
			}

			return result;
		}

		public override void SetLength(long value)
		{
			if (BaseStream == null)
			{
				throw new NotImplementedException();
			}

			BaseStream.SetLength(value);
		}

		public byte ReadUInt8()
		{
			return SDL_ReadU8(Handle);
		}

		public ushort ReadUInt16()
		{
			return SDL_ReadLE16(Handle);
		}

		public ushort ReadUInt16Big()
		{
			return SDL_ReadBE16(Handle);
		}

		public uint ReadUInt32()
		{
			return SDL_ReadLE32(Handle);
		}

		public uint ReadUInt32Big()
		{
			return SDL_ReadBE16(Handle);
		}

		public ulong ReadUInt64()
		{
			return SDL_ReadLE64(Handle);
		}

		public ulong ReadUInt64Big()
		{
			return SDL_ReadBE64(Handle);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int result = 0;

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				fixed (byte* pBuffer = &buffer[offset])
				{
					result = (int)SDL_RWread(
						Handle,
						new IntPtr(pBuffer),
						new IntPtr(count),
						new IntPtr(1));
				}
			}
#else
			byte[] temp = new byte[count];

			GCHandle tempHandle = GCHandle.Alloc(temp, GCHandleType.Pinned);

			try
			{
				IntPtr tempPtr = tempHandle.AddrOfPinnedObject();
				result = (int)SDL_RWread(Handle, tempPtr, (uint)count, 1);
			}
			finally
			{
				tempHandle.Free();
			}

			Array.Copy(temp, 0, buffer, offset, count);
#endif

			return result;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				fixed (byte* pBuffer = &buffer[offset])
				{
					SDL_RWwrite(
						Handle,
						new IntPtr(pBuffer),
						new IntPtr(count),
						new IntPtr(1));
				}
			}
#else
			byte[] temp = new byte[count];

			Array.Copy(buffer, offset, temp, 0, count);

			GCHandle tempHandle = GCHandle.Alloc(temp, GCHandleType.Pinned);

			try
			{
				IntPtr tempPtr = tempHandle.AddrOfPinnedObject();
				SDL_RWwrite(Handle, tempPtr, (uint)count, 1);
			}
			finally
			{
				tempHandle.Free();
			}
#endif
		}

		public override void Close()
		{
			if (Handle != IntPtr.Zero)
			{
				if (SDL_RWclose(Handle) != 0)
				{
					throw new SDLException();
				}

				Handle = IntPtr.Zero;
			}

			if (Memory != IntPtr.Zero)
			{
				if (FreeMemory)
				{
					GarbageHandle.Free();
				}

				Memory = IntPtr.Zero;
			}

			if (BaseStream != null)
			{
				BaseStream.Close();
				BaseStream = null;
			}
			
			CleanupCallbacks();
		}

		public static implicit operator IntPtr(ReadWriteOperation RWops)
		{
			return RWops.Handle;
		}

		public static implicit operator bool(ReadWriteOperation RWops)
		{
			return (RWops != null && RWops.Handle != IntPtr.Zero);
		}

#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected override void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				Close();

				disposedValue = true;
			}
		}
		
		~ReadWriteOperation()
		{
		   Dispose(false);
		}
		
		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
#endregion
	}
}
