using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.core.entities.validators
{
    public class ValidatorEntity
    {
        public String ValidatorAddress { get; set; }
        public bool IsValid { get; set; }

        public ValidatorEntity(String validatorAddress, bool isValid)
        {
            ValidatorAddress = validatorAddress;
            IsValid = isValid;
        }
    }
}
