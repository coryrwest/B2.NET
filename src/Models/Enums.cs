using System.Text.Json.Serialization;

namespace B2Net.Models {
	public enum ContentEncoding {
		gzip,
		compress,
		deflate,
		identity
	}

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RetentionMode {
		governance,
		compliance
	}

	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RetentionUnit {
		days,
		years
	}
}
