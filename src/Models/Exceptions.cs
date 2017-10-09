using System;

namespace B2Net.Models {
	public class AuthorizationException : Exception {
		public AuthorizationException(string response) : base("There was an error during authorization. See inner exception for details.", new Exception(response)) { }
	}

    public class B2Exception : Exception {
        public string Status { get; set; }
        public string Code { get; set; }
        public B2Exception(string code, string status, string message) : base(message) { Status = status; Code = Code; }
    }
}
