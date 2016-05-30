﻿using System;

namespace CakewalkIoC.Exceptions {
    
    /// <summary>
    /// Exception thrown when something goes wrong when resolving dependencies.
    /// </summary>
    public class ResolveException : Exception {
        public ResolveException(string message) : base(message) { }
    }

}