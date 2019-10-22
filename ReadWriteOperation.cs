using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SDL2;
using static SDL2.SDL;

namespace SDLWrapper
{
	// This class might be rather pointless in light of
	// System.IO.Stream but I added it for the sake of completeness
	// and for compatibility with native SDL functions that might
	// expect an SDL_RWops stream.
	public class ReadWriteOperation
		: Stream, IDisposable
	{
		IntPtr _closePtr;
		SDLRWopsCloseCallback _close;
		GCHandle _closeHandle;

		IntPtr _sizePtr;
		SDLRWopsSizeCallback _size;
		GCHandle _sizeHandle;

		IntPtr _seekPtr;
		SDLRWopsSeekCallback _seek;
		GCHandle _seekHandle;

		IntPtr _readPtr;
		SDLRWopsReadCallback _read;
		GCHandle _readHandle;

		IntPtr _writePtr;
		SDLRWopsWriteCallback _write;
		GCHandle _writeHandle;

		private bool _ownCallbacks;

		public ReadWriteOperation()
		{
			Initialize();

			Handle = SDL_AllocRW();

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			SetupCallbacks();
		}

		public ReadWriteOperation(string fileName, string fileMode = "r")
		{
			Initialize();

			Handle = SDL_RWFromFile(fileName, fileMode);

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			SetupFunctionPointers();
		}

		public ReadWriteOperation(byte[] data, bool @readonly = false)
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

				SetupFunctionPointers();
			}
		}
		
		public ReadWriteOperation(
			IntPtr filePointer,
			bool autoClose = false)
		{
			Initialize();

			Handle = SDL_RWFromFP(filePointer, autoClose.ToSDLBool());

			if (Handle == IntPtr.Zero)
			{
				throw new SDLException();
			}

			SetupFunctionPointers();
		}

		public ReadWriteOperation(
			IntPtr data,
			int length,
			bool @readonly = false)
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

				SetupFunctionPointers();
			}
		}

		private void Initialize()
		{
			Handle = IntPtr.Zero;
			Memory = IntPtr.Zero;
			FreeMemory = false;

			_close = null;
			_closePtr = IntPtr.Zero;
			
			_size = null;
			_sizePtr = IntPtr.Zero;

			_seek = null;
			_seekPtr = IntPtr.Zero;

			_read = null;
			_readPtr = IntPtr.Zero;

			_write = null;
			_writePtr = IntPtr.Zero;
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
		
		public override bool CanRead => throw new NotImplementedException();

		public override bool CanSeek => throw new NotImplementedException();

		public override bool CanWrite => throw new NotImplementedException();

		public override long Length => _size(Handle);

#if !SAFE_AS_POSSIBLE
		public unsafe ReadWriteOperation(
			byte* data,
			int length,
			bool @readonly = false)
			: this(new IntPtr(data), length, @readonly)
		{
		}
#endif

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

		private void SetupCallback<T>(
			T func,
			out GCHandle handle,
			out IntPtr ptr)
		{
			handle = GCHandle.Alloc(func);
			ptr = Marshal.GetFunctionPointerForDelegate<T>(func);
		}

		protected virtual int OnCloseCallback(IntPtr context)
		{
			return 0;
		}

		protected virtual long OnSizeCallback(IntPtr context)
		{
			return 0;
		}

		protected virtual long OnSeekCallback(
			IntPtr context,
			long offset,
			int whence)
		{
			return 0;
		}

		protected virtual uint OnReadCallback(
			IntPtr context,
			IntPtr ptr,
			uint size,
			uint maxnum)
		{
			return 0;
		}

		protected virtual uint OnWriteCallback(
			IntPtr context,
			IntPtr ptr,
			uint size,
			uint maxnum)
		{
			return 0;
		}

		private void SetupFunctionPointers()
		{
#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_RWops* ops = (SDL_RWops*)Handle.ToPointer();

				_closePtr = ops->close;
				_sizePtr = ops->size;
				_seekPtr = ops->seek;
				_readPtr = ops->read;
				_writePtr = ops->write;
			}
#else
			SDL_RWops ops =
				Marshal.PtrToStructure<SDL_RWops>(Handle);

			_closePtr = ops.close;
			_sizePtr = ops.size;
			_seekPtr = ops.seek;
			_readPtr = ops.read;
			_writePtr = ops.write;
#endif

			if (_closePtr != IntPtr.Zero)
			{
				_close =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsCloseCallback>(
						_closePtr);
			}

			if (_sizePtr != IntPtr.Zero)
			{
				_size =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsSizeCallback>(
						_sizePtr);
			}

			if (_seekPtr != IntPtr.Zero)
			{
				_seek =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsSeekCallback>(
						_seekPtr);
			}

			if (_readPtr != IntPtr.Zero)
			{
				_read =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsReadCallback>(
						_closePtr);
			}

			if (_writePtr != IntPtr.Zero)
			{
				_write =
					Marshal.GetDelegateForFunctionPointer
					<SDLRWopsWriteCallback>(
						_writePtr);
			}

			_ownCallbacks = false;
		}

		private void SetupCallbacks()
		{
			_close = new SDLRWopsCloseCallback(OnCloseCallback);
			SetupCallback(_close, out _closeHandle, out _closePtr);

			_size = new SDLRWopsSizeCallback(OnSizeCallback);
			SetupCallback(_size, out _sizeHandle, out _sizePtr);

			_seek = new SDLRWopsSeekCallback(OnSeekCallback);
			SetupCallback(_seek, out _seekHandle, out _seekPtr);

			_read = new SDLRWopsReadCallback(OnReadCallback);
			SetupCallback(_read, out _readHandle, out _readPtr);

			_write = new SDLRWopsWriteCallback(OnWriteCallback);
			SetupCallback(_write, out _writeHandle, out _writePtr);

#if !SAFE_AS_POSSIBLE
			unsafe
			{
				SDL_RWops* ops = (SDL_RWops*)Handle.ToPointer();

				ops->close = _closePtr;
				ops->size = _sizePtr;
				ops->seek = _seekPtr;
				ops->read = _readPtr;
				ops->write = _writePtr;
			}
#else
			SDL_RWops ops =
				Marshal.PtrToStructure<SDL_RWops>(Handle);

			ops.close = _closePtr;
			ops.size = _sizePtr;
			ops.seek = _seekPtr;
			ops.read = _readPtr;
			ops.write = _writePtr;

			Marshal.StructureToPtr<SDL_RWops>(ops, Handle, false);
#endif

			_ownCallbacks = true;
		}

		public override void Flush()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
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
						Handle, new IntPtr(pBuffer), (uint)count, 1);
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
					SDL_RWwrite(Handle, new IntPtr(pBuffer), (uint)count, 1);
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

			if (_close != null)
			{
				if (_ownCallbacks)
				{
					_closeHandle.Free();
				}

				_close = null;
			}

			if (_size != null)
			{
				if (_ownCallbacks)
				{
					_sizeHandle.Free();
				}

				_size = null;
			}

			if (_read != null)
			{
				if (_ownCallbacks)
				{
					_readHandle.Free();
				}

				_read = null;
			}

			if (_write != null)
			{
				if (_ownCallbacks)
				{
					_writeHandle.Free();
				}

				_write = null;
			}

			if (_seek != null)
			{
				if (_ownCallbacks)
				{
					_seekHandle.Free();
				}

				_seek = null;
			}
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
