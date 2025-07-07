using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Users;

public class UserUpdateDto
{       public string? email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
}