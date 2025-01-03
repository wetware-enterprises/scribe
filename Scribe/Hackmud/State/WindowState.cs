using Scribe.Memory;
using Scribe.Memory.Mono.Structs;
using Scribe.Hackmud.Game;

namespace Scribe.Hackmud.State;

public sealed class WindowState {
	private uint _lastTail;
	
	public readonly List<string> Output = new();

	public void Update(MemoryReader reader, nint addr) {
		if (!reader.Read<Window>(addr, out var window))
			return;

		if (!reader.Read<MonoQueueBase>(window.Output, out var output))
			return;

		if (!reader.Read<MonoQueue>(output.Data, out var queue))
			return;

		if (this._lastTail == queue._tail)
			return;
		
		var count = (queue._tail < this._lastTail ? queue._size : 0) + queue._tail;
		for (var i = this._lastTail; i < count && queue._size > 0; i++) {
			var p = (nint)(queue._array + 0x20 + 8 * (i % queue._size));
			var str = reader.ReadUnchecked<MonoString>(p);
			this.Output.Add(str.Read(reader) ?? string.Empty);
		}
		this._lastTail = queue._tail;
	}
}