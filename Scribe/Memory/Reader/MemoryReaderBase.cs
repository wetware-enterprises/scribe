using System.Text;
using System.Diagnostics.CodeAnalysis;

using Scribe.Memory.Mono.Structs;
using Scribe.Memory.Reader.Types;

namespace Scribe.Memory.Reader;

public abstract class MemoryReaderBase : IMemoryReader {
	private const string MainAssembly = "Core";
	
	private readonly Dictionary<string, Dictionary<string, MonoClass>> _classMap = new();

	// Memory reading
	
	public abstract bool TryReadBuffer(nint address, int size, out byte[] buffer);
	
	public unsafe bool TryRead<T>(nint address, out T result) where T : unmanaged {
		if (this.TryReadBuffer(address, sizeof(T), out var buffer)) {
			fixed (void* data = buffer)
				result = *(T*)data;
			return true;
		}
		result = default;
		return false;
	}

	public bool TryReadPtr(nint address, out nint result)
		=> this.TryRead(address, out result);
	
	public bool TryReadString(nint address, [NotNullWhen(true)] out string? result) {
		if (address == nint.Zero) {
			result = null;
			return false;
		}

		var str = new StringBuilder();
		
		var cursor = address;
		while (true) {
			if (!this.TryRead<byte>(cursor++, out var val)) {
				result = null;
				return false;
			}
			if (val == 0) break;
			str.Append((char)val);
		}

		result = str.ToString();
		return true;
	}

	public T Read<T>(nint address) where T : unmanaged {
		this.TryRead<T>(address, out var result);
		return result;
	}

	public string? ReadString(nint address) {
		this.TryReadString(address, out var result);
		return result;
	}
	
	// Mono
	
	private bool IsClassMapPopulated => this._classMap.Count != 0;
	
	public MonoClass GetClass(string nameSpace, string name) {
		if (!this.IsClassMapPopulated)
			this.BuildClassMap();

		if (!this._classMap.TryGetValue(nameSpace, out var nameSpaceMap))
			throw new Exception($"No mapping for namespace: '{nameSpace}'");

		if (!nameSpaceMap.TryGetValue(name, out var monoClass))
			throw new Exception($"Unknown class '{name}' in '{nameSpace}'");

		return monoClass;
	}
	
	public abstract nint FindClassInstance(MonoClass monoClass, Func<nint, bool> validate);

	protected abstract bool TryResolveDomainPtr(out nint gDomainPtr);
	
	private void BuildClassMap() {
		this._classMap.Clear();

		if (!this.TryResolveDomainPtr(out var gDomainPtr))
			throw new Exception("Failed to scan for global domain pointer.");

		if (!this.TryRead<MonoDomain>(gDomainPtr, out var domain))
			throw new Exception("Failed to read mono domain.");

		MonoAssembly assembly;
		try {
			assembly = domain.Assemblies.GetEnumerator(this)
				.First(item => item.ReadName(this) == MainAssembly);
		} catch {
			throw new Exception($"Failed to find assembly: {MainAssembly}");
		}
		
		if (!assembly.TryReadImage(this, out var image))
			throw new Exception("Failed to read MonoImage from assembly.");
		
		var i = 0;
		foreach (var type in image.ReadTypeDefInfo(this)) {
			i++;
			
			var nameSpace = this.ReadString(image.StringHeap + type.Namespace) ?? string.Empty;
			if (!this._classMap.TryGetValue(nameSpace, out var nameSpaceMap))
				this._classMap.Add(nameSpace, nameSpaceMap = []);
			
			var name = this.ReadString(image.StringHeap + type.Name);
			if (name == null || nameSpaceMap.ContainsKey(name)) continue;

			var handle = (uint)(i | 0x02000000);
			var monoClass = image.FindClassDef(this, handle);
			if (monoClass != null) nameSpaceMap.Add(name, monoClass.Value);
		}
	}
	
	// IDisposable

	public abstract void Dispose();
}