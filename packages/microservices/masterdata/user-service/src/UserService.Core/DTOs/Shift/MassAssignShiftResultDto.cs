using System;
using System.Collections.Generic;

namespace UserService.Core.DTOs.Shift
{
    public class MassAssignShiftResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int TotalUsersProcessed { get; set; }
        public int UsersAssigned { get; set; }
        public int UsersFailed { get; set; }
        public List<string> FailedUserIds { get; set; } = new List<string>();
        public Dictionary<string, string> FailedUserMessages { get; set; } = new Dictionary<string, string>();
    }
}
