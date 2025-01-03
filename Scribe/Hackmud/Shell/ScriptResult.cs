using System.Text;
using Scribe.Hackmud.State;

namespace Scribe.Hackmud.Shell;

public class ScriptResult {
	public string Command;
	public string Parameters;

	private readonly List<string> _data = new();

	public ScriptResult(string cmd, string param) {
		this.Command = cmd;
		this.Parameters = param;
	}
	
	// Process output
	
	public void Populate(WindowState window) {
		this._data.Clear();
		this._data.AddRange(window.Output);
	}
	
	// Get result
	
	public string GetRawData() => string.Join('\n', this._data);

	public string GetPlainText() {
		var builder = new StringBuilder();
		foreach (var token in this.ReadTokens())
			builder.Append(token.Value);
		return builder.ToString();
	}
	
	// Token parsing

	public IEnumerable<Token> ReadTokens() {
		var slice = this.GetRawData();
		return this.ReadTokensFrom(slice);
	}

	private IEnumerable<Token> ReadTokensFrom(string slice) {
		var tokens = this.BuildTokenList(slice);
		if (tokens.Count == 0) yield break;

		var i = 0;
		var reading = true;
		while (reading) {
			foreach (var result in Read()) {
				yield return result;
			}

			IEnumerable<Token> Read() {
				var token = tokens[i];
				var next = tokens[i + 1];

				var color = token.Type == TokenType.Open ? slice[(token.Index + 8)..(token.Index + 16)] : null;

				var start = token.Index + token.Type switch {
					TokenType.Open => 17,
					TokenType.Close => 8,
					_ => 0
				};
				
				yield return new Token {
					Value = slice[start..next.Index],
					Color = color
				};
				
				if (next.Type == TokenType.End) {
					reading = false;
					yield break;
				}

				i++;
			}
		}
	}

	private List<(TokenType Type, int Index)> BuildTokenList(string slice) {
		const string openToken = "<color=#";
		const string closeToken = "</color>";

		var list = new List<(TokenType Type, int Index)>();
		list.Add((TokenType.Start, 0));

		var index = 0;
		while (index != -1) {
			var inner = slice[index..];
			var open = inner.IndexOf(openToken, StringComparison.Ordinal);
			var close = inner.IndexOf(closeToken, StringComparison.Ordinal);

			if (open != -1) open += index;
			if (close != -1) close += index;
			
			if (open != -1 && (close == -1 || open < close))
				list.Add((TokenType.Open, open));
			else if (close != -1 && (open == -1 || close < open))
				list.Add((TokenType.Close, close));

			index = open != -1 ? (close != -1 ? Math.Min(open, close) : open) : close;
			if (index != -1) index += 1;
		}
		
		list.Add((TokenType.End, slice.Length));

		return list;
	}

	public struct Token {
		public string? Color;
		public string Value;
	}

	private enum TokenType {
		Start,
		Open,
		Close,
		End
	}
}