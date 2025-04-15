using System.Text;

namespace Scribe.Memory.Reader;

public class FileReader : IDisposable {
	private readonly FileStream _stream;
	private readonly BinaryReader _br;
	
	public FileReader(
		FileStream stream,
		BinaryReader br
	) {
		this._stream = stream;
		this._br = br;
	}

	public static FileReader Open(string file) {
		var stream = File.OpenRead(file);
		var reader = new BinaryReader(stream);
		return new FileReader(stream, reader);
	}

	public bool CanSeek => this._stream.CanSeek;
	
	public long Position
	{
		get => this._stream.Position;
		set => this._stream.Position = value;
	}

	public void Seek(nint offset) => this._stream.Seek(offset, SeekOrigin.Current);
	
	public byte[] ReadBuffer(int size) => this._br.ReadBytes(size);
	
	public byte ReadByte() => this._br.ReadByte();
	public ushort ReadUInt16() => this._br.ReadUInt16();
	public uint ReadUInt32() => this._br.ReadUInt32();
	public ulong ReadUInt64() => this._br.ReadUInt64();

	public sbyte ReadSByte() => this._br.ReadSByte();
	public short ReadInt16() => this._br.ReadInt16();
	public int ReadInt32() => this._br.ReadInt32();
	public long ReadInt64() => this._br.ReadInt64();

	public string ReadCString() {
		var str = new StringBuilder();
	
		char c;
		while ((c = this._br.ReadChar()) != '\0') {
			str.Append(c);
		}
	
		return str.ToString();
	}

	public void Dispose() {
		this._br.Dispose();
		this._stream.Dispose();
		GC.SuppressFinalize(this);
	}
}