using System.Diagnostics.CodeAnalysis;
using Scribe.Memory.Mono.Structs;

namespace Scribe.Memory.Reader.Types;

public interface IMemoryReader : IDisposable {
	public bool TryReadBuffer(nint address, int size, out byte[] buffer);
	public bool TryRead<T>(nint address, out T result) where T : unmanaged;
	public bool TryReadPtr(nint address, out nint result);
	public bool TryReadString(nint address, [NotNullWhen(true)] out string? result);
	
	public T Read<T>(nint address) where T : unmanaged;
	public string? ReadString(nint address);

	public MonoClass GetClass(string nameSpace, string name);
	public nint FindClassInstance(MonoClass monoClass, Func<nint, bool> validate);
}