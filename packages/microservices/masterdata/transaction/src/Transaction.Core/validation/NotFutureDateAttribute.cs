using System;
using System.ComponentModel.DataAnnotations;

namespace Transaction.Core.Validation
{
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public NotFutureDateAttribute()
        {
            ErrorMessage = "The {0} field cannot be a future date.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true; // Let RequiredAttribute handle null values
            }

            if (value is DateTime dateValue)
            {
                return dateValue <= DateTime.UtcNow;
            }

            return false;
        }
    }
}