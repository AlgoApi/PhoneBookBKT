﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyFinder.ViewModel
{
    public static class RegistrationData
    {
        public static TaskCompletionSource<List<Monkey>> CompletionSource { get; set; }
    }
}
