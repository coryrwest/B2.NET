using System;

namespace B2Net.Models {
	public class AuthorizationException : Exception {
		public AuthorizationException(string response) : base("There was an error during authorization. See inner exception for details.", new Exception(response)) { }
	}
}
