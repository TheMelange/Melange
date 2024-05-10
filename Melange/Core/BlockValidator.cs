using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Core
{
    public class BlockValidator
    {
        public string ValidatorAddress { get; set; }

        public string Signature { get; set; }

        public bool IsValid { get; set; }

        public BlockValidator(string validatorAddress, string signature, bool isValid)
        {
            ValidatorAddress = validatorAddress;
            Signature = signature;
            IsValid = isValid;
        }

    }
}
