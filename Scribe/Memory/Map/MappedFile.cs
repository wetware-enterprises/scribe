using System.Diagnostics;

namespace Scribe.Memory.Map;

public record MappedFile {
	public nint Base;
	public nint Ceiling;
	public MappedFilePerms Perms;
	public nint Offset;
	public (ulong Major, ulong Minor) Device;
	public ulong Node;
	public string PathName = string.Empty;

	public static IEnumerable<MappedFile> GetAll(Process process) {
		using var file = File.OpenRead($"/proc/{process.Id}/maps");
		using var reader = new StreamReader(file);

		while (reader.ReadLine() is string str) {
			var mapping = new MappedFile();
			
			mapping.Base = (nint)Convert.ToUInt64(TakeUntil(str, '-', out str), 16);
			mapping.Ceiling = (nint)Convert.ToUInt64(TakeUntil(str, ' ', out str), 16);
			for (var i = 0; i < 4; i++) {
				if (str[i] == '-') continue;
				mapping.Perms |= (MappedFilePerms)(1 << i);
			}
			mapping.Offset = (nint)Convert.ToUInt64(TakeUntil(str[5..], ' ', out str), 16);
			mapping.Device = (
				Major: Convert.ToUInt64(TakeUntil(str, ':', out str), 16),
				Minor: Convert.ToUInt64(TakeUntil(str, ' ', out str), 16)
			);
			mapping.Node = Convert.ToUInt64(TakeUntil(str, ' ', out str), 16);
			mapping.PathName = str.Trim();
			
			yield return mapping;
		}
	}
	
	private static string TakeUntil(string str, char value, out string remainder) {
		var index = str.IndexOf(value);
		remainder = str[(index + 1)..];
		return str[..index];
	}
}