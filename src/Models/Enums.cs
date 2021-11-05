using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace B2Net.Models {
	public enum ContentEncoding {
		gzip,
		compress,
		deflate,
		identity
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum RetentionMode {
		governance,
		compliance
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum RetentionUnit {
		days,
		years
	}
}
