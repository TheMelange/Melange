using Melange.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melange.Commands
{
    public static class CreateWalletCommand
    {

        public static void Execute(string input)
        {
            Wallet wallet = new Wallet();
            wallet.CreateNewWallet();
        }

    }
}
