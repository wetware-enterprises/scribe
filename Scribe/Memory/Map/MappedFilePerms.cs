namespace Scribe.Memory.Map;

[Flags]
public enum MappedFilePerms {
	Read = 1,
	Write = 2,
	Execute = 4,
	Private = 8
}